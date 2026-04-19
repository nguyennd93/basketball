using UnityEngine;

namespace Basketball.Utilities
{
    public static class BallisticArcUtility
    {
        public static bool TrySolveArc(
            Vector3 startPoint,
            Vector3 targetPoint,
            float arcHeight,
            float distanceArcMultiplier,
            Vector3 gravity,
            out Vector3 initialVelocity,
            out float totalTime)
        {
            initialVelocity = Vector3.zero;
            totalTime = 0f;

            float gravityY = gravity.y;
            if (gravityY >= -Mathf.Epsilon)
            {
                return false;
            }

            Vector3 displacement = targetPoint - startPoint;
            Vector3 displacementXZ = new Vector3(displacement.x, 0f, displacement.z);
            float horizontalDistance = displacementXZ.magnitude;
            float apexHeight = Mathf.Max(startPoint.y, targetPoint.y) + arcHeight + horizontalDistance * distanceArcMultiplier;
            float ascent = apexHeight - startPoint.y;
            float descent = apexHeight - targetPoint.y;

            if (ascent <= 0f || descent < 0f)
            {
                return false;
            }

            float timeUp = Mathf.Sqrt(2f * ascent / -gravityY);
            float timeDown = Mathf.Sqrt(2f * descent / -gravityY);
            totalTime = timeUp + timeDown;

            if (totalTime <= Mathf.Epsilon)
            {
                return false;
            }

            Vector3 velocityXZ = displacementXZ / totalTime;
            float velocityY = Mathf.Sqrt(2f * -gravityY * ascent);
            initialVelocity = velocityXZ + Vector3.up * velocityY;
            return true;
        }

        public static Vector3 EvaluatePosition(Vector3 startPoint, Vector3 initialVelocity, Vector3 gravity, float time)
        {
            return startPoint + initialVelocity * time + 0.5f * gravity * time * time;
        }
    }
}
