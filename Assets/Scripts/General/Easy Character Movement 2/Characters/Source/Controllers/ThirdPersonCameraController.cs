using System.Collections.Generic;
using UnityEngine;

namespace EasyCharacterMovement
{
    /// <summary>
    /// Third person camera, used by the ThirdPersonCharacter.
    /// </summary>

    public class ThirdPersonCameraController : MonoBehaviour, IColliderFilter
    {
        #region EDITOR EXPOSED FIELDS

    	[Tooltip("The object that the camera wants to move with (e.g. Its target")]
        [SerializeField]
        private Transform _follow;
        
        [Tooltip("The default distance behind the Follow target.")]
        [SerializeField]
        private float _followDistance;

        [Tooltip("The minimum distance to Follow target.")]
        [SerializeField]
        private float _followMinDistance;

        [Tooltip("The maximum distance to Follow target.")]
        [SerializeField]
        private float _followMaxDistance;

        [Tooltip("The change in rotation per second (Deg / Sec) when looking around.")]
        [SerializeField]
        private float _lookRate;

        [Tooltip("The change in follow distance per second.")]
        [SerializeField]
        private float _zoomRate;

        [Space(15f)]
        [Tooltip("Should the cursor be locked?")]
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
        [Tooltip("Should clamp pitch rotation?")]
        [SerializeField]
        private bool _clampPitchRotation;

        [Tooltip("If clamp pitch rotation is enabled, determines the minimum pitch angle (in degrees).")]
        [SerializeField]
        private float _minPitchAngle;

        [Tooltip("If clamp pitch rotation is enabled, determines the maximum pitch angle (in degrees).")]
        [SerializeField]
        private float _maxPitchAngle;

        [Header("Obstacles")]
        [Tooltip("Camera will avoid obstacles on these layers.")]
        [SerializeField]
        private LayerMask cameraCollisionFilter = 1;

        [Tooltip("Specifies how close the camera can get to obstacles.")]
        [SerializeField]
        private float _cameraRadius;

        [Tooltip("Colliders to be ignored as obstacles.")]
        [SerializeField]
        private List<Collider> _ignoredColliders = new List<Collider>();        

        #endregion

        #region FIELDS

        protected static readonly RaycastHit[] Hits = new RaycastHit[8];

        protected float _yaw;
        protected float _pitch;

        protected Vector3 _prevFollowPosition;
        
        protected float _currentFollowDistance;
        protected float _followDistanceSmoothVelocity;

        protected bool _isCursorLocked;

        #endregion

        #region PROPERTIES

        public Transform follow
        {
            get => _follow;
            set => _follow = value;
        }
        
        public float followDistance
        {
            get => _followDistance;
            set => _followDistance = Mathf.Clamp(value, followMinDistance, followMaxDistance);
        }

        public float followMinDistance
        {
            get => _followMinDistance;
            set => _followMinDistance = Mathf.Max(0.0f, value);
        }

        public float followMaxDistance
        {
            get => _followMaxDistance;
            set => _followMaxDistance = Mathf.Max(0.0f, value);
        }

        public float lookRate
        {
            get => _lookRate;
            set => _lookRate = Mathf.Max(0.0f, value);
        }

        public float zoomRate
        {
            get => _zoomRate;
            set => _zoomRate = Mathf.Max(0.0f, value);
        }

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

        /// <summary>
        /// How close the camera can get from obstacles.
        /// </summary>

        public float cameraRadius
        {
            get => _cameraRadius;
            set => _cameraRadius = Mathf.Max(0.0f, value);
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Determines if the given collider should be filtered (eg: ignored).
        /// </summary>

        public bool Filter(Collider otherCollider)
        {
            for (int i = _ignoredColliders.Count - 1; i >= 0; i--)
            {
                Collider item = _ignoredColliders[i];

                if (item == otherCollider)
                    return true;
            }
            
            return false;
        }

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

            if (_isCursorLocked)
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
        /// Adds the given input value to this yaw.
        /// </summary>

        protected virtual void AddYawInput(float value)
        {
            _yaw = MathLib.Clamp0360(_yaw + value);
        }

        /// <summary>
        /// Adds the given input value to this pitch.
        /// </summary>

        protected virtual void AddPitchInput(float value)
        {
            _pitch = invertLook ? _pitch - value : _pitch + value;

            if (clampPitchRotation)
                _pitch = Mathf.Clamp(_pitch, minPitchAngle, maxPitchAngle);
        }

        /// <summary>
        /// Called via input to turn at an absolute delta, eg: from a mouse.
        /// </summary>

        public virtual void Turn(float value)
        {
            float yaw = value * mouseHorizontalSensitivity;

            AddYawInput(yaw);
        }

        /// <summary>
        /// Called via input to turn at a given rate, eg: an analog joystick.
        /// </summary>

        public virtual void TurnAtRate(float value)
        {
            float yaw = value * controllerHorizontalSensitivity * lookRate * Time.deltaTime;

            AddYawInput(yaw);
        }

        /// <summary>
        /// Called via input to look up/down at an absolute delta, eg: from a mouse.
        /// </summary>

        public virtual void LookUp(float value)
        {
            float pitch = value * mouseVerticalSensitivity;

            AddPitchInput(pitch);
        }

        /// <summary>
        /// Called via input to look up/down at a given rate, eg: an analog joystick.
        /// </summary>

        public virtual void LookUpAtRate(float value)
        {
            float pitch = value * controllerVerticalSensitivity * lookRate * Time.deltaTime;

            AddPitchInput(pitch);
        }

        /// <summary>
        /// Called via input to zoom in / zoom out at a given rate.
        /// </summary>
        
        public virtual void ZoomAtRate(float value)
        {
            followDistance -= value * zoomRate * Time.deltaTime;
        }

        /// <summary>
        /// Set the camera current orientation.
        /// </summary>

        protected virtual void UpdateCameraRotation()
        {
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0.0f);
        }

