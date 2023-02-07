using UnityEngine;

namespace EasyCharacterMovement
{
    /// <summary>
    /// The slope behavior for attached collider.
    /// </summary>

    public enum SlopeBehaviour
    {
        Default,

        /// <summary>
        /// Sets the collider as walkable.
        /// </summary>

        Walkable,

        /// <summary>
        /// Sets the collider as not walkable.
        /// </summary>

        NotWalkable,

        /// <summary>
        /// Let you specify a custom slope limit value for collider.
        /// </summary>

        Override
    }

    /// <summary>
    /// Overrides a CharacterMovement SlopeLimit property allowing to define per-object behavior instead of per face.
    /// This enable you to tweak what surfaces Characters can walk up. Perhaps a stair case is too steep or
    /// maybe you want to enforce the "no walking on the grass" signs, these settings will enable you to do so. 
    /// </summary>

    public class SlopeLimitBehavior : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Tooltip("The desired behavior.")]
        [SerializeField]
        private SlopeBehaviour _slopeBehaviour = SlopeBehaviour.Default;

        [SerializeField]
        private float _slopeLimit;

        #endregion

        #region FIELDS

        [SerializeField, HideInInspector]
        private float _slopeLimitCos;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The current behavior.
        /// </summary>

        public SlopeBehaviour walkableSlopeBehaviour
        {
            get => _slopeBehaviour;
            set => _slopeBehaviour = value;
        }

        /// <summary>
        /// The slope limit angle in degrees.
        /// </summary>

        public float slopeLimit
        {
            get => _slopeLimit;

            set
            {
                _slopeLimit = Mathf.Clamp(value, 0.0f, 89.0f);
                
                _slopeLimitCos = Mathf.Cos(_slopeLimit * Mathf.Deg2Rad);
            }
        }

        /// <summary>
        /// The cosine of slope angle (in radians), this is used to faster angle tests (e.g. dotProduct > slopeLimitCos)
        /// </summary>

        public float slopeLimitCos
        {
            get => _slopeLimitCos;

            set
            {
                _slopeLimitCos = Mathf.Clamp01(value);

                _slopeLimit = Mathf.Clamp(Mathf.Acos(_slopeLimitCos) * Mathf.Rad2Deg, 0.0f, 89.0f);
            }
        }

        #endregion

        #region MONOBEHAVIOUR

        private void OnValidate()
        {
            slopeLimit = _slopeLimit;
        }

        #endregion
    }
}
