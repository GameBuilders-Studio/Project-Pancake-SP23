using UnityEngine;

namespace EasyCharacterMovement
{
    /// <summary>
    /// CharacterLook.
    ///
    /// Holds look (camera look) related data.
    /// </summary>

    public class CharacterLook : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Determines if the mouse cursor should be locked.")]
        [SerializeField]
        private bool _lockCursor;

        [Tooltip("Determines if the vertical axis should be inverted.")]
        [SerializeField]
        private bool _invertLook;

        [Tooltip("The horizontal sensitivity while using Mouse input, higher values cause faster movement.")]
        [SerializeField]
        private float _mouseHorizontalSensitivity;

        [Tooltip("The vertical sensitivity while using Mouse input, higher values cause faster movement.")]
        [SerializeField]
        private float _mouseVerticalSensitivity;

        [Tooltip("The horizontal sensitivity while using a Controller, higher values cause faster movement.")]
        [SerializeField]
        private float _controllerHorizontalSensitivity;

        [Tooltip("The vertical sensitivity while using a Controller, higher values cause faster movement.")]
        [SerializeField]
        private float _controllerVerticalSensitivity;

        [Space(15f)]
        [Tooltip("Determines if pitch rotation should be clamped between minPitchAngle and maxPitchAngle.")]
        [SerializeField]
        private bool _clampPitchRotation;

        [Tooltip("If clamp pitch rotation is enabled, determines the minimum pitch angle (in degrees).")]
        [SerializeField]
        private float _minPitchAngle;

        [Tooltip("If clamp pitch rotation is enabled, determines the maximum pitch angle (in degrees).")]
        [SerializeField]
        private float _maxPitchAngle;

        #endregion

        #region FIELDS

        protected bool _isCursorLocked;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Determines if the mouse cursor should be locked.
        /// </summary>

        public bool lockCursor
        {
            get => _lockCursor;
            set
            {
                _lockCursor = value;
                if (_lockCursor)
                    return;

                // We force unlock the cursor if the user disable the cursor locking helper

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// The horizontal sensitivity while using Mouse input, higher values cause faster movement.
        /// </summary>

        public float mouseHorizontalSensitivity
        {
            get => _mouseHorizontalSensitivity;
            set => _mouseHorizontalSensitivity = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The vertical sensitivity while using Mouse input, higher values cause faster movement.
        /// </summary>

        public float mouseVerticalSensitivity
        {
            get => _mouseVerticalSensitivity;
            set => _mouseVerticalSensitivity = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The horizontal sensitivity while using a Controller, higher values cause faster movement.
        /// </summary>

        public float controllerHorizontalSensitivity
        {
            get => _controllerHorizontalSensitivity;
            set => _controllerHorizontalSensitivity = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The vertical sensitivity while using a Controller, higher values cause faster movement.
        /// </summary>

        public float controllerVerticalSensitivity
        {
            get => _controllerVerticalSensitivity;
            set => _controllerVerticalSensitivity = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Determines if the look vertical axis should be inverted.
        /// </summary>

        public bool invertLook
        {
            get => _invertLook;
            set => _invertLook = value;
        }

        /// <summary>
        /// Determines if pitch rotation should be clamped between minPitchAngle and maxPitchAngle.
        /// </summary>

        public bool clampPitchRotation
        {
            get => _clampPitchRotation;
            set => _clampPitchRotation = value;
        }
        
        /// <summary>
        /// If clamp pitch rotation is enabled, determines the min pitch angle (in degrees).
        /// </summary>

        public float minPitchAngle
        {
            get => _minPitchAngle;
            set => _minPitchAngle = value;
        }

        /// <summary>
        /// If clamp pitch rotation is enabled, determines the max pitch angle (in degrees).
        /// </summary>

        public float maxPitchAngle
        {
            get => _maxPitchAngle;
            set => _maxPitchAngle = value;
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Is the cursor locked ?
        /// </summary>

        public virtual bool IsCursorLocked()
        {
            return _isCursorLocked;
        }

        /// <summary>
        /// Request to lock the cursor to the center of the screen.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public virtual void LockCursor()
        {
            // If is allowed, lock the cursor

            if (lockCursor)
                _isCursorLocked = true;

            UpdateCursorLockState();
        }

        /// <summary>
        /// Request to unlock the cursor.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public virtual void UnlockCursor()
        {
            _isCursorLocked = false;

            UpdateCursorLockState();
        }

        /// <summary>
        /// Update mouse cursor lock status, eg: toggle lock / unlock.
        /// </summary>

        protected virtual void UpdateCursorLockState()
        {
            if (!lockCursor)
                return;

            if (IsCursorLocked())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        /// <summary>
        /// Assign this default values.
        /// </summary>

        protected virtual void OnReset()
        {
            lockCursor = true;
            mouseHorizontalSensitivity = 0.1f;
            mouseVerticalSensitivity = 0.1f;
            controllerHorizontalSensitivity = 0.5f;
            controllerVerticalSensitivity = 0.5f;
            invertLook = true;

            clampPitchRotation = true;
            minPitchAngle = -80.0f;
            maxPitchAngle =  80.0f;
        }

        /// <summary>
        /// Validate this on editor.
        /// </summary>

        protected virtual void OnOnValidate()
        {
            mouseHorizontalSensitivity = _mouseHorizontalSensitivity;
            mouseVerticalSensitivity = _mouseVerticalSensitivity;
            controllerHorizontalSensitivity = _controllerHorizontalSensitivity;
            controllerVerticalSensitivity = _controllerVerticalSensitivity;

            minPitchAngle = _minPitchAngle;
            maxPitchAngle = _maxPitchAngle;
        }

        /// <summary>
        /// Initialize this, eg: lock cursor (if enabled).
        /// </summary>
        
        protected virtual void OnStart()
        {
            if (lockCursor)
                LockCursor();
        }

        #endregion

        #region MONOBEHAVIOUR

        private void Start()
        {
            OnStart();
        }

        private void Reset()
        {
            OnReset();
        }

        private void OnValidate()
        {
            OnOnValidate();
        }

        #endregion
    }
}
