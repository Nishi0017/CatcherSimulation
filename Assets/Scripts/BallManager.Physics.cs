using UnityEngine;

public partial class BallManager
{
    [Header("DevelopScene physical pitch (1 unit = 1 metre)")]
    [SerializeField] bool enablePhysicalFlight = true;
    [SerializeField, Min(1f)] float minimumSpeedKmh = 60f;
    [SerializeField, Min(1f)] float maximumSpeedKmh = 160f;
    [SerializeField, Min(1f)] float specialSpeedKmh = 200f;
    [SerializeField, Min(0.001f)] float ballMassKg = 0.145f;
    [SerializeField, Min(0.001f)] float ballRadiusMetres = 0.0366f;
    [SerializeField, Min(0f)] float airDensity = 1.225f;
    [SerializeField, Min(0f)] float dragCoefficient = 0.35f;
    [SerializeField, Min(0.01f)] float targetTolerance = 0.03f;
    [SerializeField, Min(0.1f)] float missDistanceBehindTarget = 2f;
    [SerializeField, Min(0.1f)] float flightTimeout = 5f;
    [SerializeField] float groundHeight = 0f;

    Rigidbody flightBody;
    Vector3 previousBallPosition;
    Vector3 flightForward;
    Catcher[] catchers;
    bool UsePhysicalFlight => enablePhysicalFlight && gameObject.scene.name == "DevelopScene";
    float DragPerMass => 0.5f * airDensity * dragCoefficient * Mathf.PI
                         * ballRadiusMetres * ballRadiusMetres / ballMassKg;

    void PitchPhysical(bool special, float? testSpeedKmh)
    {
        if (ball != null) return;
        if (pitcherHand == null || targetArea_min == null || targetArea_max == null
            || (special ? ballPrefab2 : ballPrefab) == null)
        {
            Debug.LogError("Physical pitch requires hand, target bounds and ball prefab.", this);
            if (pithcer != null) pithcer.CanPitch = true;
            return;
        }
        throwSpeed = special ? specialSpeedKmh : testSpeedKmh ?? Random.Range(
            Mathf.Min(minimumSpeedKmh, maximumSpeedKmh), Mathf.Max(minimumSpeedKmh, maximumSpeedKmh));
        pitchPos = pitcherHand.position;
        Vector3 velocity = Vector3.zero;
        bool solved = false;
        float duration = 0f;
        for (int attempt = 0; attempt < 12 && !solved; attempt++)
        {
            Vector3 lo = Vector3.Min(targetArea_min.position, targetArea_max.position);
            Vector3 hi = Vector3.Max(targetArea_min.position, targetArea_max.position);
            targetPoint = new Vector3(Random.Range(lo.x, hi.x), Random.Range(lo.y, hi.y), Random.Range(lo.z, hi.z));
            solved = BaseballTrajectory.TrySolve(pitchPos, targetPoint, throwSpeed / 3.6f,
                Physics.gravity, DragPerMass, Time.fixedDeltaTime, flightTimeout,
                targetTolerance, out velocity, out duration);
        }
        if (!solved)
        {
            Debug.LogWarning("No reachable pitch target at this speed. Check distance, bounds and gravity.", this);
            if (pithcer != null) pithcer.CanPitch = true;
            if (displaySpeed != null) displaySpeed.UpdateSpeedText(Mathf.RoundToInt(throwSpeed));
            return;
        }
        ball = Instantiate(special ? ballPrefab2 : ballPrefab, pitchPos, pitcherHand.rotation);
        flightBody = ball.GetComponent<Rigidbody>();
        if (flightBody == null) flightBody = ball.AddComponent<Rigidbody>();
        flightBody.mass = ballMassKg;
        flightBody.drag = 0f;
        flightBody.useGravity = true;
        flightBody.isKinematic = false;
        flightBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        flightBody.interpolation = RigidbodyInterpolation.Interpolate;
        SphereCollider sphere = ball.GetComponent<SphereCollider>();
        if (sphere != null)
        {
            Vector3 scale = ball.transform.lossyScale;
            float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            if (largestScale > 0f) sphere.radius = ballRadiusMetres / largestScale;
        }
        flightBody.velocity = velocity;
        previousBallPosition = pitchPos;
        flightForward = Vector3.ProjectOnPlane(targetPoint - pitchPos, Vector3.up).normalized;
        throwTime = Time.time;
        catchedBall = false;
        catchers = FindObjectsOfType<Catcher>();
        if (batter != null) batter.SwingStart(duration);
    }

