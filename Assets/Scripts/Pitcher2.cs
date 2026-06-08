using UnityEngine;

public class Pitcher2 : MonoBehaviour
{
    public GameObject ball;
    public Transform target; // ボールを投げる目標位置
    public float speed; // ボールの初速度

    private Rigidbody rb;
    private const float gravity = 9.81f; // 重力加速度（m/s^2）
    private const float airResistanceCoefficient = 0.47f; // 空気抵抗係数
    private const float airDensity = 1.225f; // 空気の密度（kg/m^3）
    private const float ballRadius = 0.0366f; // ボールの半径（m）
    private const float ballMass = 0.145f; // ボールの質量（kg）

    void Start()
    {
        //rb = ball.GetComponent<Rigidbody>();
        //rb.useGravity = false;
        //ThrowBall();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            ball = Instantiate(ball, new Vector3(1.57f, 1.6f, 1.57f), Quaternion.identity);
            rb = ball.GetComponent<Rigidbody>();
            ThrowBall();
        }
    }

    void ThrowBall()
    {
        rb.useGravity = true;
        Vector3 targetPosition = target.position;
        Vector3 startPosition = transform.position;

        Vector3 direction = targetPosition - startPosition;
        float distance = direction.magnitude;

        // 目標位置への角度計算（放物運動）
        float angle = CalculateThrowingAngle(distance, speed);

        // 初速度の成分を計算
        Vector3 initialVelocity = CalculateInitialVelocity(direction, angle, speed);

        // ボールに初速度を設定
        rb.velocity = initialVelocity;
    }

    float CalculateThrowingAngle(float distance, float speed)
    {
        // 重力と速度から投擲角度を計算
        float angle = Mathf.Asin((distance * gravity) / (speed * speed)) / 2;
        return angle * Mathf.Rad2Deg;
    }

    Vector3 CalculateInitialVelocity(Vector3 direction, float angle, float speed)
    {
        // 方向ベクトルを正規化
        direction = direction.normalized;

        // 方向ベクトルを回転させて初速度ベクトルを計算
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.Cross(direction, Vector3.up));
        Vector3 initialVelocity = rotation * direction * speed;

        return initialVelocity;
    }

    void FixedUpdate()
    {
        ApplyAirResistance();
    }

    void ApplyAirResistance()
    {
        Vector3 velocity = rb.velocity;
        float velocityMagnitude = velocity.magnitude;

        // 空気抵抗の力を計算
        float dragForceMagnitude = 0.5f * airResistanceCoefficient * airDensity * Mathf.PI * Mathf.Pow(ballRadius, 2) * Mathf.Pow(velocityMagnitude, 2);
        Vector3 dragForce = dragForceMagnitude * -velocity.normalized;

        // 空気抵抗の力を剛体に適用
        rb.AddForce(dragForce, ForceMode.Force);
    }
}