        /// <summary>
        /// Position the camera followDistance away from target, handling obstructions (if any).
        /// </summary>

        protected virtual void UpdateCameraPosition()
        {
            Vector3 targetPos = follow.position;
            Vector3 prevTargetPos = Time.deltaTime >= 0 ? _prevFollowPosition : targetPos;

            Vector3 dampedTargetPos = Quaternion.Inverse(transform.rotation) * (targetPos - prevTargetPos);
            dampedTargetPos = prevTargetPos + transform.rotation * dampedTargetPos;
            
            Vector3 cameraForward = transform.forward;

            int hitCount = CollisionDetection.SphereCast(dampedTargetPos, cameraRadius, -cameraForward,
                followDistance, cameraCollisionFilter, QueryTriggerInteraction.Ignore, out RaycastHit closestHit, Hits, this, 0.002f);

            _currentFollowDistance = hitCount > 0
                ? closestHit.distance
                : Mathf.SmoothDamp(_currentFollowDistance, followDistance, ref _followDistanceSmoothVelocity, 0.3f);

            _currentFollowDistance = Mathf.Clamp(_currentFollowDistance, 0f, followMaxDistance);
            
            transform.position = dampedTargetPos - cameraForward * _currentFollowDistance;

            _prevFollowPosition = dampedTargetPos;
        }        
        
        /// <summary>
        /// Set default values.
        /// </summary>

        protected virtual void OnReset()
        {
            _followDistance = 5.0f;
            _followMinDistance = 0.0f;
            _followMaxDistance = 10.0f;

            _lookRate = 45.0f;
            _zoomRate = 1.0f;

            _lockCursor = true;
            _invertLook = true;

            _mouseHorizontalSensitivity = 0.1f;
            _mouseVerticalSensitivity = 0.1f;

            _controllerHorizontalSensitivity = 0.5f;
            _controllerVerticalSensitivity = 0.5f;

            _clampPitchRotation = true;

            _minPitchAngle = -80.0f;
            _maxPitchAngle = 80.0f;

            _cameraRadius = 0.2f;
        }

        /// <summary>
        /// Validate this editor exposed fields.
        /// </summary>
        
        protected virtual void OnOnValidate()
        {
            followDistance = _followDistance;
            followMinDistance = _followMinDistance;
            followMaxDistance = _followMaxDistance;

            lookRate = _lookRate;
            zoomRate = _zoomRate;

            mouseHorizontalSensitivity = _mouseHorizontalSensitivity;
            mouseVerticalSensitivity = _mouseVerticalSensitivity;

            controllerHorizontalSensitivity = _controllerHorizontalSensitivity;
            controllerVerticalSensitivity = _controllerVerticalSensitivity;

            minPitchAngle = _minPitchAngle;
            maxPitchAngle = _maxPitchAngle;
        }

        protected virtual void OnAwake()
        {
            Vector3 eulerAngles = transform.eulerAngles;

            _yaw = MathLib.Clamp0360(eulerAngles.y);

            _pitch = clampPitchRotation ? Mathf.Clamp(eulerAngles.x, minPitchAngle, maxPitchAngle) : eulerAngles.x;

            _currentFollowDistance = followDistance;            
        }

        protected virtual void OnStart()
        {
            if (lockCursor)
                LockCursor();
        }

        protected virtual void OnLateUpdate()
        {
            UpdateCameraRotation();

            UpdateCameraPosition();
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

        private void Start()
        {
            OnStart();
        }

        private void LateUpdate()
        {
            OnLateUpdate();
        }        

        #endregion
    }
}
