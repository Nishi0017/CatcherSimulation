using System;
using UnityEngine;
class TrajectoryChecks
{
    static void Require(bool ok, string message) { if (!ok) throw new Exception(message); }
    static float Simulate(Vector3 origin, Vector3 velocity, Vector3 target, float drag, out float arrivalSpeed)
    {
        Vector3 forward = Vector3.ProjectOnPlane(target - origin, Vector3.up).normalized;
        float distance = Vector3.Dot(target - origin, forward);
        Vector3 position = origin;
        arrivalSpeed = 0;
        for (int i = 0; i < 250; i++)
        {
            Vector3 previous = position;
            velocity += (new Vector3(0, -9.81f, 0) - drag * velocity.magnitude * velocity) * .02f;
            position += velocity * .02f;
            float travelled = Vector3.Dot(position - origin, forward);
            if (travelled >= distance)
            {
                float before = Vector3.Dot(previous - origin, forward);
                float fraction = (distance - before) / (travelled - before);
                arrivalSpeed = Vector3.Dot(velocity, forward);
                return Mathf.Lerp(previous.y, position.y, fraction);
            }
        }
        throw new Exception("Target plane not reached");
    }
    static void Main()
    {
        float drag = .5f * 1.225f * .35f * Mathf.PI * .0366f * .0366f / .145f;
        int count = 0;
        foreach (float distance in new[] { 5f, 12f, 16f, 18.44f })
        foreach (float kmh in new[] { 60f, 100f, 160f, 200f })
        foreach (float height in new[] { .7f, 1.3f, 2f })
        foreach (float coefficient in new[] { 0f, drag })
        {
            Vector3 origin = new Vector3(0, 1.8f, 0), target = new Vector3(2, height, distance);
            Require(BaseballTrajectory.TrySolve(origin, target, kmh / 3.6f, new Vector3(0, -9.81f, 0),
                coefficient, .02f, 5f, .03f, out Vector3 velocity, out float duration), $"Expected reachable target: distance={distance}, kmh={kmh}, height={height}, drag={coefficient}");
            Require(Math.Abs(velocity.magnitude - kmh / 3.6f) < .001f, "Initial speed changed");
            Require(duration > 0 && duration < 5, "Invalid duration");
            Require(Math.Abs(Simulate(origin, velocity, target, coefficient, out float arrival) - height) <= .031f,
                "Target error exceeds tolerance");
            if (coefficient > 0) Require(arrival < Vector3.Dot(velocity, Vector3.ProjectOnPlane(target - origin, Vector3.up).normalized), "Drag did not reduce speed");
            count++;
        }
        Require(!BaseballTrajectory.TrySolve(Vector3.zero, new Vector3(0, 100, 100), 1,
            new Vector3(0, -9.81f, 0), drag, .02f, 5, .03f, out _, out _), "Unreachable target accepted");
        Console.WriteLine("PASS: " + count + " trajectories; initial speeds, target heights, drag slowdown, unreachable target.");
    }
}