    void FixedUpdate()
    {
        if (!UsePhysicalFlight || ball == null || flightBody == null) return;
        Vector3 position = flightBody.position;
        // Sweep the completed physics step. Direct collider casts include trigger gloves.
        Vector3 segment = position - previousBallPosition;
        if (segment.sqrMagnitude > 0.000001f)
        {
            RaycastHit[] hits = Physics.SphereCastAll(previousBallPosition, ballRadiusMetres,
                segment.normalized, segment.magnitude, ~0, QueryTriggerInteraction.Collide);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit hit in hits)
            {
                Catcher catcher = hit.collider.GetComponentInParent<Catcher>();
                if (catcher != null && catcher.TryCatch(this, ball)) return;
            }
        }
        // Also handle initial overlap and a stationary glove containing the ball.
        foreach (Catcher catcher in catchers)
        {
            if (catcher == null || !catcher.isActiveAndEnabled) continue;
            foreach (Collider glove in catcher.GetComponentsInChildren<Collider>())
                if (glove.enabled && glove.isTrigger &&
                    (glove.ClosestPoint(position) - position).sqrMagnitude <= ballRadiusMetres * ballRadiusMetres
                    && catcher.TryCatch(this, ball)) return;
        }
        previousBallPosition = position;
        if (position.y <= groundHeight + ballRadiusMetres
            || Vector3.Dot(position - targetPoint, flightForward) >= missDistanceBehindTarget
            || Time.time - throwTime >= flightTimeout)
        {
            FinishPhysicalMiss();
            return;
        }
        Vector3 v = flightBody.velocity;
        flightBody.AddForce(-DragPerMass * v.magnitude * v, ForceMode.Acceleration);
    }

    public bool OwnsBall(GameObject candidate) => ball != null && candidate == ball;

    void FinishPhysicalMiss()
    {
        if (ball == null) return;
        if (audioSource != null && miss != null) audioSource.PlayOneShot(miss);
        Destroy(ball);
        ball = null;
        flightBody = null;
        if (pithcer != null) pithcer.CanPitch = true;
        if (displaySpeed != null) displaySpeed.UpdateSpeedText(Mathf.RoundToInt(throwSpeed));
    }
}

// Predicts the same semi-implicit gravity and quadratic drag steps used by Rigidbody.
public static class BaseballTrajectory
{
    public static bool TrySolve(Vector3 origin, Vector3 target, float speed, Vector3 gravity,
        float dragPerMass, float step, float timeout, float tolerance,
        out Vector3 velocity, out float duration)
    {
        velocity = Vector3.zero;
        duration = 0f;
        Vector3 horizontal = Vector3.ProjectOnPlane(target - origin, Vector3.up);
        float distance = horizontal.magnitude;
        if (distance < 0.01f || speed <= 0f || step <= 0f || timeout <= 0f) return false;
        // This angle solver assumes standard vertical gravity and still air.
        if (Mathf.Abs(gravity.x) > 0.0001f || Mathf.Abs(gravity.z) > 0.0001f) return false;
        Vector3 forward = horizontal / distance;
        bool hadLower = false;
        float lower = -80f;
        for (float angle = -80f; angle <= 80f; angle += 2f)
        {
            Vector3 candidate = Direction(forward, angle) * speed;
            if (!Crossing(origin, candidate, forward, distance, gravity, dragPerMass, step,
                timeout, out float height, out float time)) { hadLower = false; continue; }
            float error = height - target.y;
            if (Mathf.Abs(error) <= tolerance) { velocity = candidate; duration = time; return true; }
            if (error < 0f) { lower = angle; hadLower = true; continue; }
            if (!hadLower) continue;
            float upper = angle;
            for (int iteration = 0; iteration < 24; iteration++)
            {
                float middle = (lower + upper) * 0.5f;
                candidate = Direction(forward, middle) * speed;
                if (!Crossing(origin, candidate, forward, distance, gravity, dragPerMass,
                    step, timeout, out height, out time)) break;
                error = height - target.y;
                if (Mathf.Abs(error) <= tolerance) { velocity = candidate; duration = time; return true; }
                if (error < 0f) lower = middle; else upper = middle;
            }
            return false;
        }
        return false;
    }

    static Vector3 Direction(Vector3 forward, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return forward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians);
    }

    static bool Crossing(Vector3 origin, Vector3 velocity, Vector3 forward, float distance,
        Vector3 gravity, float drag, float step, float timeout, out float height, out float time)
    {
        Vector3 position = origin;
        height = 0f;
        time = 0f;
        for (int i = 0; i < Mathf.CeilToInt(timeout / step); i++)
        {
            Vector3 previous = position;
            velocity += (gravity - drag * velocity.magnitude * velocity) * step;
            position += velocity * step;
            float travelled = Vector3.Dot(position - origin, forward);
            if (travelled >= distance)
            {
                float before = Vector3.Dot(previous - origin, forward);
                float fraction = (distance - before) / (travelled - before);
                height = Mathf.Lerp(previous.y, position.y, fraction);
                time = (i + fraction) * step;
                return true;
            }
            if (Vector3.Dot(velocity, forward) <= 0f) break;
        }
        return false;
    }
}
