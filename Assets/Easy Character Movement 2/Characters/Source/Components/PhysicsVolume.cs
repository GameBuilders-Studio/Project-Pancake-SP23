using UnityEngine;

namespace EasyCharacterMovement
{
    /// <summary>
    /// Helper component used to define physics volumes like water, air, oil, etc.
    /// Characters will react according to this settings when inside this volume.
    /// </summary>

    [RequireComponent(typeof(BoxCollider))]
    public class PhysicsVolume : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Tooltip("Determines which PhysicsVolume takes precedence if they overlap (higher value == higher priority).")]
        [SerializeField]
        private int _priority;

        [Tooltip("Determines the amount of friction applied by the volume as Character using CharacterMovement moves through it.\n" +
                 "The higher this value, the harder it will feel to move through the volume.")]
        [SerializeField]
        private float _friction;

        [Tooltip("Determines the terminal velocity of Characters using CharacterMovement when falling.")]
        [SerializeField]
        private float _maxFallSpeed;

        [Tooltip("Determines if the volume contains a fluid, like water.")]
        [SerializeField]
        private bool _waterVolume;

        #endregion

        #region FIELDS

        private BoxCollider _collider;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// This volume collider (trigger).
        /// </summary>

        public BoxCollider boxCollider
        {
            get
            {
                if (_collider == null)
                    _collider = GetComponent<BoxCollider>();

                return _collider;
            }
        }

        /// <summary>
        /// Determines which PhysicsVolume takes precedence if they overlap (higher value == higher priority).
        /// </summary>

        public int priority
        {
            get => _priority;
            set => _priority = value;
        }

        /// <summary>
        /// Determines the amount of friction applied by the volume as Character's using CharacterMovement move through it.
        /// The higher this value, the harder it will feel to move through the volume.
        /// </summary>

        public float friction
        {
            get => _friction;
            set => _friction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Determines the terminal velocity of Character's using CharacterMovement when falling.
        /// </summary>

        public float maxFallSpeed
        {
            get => _maxFallSpeed;
            set => _maxFallSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Determines if the volume contains a fluid, like water.
        /// </summary>

        public bool waterVolume
        {
            get => _waterVolume;
            set => _waterVolume = value;
        }

        #endregion

        #region METHODS

        protected virtual void OnReset()
        {
            priority = 0;
            friction = 0.5f;
            maxFallSpeed = 40.0f;
            waterVolume = true;
        }

        protected virtual void OnOnValidate()
        {
            friction = _friction;
            maxFallSpeed = _maxFallSpeed;
        }

        protected virtual void OnAwake()
        {
            boxCollider.isTrigger = true;
        }

        #endregion

        #region MONOBEHAVIOUR

        private void Reset()
        {
            OnReset();
        }

        private void OnValidate()
        {
            OnOnValidate();
        }

        private void Awake()
        {
            OnAwake();
        }

        #endregion
    }
}
