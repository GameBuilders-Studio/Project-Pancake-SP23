using UnityEngine;

namespace EasyCharacterMovement
{
    public static class Extensions
    {
        /// <summary>
        /// Return the square of the given value.
        /// </summary>

        public static int square(this int value)
        {
            return value * value;
        }

        /// <summary>
        /// Return the square of the given value.
        /// </summary>

        public static float square(this float value)
        {
            return value * value;
        }

        /// <summary>
        /// Checks whether value is near to zero within a tolerance.
        /// </summary>

        public static bool isZero(this float value)
        {
            const float kTolerance = 0.0000000001f;

            return Mathf.Abs(value) < kTolerance;
        }

        /// <summary>
        /// Returns a copy of given vector with only X component of the vector.
        /// </summary>

        public static Vector3 onlyX(this Vector3 vector3)
        {
            vector3.y = 0.0f;
            vector3.z = 0.0f;

            return vector3;
        }

        /// <summary>
        /// Returns a copy of given vector with only Y component of the vector.
        /// </summary>

        public static Vector3 onlyY(this Vector3 vector3)
        {
            vector3.x = 0.0f;
            vector3.z = 0.0f;

            return vector3;
        }

        /// <summary>
        /// Returns a copy of given vector with only Z component of the vector.
        /// </summary>

        public static Vector3 onlyZ(this Vector3 vector3)
        {
            vector3.x = 0.0f;
            vector3.y = 0.0f;

            return vector3;
        }

        /// <summary>
        /// Returns a copy of given vector with only X and Z components of the vector.
        /// </summary>

        public static Vector3 onlyXZ(this Vector3 vector3)
        {
            vector3.y = 0.0f;

            return vector3;
        }

        /// <summary>
        /// Checks whether vector is near to zero within a tolerance.
        /// </summary>

        public static bool isZero(this Vector2 vector2)
        {
            return vector2.sqrMagnitude < 9.99999943962493E-11;
        }

        /// <summary>
        /// Checks whether vector is near to zero within a tolerance.
        /// </summary>

        public static bool isZero(this Vector3 vector3)
        {
            return vector3.sqrMagnitude < 9.99999943962493E-11;
        }

        /// <summary>
        /// Checks whether vector is exceeding the magnitude within a small error tolerance.
        /// </summary>

        public static bool isExceeding(this Vector3 vector3, float magnitude)
        {
            // Allow 1% error tolerance, to account for numeric imprecision.

            const float kErrorTolerance = 1.01f;

            return vector3.sqrMagnitude > magnitude * magnitude * kErrorTolerance;
        }

        /// <summary>
        /// Returns a copy of given vector with a magnitude of 1,
        /// and outs its magnitude before normalization.
        /// 
        /// If the vector is too small to be normalized a zero vector will be returned.
        /// </summary>

        public static Vector3 normalized(this Vector3 vector3, out float magnitude)
        {
            magnitude = vector3.magnitude;
            if (magnitude > 9.99999974737875E-06)
                return vector3 / magnitude;

            magnitude = 0.0f;

            return Vector3.zero;
        }

        /// <summary>
        /// Dot product of two vectors.
        /// </summary>        
        
        public static float dot(this Vector3 vector3, Vector3 otherVector3)
        {
            return Vector3.Dot(vector3, otherVector3);
        }

        /// <summary>
        /// Returns a copy of given vector projected onto normal vector.
        /// </summary>

        public static Vector3 projectedOn(this Vector3 thisVector, Vector3 normal)
        {
            return Vector3.Project(thisVector, normal);
        }

        /// <summary>
        /// Returns a copy of given vector projected onto a plane defined by a normal orthogonal to the plane.
        /// </summary>

        public static Vector3 projectedOnPlane(this Vector3 thisVector, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(thisVector, planeNormal);
        }

        /// <summary>
        /// Returns a copy of given vector with its magnitude clamped to maxLength.
        /// </summary>

        public static Vector3 clampedTo(this Vector3 vector3, float maxLength)
        {
            return Vector3.ClampMagnitude(vector3, maxLength);
        }

        /// <summary>
        /// Returns a copy of given vector perpendicular to other vector.
        /// </summary>

        public static Vector3 perpendicularTo(this Vector3 thisVector, Vector3 otherVector)
        {
            return Vector3.Cross(thisVector, otherVector).normalized;
        }

        /// <summary>
        /// Returns a copy of given vector adjusted to be tangent to a specified surface normal relatively to given up axis.
        /// </summary>

        public static Vector3 tangentTo(this Vector3 thisVector, Vector3 normal, Vector3 up)
        {
            Vector3 r = thisVector.perpendicularTo(up);
            Vector3 t = normal.perpendicularTo(r);

            return t * thisVector.magnitude;
        }

        /// <summary>
        /// Transforms a vector to be relative to given transform.
        /// If isPlanar == true, the transform will be applied on the plane defined by world up axis.
        /// </summary>

        public static Vector3 relativeTo(this Vector3 vector3, Transform relativeToThis, bool isPlanar = true)
        {
            Vector3 forward = relativeToThis.forward;

            if (isPlanar)
            {
                Vector3 upAxis = Vector3.up;
                forward = forward.projectedOnPlane(upAxis);

                if (forward.isZero())
                    forward = Vector3.ProjectOnPlane(relativeToThis.up, upAxis);
            }
            
            Quaternion q = Quaternion.LookRotation(forward);

            return q * vector3;
        }

        /// <summary>
        /// Transforms a vector to be relative to given transform.
        /// If isPlanar == true, the transform will be applied on the plane defined by upAxis.
        /// </summary>

        public static Vector3 relativeTo(this Vector3 vector3, Transform relativeToThis, Vector3 upAxis, bool isPlanar = true)
        {
            Vector3 forward = relativeToThis.forward;

            if (isPlanar)
            {
                forward = Vector3.ProjectOnPlane(forward, upAxis);

                if (forward.isZero())
                    forward = Vector3.ProjectOnPlane(relativeToThis.up, upAxis);
            }

            Quaternion q = Quaternion.LookRotation(forward, upAxis);

            return q * vector3;
        }

        /// <summary>
        /// Clamps the given quaternion pitch rotation between the given minPitchAngle and maxPitchAngle.
        /// </summary>

        public static Quaternion clampPitch(this Quaternion quaternion, float minPitchAngle, float maxPitchAngle)
        {
            quaternion.x /= quaternion.w;
            quaternion.y /= quaternion.w;
            quaternion.z /= quaternion.w;
            quaternion.w = 1.0f;

            float pitch = Mathf.Clamp(2.0f * Mathf.Rad2Deg * Mathf.Atan(quaternion.x), minPitchAngle, maxPitchAngle);

            quaternion.x = Mathf.Tan(pitch * 0.5f * Mathf.Deg2Rad);

            return quaternion;
        }
    }
}