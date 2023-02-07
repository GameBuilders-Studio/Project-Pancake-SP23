using UnityEngine;

namespace EasyCharacterMovement
{
    /// <summary>
    ///
    /// RootMotionController.
    /// 
    /// Helper component to get Animator root motion velocity vector (animRootMotionVelocity).
    /// 
    /// This must be attached to a game object with an Animator component.
    /// </summary>

    [RequireComponent(typeof(Animator))]
    public class RootMotionController : MonoBehaviour
    {
        #region FIELDS

        protected Animator _animator;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The animation root motion velocity vector.
        /// </summary>

        public virtual Vector3 animRootMotionVelocity { get; protected set; }

        /// <summary>
        /// The animation root motion delta rotation.
        /// </summary>

        public virtual Quaternion animDeltaRotation => _animator.deltaRotation;

        #endregion

        #region METHOD

        /// <summary>
        /// Calculate velocity from anim root motion.
        /// </summary>

        protected virtual Vector3 CalcAnimRootMotionVelocity()
        {
            float deltaTime = Time.deltaTime;

            if (deltaTime > 0.0f)
                return _animator.deltaPosition / deltaTime;

            return Vector3.zero;
        }

        #endregion

        #region MONOBEHAVIOUR

        public virtual void Awake()
        {
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError($"RootMotionController: There is no 'Animator' attached to the '{name}' game object.\n" +
                               $"Please attach a 'Animator' to the '{name}' game object");
            }
        }

        public virtual void OnAnimatorMove()
        {
            // Compute animation root motion velocity

            animRootMotionVelocity = CalcAnimRootMotionVelocity();
        }

        #endregion
    }
}