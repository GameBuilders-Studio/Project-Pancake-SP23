using UnityEngine;

namespace EasyCharacterMovement
{
    public static class MathLib
    {
        /// <summary>
        /// Return the square of the given value.
        /// </summary>

        public static float Square(float value)
        {
            return value * value;
        }

        /// <summary>
        /// Returns the direction adjusted to be tangent to a specified surface normal relatively to given up axis.
        /// </summary>
        
        public static Vector3 GetTangent(Vector3 direction, Vector3 normal, Vector3 up)
        {
            Vector3 right = direction.perpendicularTo(up);

            return normal.perpendicularTo(right);
        }

        /// <summary>
        /// Projects a given point onto the plane defined by plane origin and plane normal.
        /// </summary>

        public static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeOrigin, Vector3 planeNormal)
        {
            Vector3 toPoint = point - planeOrigin;
            Vector3 toPointProjected = Vector3.Project(toPoint, planeNormal);

            return point - toPointProjected;
        }

        /// <summary>
        /// Clamps the given angle into 0 - 360 degrees range.
        /// </summary>

        public static float Clamp0360(float eulerAngles)
        {
            float result = eulerAngles - Mathf.CeilToInt(eulerAngles / 360f) * 360f;
            if (result < 0) result += 360f;

            return result;
        }

        /// <summary>
        /// Returns a new rotation angle (interpolated) clamped in the range (0.0f , 360.0f)
        /// </summary>

        public static float FixedTurn(float current, float target, float maxDegreesDelta)
        {
            if (maxDegreesDelta == 0.0f)
                return Clamp0360(current);

            if (maxDegreesDelta >= 360.0f)
                return Clamp0360(target);

            float result = Clamp0360(current);
            current = result;
            target = Clamp0360(target);

            if (current > target)
            {
                if (current - target < 180.0f)
                    result -= Mathf.Min(current - target, Mathf.Abs(maxDegreesDelta));
                else
                    result += Mathf.Min(target + 360.0f - current, Mathf.Abs(maxDegreesDelta));
            }
            else
            {
                if (target - current < 180.0f)
                    result += Mathf.Min(target - current, Mathf.Abs(maxDegreesDelta));
                else
                    result -= Mathf.Min(current + 360.0f - target, Mathf.Abs(maxDegreesDelta));
            }

            return Clamp0360(result);
        }
    }
}