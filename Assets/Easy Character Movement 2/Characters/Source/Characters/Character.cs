using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement
{
    #region ENUMS

    /// <summary>
    /// Character's current movement mode (walking, falling, etc):
    /// 
    ///    - walking:  Walking on a surface, under the effects of friction, and able to "step up" barriers. Vertical velocity is zero.
    ///    - falling:  Falling under the effects of gravity, after jumping or walking off the edge of a surface.
    ///    - flying:   Flying, ignoring the effects of gravity.
    ///    - swimming: Swimming through a fluid volume, under the effects of gravity and buoyancy.
    ///    - custom:   User-defined custom movement mode, including many possible sub-modes.
    /// </summary>

    public enum MovementMode
    {
        None,
        Walking,
        Falling,
        Swimming,
        Flying,
        Custom
    }

    /// <summary>
    /// Character's current rotation mode:
    /// 
    ///     - None:                        Disables character's rotation.
    ///     - OrientToMovement:            Rotates the Character towards the given input move direction vector, using rotationRate as the rate of rotation change.
    ///     - OrientToCameraViewDirection: Rotates the Character towards the camera's current view direction, using rotationRate as the rate of rotation change.
    ///     - OrientWithRootMotion:        Append root motion rotation to Character's rotation.
    ///     - Custom:                      User-defined custom rotation mode.
    /// </summary>

    public enum RotationMode
    {
        None,
        OrientToMovement,
        OrientToCameraViewDirection,
        OrientWithRootMotion,
        Custom
    }

    #endregion

    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Input actions associated with this Character." +
                 " If not assigned, this Character wont process any input so you can externally take control of this Character (e.g. a Controller).")]
        [SerializeField]
        private InputActionAsset _inputActions;
        
        [Space(15f)]
        [Tooltip("Character's current rotation mode.")]
        [SerializeField]
        private RotationMode _rotationMode;

        [Tooltip("Change in rotation per second (Deg / s).")]
        [SerializeField]
        private float _rotationRate;
        
        [Space(15f)]
        [Tooltip("The Character's default movement mode. Used at player startup.")]
        [SerializeField]
        private MovementMode _defaultMovementMode;

        [Space(15f)]
        [Tooltip("The maximum ground speed when walking.\n" +
                 "Also determines maximum lateral speed when falling.")]
        [SerializeField]
        private float _maxWalkSpeed;

        [Tooltip("The ground speed that we should accelerate up to when walking at minimum analog stick tilt.")]
        [SerializeField]
        private float _minAnalogWalkSpeed;

        [Tooltip("Max Acceleration (rate of change of velocity).")]
        [SerializeField]
        private float _maxAcceleration;

        [Tooltip("Deceleration when walking and not applying acceleration.\n" +
                 "This is a constant opposing force that directly lowers velocity by a constant value.")]
        [SerializeField]
        private float _brakingDecelerationWalking;

        [Tooltip("Setting that affects movement control.\n" +
                 "Higher values allow faster changes in direction.\n" +
                 "If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).")]
        [SerializeField]
        private float _groundFriction;

        [Space(15f)]
        [Tooltip("The maximum vertical velocity a Character can reach when falling. Eg: Terminal velocity.")]
        [SerializeField]
        private float _maxFallSpeed;

        [Tooltip("Lateral deceleration when falling and not applying acceleration.")]
        [SerializeField]
        private float _brakingDecelerationFalling;

        [Tooltip("Friction to apply to lateral movement when falling. \n" +
                 "If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).")]
        [SerializeField]
        private float _fallingLateralFriction;

        [Range(0.0f, 1.0f)]
        [Tooltip("When falling, amount of lateral movement control available to the Character.\n" +
                 "0 = no control, 1 = full control at max acceleration.")]
        [SerializeField]
        private float _airControl;

        [Space(15f)]
        [Tooltip("The maximum flying speed.")]
        [SerializeField]
        private float _maxFlySpeed;

        [Tooltip("Deceleration when flying and not applying acceleration.")]
        [SerializeField]
        private float _brakingDecelerationFlying;

        [Tooltip("Friction to apply to movement when flying.")]
        [SerializeField]
        private float _flyingFriction;

        [Space(15f)]
        [Tooltip("If True, this Character is capable to Swim or move through fluid volumes.")]
        [SerializeField]
        private bool _canEverSwim;

        [Tooltip("The maximum swimming speed.")]
        [SerializeField]
        private float _maxSwimSpeed;

        [Tooltip("Deceleration when swimming and not applying acceleration.")]
        [HideInInspector]
        [SerializeField]
        private float _brakingDecelerationSwimming;

        [Tooltip("Friction to apply to movement when swimming.")]
        [SerializeField]
        private float _swimmingFriction;

        [Tooltip("Water buoyancy ratio. 1 = Neutral Buoyancy, 0 = No Buoyancy.")]
        [SerializeField]
        private float _buoyancy;

        [Space(15f)]
        [SerializeField]
        private bool _useSeparateBrakingFriction;

        [SerializeField]
        private float _brakingFriction;

        [Space(15f)]
        [SerializeField]
        private bool _canEverSprint;

        [Tooltip("The walk speed multiplier while sprinting.")]
        [SerializeField]
        private float _sprintSpeedModifier;

        [Tooltip("The walk acceleration multiplier while sprinting.")]
        [SerializeField]
        private float _sprintAccelerationModifier;

        [Space(15f)]
        [SerializeField]
        private bool _canEverCrouch;

        [SerializeField]
        private float _unCrouchedHeight;

        [SerializeField]
        private float _crouchedHeight;

        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float _crouchingSpeedModifier;

        [Space(15f)]
        [Tooltip("Is the character able to jump ?")]
        [SerializeField]
        private bool _canEverJump;

        [Tooltip("Can jump while crouching ?")]
        [SerializeField]
        private bool _jumpWhileCrouching;

        [Tooltip("The max number of jumps the Character can perform.")]
        [SerializeField]
        private int _jumpMaxCount;

        [Tooltip("Initial velocity (instantaneous vertical velocity) when jumping.")]
        [SerializeField]
        private float _jumpImpulse;

        [Tooltip("The maximum time (in seconds) to hold the jump. eg: Variable height jump.")]
        [SerializeField]
        private float _jumpMaxHoldTime;

        [Tooltip("How early before hitting the ground you can trigger a jump (in seconds).")]
        [SerializeField]
        private float _jumpPreGroundedTime;

        [Tooltip("How long after leaving the ground you can trigger a jump (in seconds).")]
        [SerializeField]
        private float _jumpPostGroundedTime;

        [Space(15f)]
        [SerializeField]
        private Vector3 _gravity;

        [SerializeField]
        private float _gravityScale;

        [Space(15f)]
        [Tooltip("Should animation determines the Character's movement ?")]
        [SerializeField]
        private bool _useRootMotion;

        [Space(15f)]
        [Tooltip("Whether the Character moves with the moving platform it is standing on.")]
        [SerializeField]
        private bool _impartPlatformMovement;

        [Tooltip("Whether the Character receives the changes in rotation of the platform it is standing on.")]
        [SerializeField]
        private bool _impartPlatformRotation;

        [Tooltip("If true, impart the platform's velocity when jumping or falling off it.")]
        [SerializeField]
        private bool _impartPlatformVelocity;

        [Space(15f)]
        [Tooltip("If enabled, the player will interact with dynamic rigidbodies when walking into them.")]
        [SerializeField]
        private bool _enablePhysicsInteraction;

        [Tooltip("Should apply push force to characters when walking into them ?")]
        [SerializeField]
        private bool _applyPushForceToCharacters;

        [Tooltip("Should apply a downward force to rigidbodies we stand on ?")]
        [SerializeField]
        private bool _applyStandingDownwardForce;

        [Tooltip("This Character's mass (in Kg)." +
                 "Determines how the character interact against other characters or dynamic rigidbodies if enablePhysicsInteraction == true.")]
        [SerializeField]
        private float _mass;

        [Tooltip("Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.")]
        [SerializeField]
        private float _pushForceScale;

        [Tooltip("Force applied to rigidbodies we stand on (due to mass and gravity) is scaled by this amount.")]
        [SerializeField]
        private float _standingDownwardForceScale;

        [Space(15f)]
        [Tooltip("Reference to the Player's Camera.\n" +
                 "If assigned, the Character's movement will be relative to this camera, otherwise movement will be relative to world axis.")]
        [SerializeField]
        private Camera _camera;

        #endregion

        #region FIELDS

        private bool _enableLateFixedUpdateCoroutine;
        private Coroutine _lateFixedUpdateCoroutine;

        protected List<PhysicsVolume> _volumes = new List<PhysicsVolume>();
        
        private Transform _transform;
        private CharacterMovement _characterMovement;
        private Animator _animator;
        private RootMotionController _rootMotionController;
        private Transform _cameraTransform;

        /// <summary>
        /// The Character's current movement mode.
        /// </summary>

        protected MovementMode _movementMode = MovementMode.None;

        /// <summary>
        /// Character's User-defined custom movement mode (sub-mode).
        /// Only applicable if _movementMode == Custom.
        /// </summary>

        protected int _customMovementMode;

        protected bool _applyGravity = true;

        protected float _fallingTime;

        protected bool _sprintButtonPressed;

        protected bool _isCrouching;
        protected bool _crouchButtonPressed;

        protected bool _jumpButtonPressed;
        protected float _jumpButtonHeldDownTime;
        protected float _jumpHoldTime;
        protected int _jumpCount;
        protected bool _isJumping;

        private Vector3 _rotationInput = Vector3.zero;
        private Vector3 _movementDirection = Vector3.zero;

        private float _deltaTime;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The used deltaTime. Defaults to Time.deltaTime.
        /// </summary>

        public float deltaTime
        {
            get => _deltaTime == 0.0f ? Time.deltaTime : _deltaTime;
            set => _deltaTime = value;
        }

        /// <summary>
        /// This Character's camera transform.
        /// If assigned, the Character's movement will be relative to this, otherwise movement will be relative to world.
        /// </summary>

        public new Camera camera
        {
            get => _camera;
            set => _camera = value;
        }

        /// <summary>
        /// Cached camera transform (if any).
        /// </summary>

        public Transform cameraTransform
        {
            get
            {
                if (_camera != null)
                    _cameraTransform = _camera.transform;

                return _cameraTransform;
            }
        }

        /// <summary>
        /// Cached Character transform.
        /// </summary>

        public new Transform transform
        {
            get
            {
#if UNITY_EDITOR
                if (_transform == null)
                    _transform = GetComponent<Transform>();
#endif

                return _transform;
            }
        }

        /// <summary>
        /// Cached CharacterMovement component.
        /// </summary>

        protected CharacterMovement characterMovement
        {
            get
            {
#if UNITY_EDITOR
                if (_characterMovement == null)
                    _characterMovement = GetComponent<CharacterMovement>();
#endif

                return _characterMovement;
            }
        }

        /// <summary>
        /// Cached Animator component (if any).
        /// </summary>

        protected Animator animator
        {
            get
            {
#if UNITY_EDITOR
                if (_animator == null)
                    _animator = GetComponentInChildren<Animator>();
#endif

                return _animator;
            }
        }

        /// <summary>
        /// Cached Character's RootMotionController component (if any).
        /// </summary>

        protected RootMotionController rootMotionController
        {
            get
            {
#if UNITY_EDITOR
                if (_rootMotionController == null)
                    _rootMotionController = GetComponentInChildren<RootMotionController>();
#endif

                return _rootMotionController;
            }
        }

        /// <summary>
        /// Change in rotation per second (Deg / s).
        /// </summary>

        public float rotationRate
        {
            get => _rotationRate;
            set => _rotationRate = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Default movement mode. Used at player startup. 
        /// </summary>

        public MovementMode defaultMovementMode
        {
            get => _defaultMovementMode;
            set => _defaultMovementMode = value;
        }

        /// <summary>
        /// The maximum ground speed when walking. Also determines maximum lateral speed when falling.
        /// </summary>

        public float maxWalkSpeed
        {
            get => _maxWalkSpeed;
            set => _maxWalkSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The ground speed that we should accelerate up to when walking at minimum analog stick tilt.
        /// </summary>

        public float minAnalogWalkSpeed
        {
            get => _minAnalogWalkSpeed;
            set => _minAnalogWalkSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Max Acceleration (rate of change of velocity).
        /// </summary>

        public float maxAcceleration
        {
            get => _maxAcceleration;
            set => _maxAcceleration = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when walking and not applying acceleration.
        /// This is a constant opposing force that directly lowers velocity by a constant value.
        /// </summary>

        public float brakingDecelerationWalking
        {
            get => _brakingDecelerationWalking;
            set => _brakingDecelerationWalking = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Setting that affects movement control.
        /// Higher values allow faster changes in direction.
        /// If useSeparateBrakingFriction is false, also affects the ability to stop more quickly when braking (whenever acceleration is zero).
        /// </summary>

        public float groundFriction
        {
            get => _groundFriction;
            set => _groundFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The maximum vertical velocity (in m/s) a Character can reach when falling.
        /// Eg: Terminal velocity.
        /// </summary>

        public float maxFallSpeed
        {
            get => _maxFallSpeed;
            set => _maxFallSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Lateral deceleration when falling and not applying acceleration.
        /// </summary>

        public float brakingDecelerationFalling
        {
            get => _brakingDecelerationFalling;
            set => _brakingDecelerationFalling = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to lateral air movement when falling.
        /// </summary>

        public float fallingLateralFriction
        {
            get => _fallingLateralFriction;
            set => _fallingLateralFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The Character's time in falling movement mode.
        /// </summary>

        public float fallingTime => _fallingTime;

        /// <summary>
        /// When falling, amount of lateral movement control available to the Character.
        /// 0 = no control, 1 = full control at max acceleration.
        /// </summary>

        public float airControl
        {
            get => _airControl;
            set => _airControl = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The maximum flying speed.
        /// </summary>

        public float maxFlySpeed
        {
            get => _maxFlySpeed;
            set => _maxFlySpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when flying and not applying acceleration.
        /// </summary>

        public float brakingDecelerationFlying
        {
            get => _brakingDecelerationFlying;
            set => _brakingDecelerationFlying = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to movement when flying.
        /// </summary>

        public float flyingFriction
        {
            get => _flyingFriction;
            set => _flyingFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// If True, this Character is capable to Swim or move through fluid volumes.
        /// </summary>

        public bool canEverSwim
        {
            get => _canEverSwim;
            set => _canEverSwim = value;
        }

        /// <summary>
        /// The maximum swimming speed.
        /// </summary>

        public float maxSwimSpeed
        {
            get => _maxSwimSpeed;
            set => _maxSwimSpeed = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Deceleration when swimming and not applying acceleration.
        /// </summary>

        public float brakingDecelerationSwimming
        {
            get => _brakingDecelerationSwimming;
            set => _brakingDecelerationSwimming = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Friction to apply to movement when swimming.
        /// </summary>

        public float swimmingFriction
        {
            get => _swimmingFriction;
            set => _swimmingFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Water buoyancy ratio. 1 = Neutral Buoyancy, 0 = No Buoyancy.
        /// </summary>

        public float buoyancy
        {
            get => _buoyancy;
            set => _buoyancy = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Should use a separate braking friction ?
        /// </summary>

        public bool useSeparateBrakingFriction
        {
            get => _useSeparateBrakingFriction;
            set => _useSeparateBrakingFriction = value;
        }

        /// <summary>
        /// Friction (drag) coefficient applied when braking (whenever Acceleration = 0, or if Character is exceeding max speed).
        /// This is the value, used in all movement modes IF useSeparateBrakingFriction is True.
        /// </summary>

        public float brakingFriction
        {
            get => _brakingFriction;
            set => _brakingFriction = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Is the character able to sprint ?
        /// </summary>

        public bool canEverSprint
        {
            get => _canEverSprint;
            set => _canEverSprint = value;
        }

        /// <summary>
        /// The walk speed modifier while sprinting.
        /// </summary>

        public float sprintSpeedModifier
        {
            get => _sprintSpeedModifier;
            set => _sprintSpeedModifier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The walk acceleration modifier while sprinting.
        /// </summary>

        public float sprintAccelerationModifier
        {
            get => _sprintAccelerationModifier;
            set => _sprintAccelerationModifier = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Is the character able to crouch ? Enable / Disable Crouch mechanic.
        /// </summary>

        public bool canEverCrouch
        {
            get => _canEverCrouch;
            set => _canEverCrouch = value;
        }

        /// <summary>
        /// If canEverCrouch == true, determines the character un-crouched height.
        /// </summary>

        public float unCrouchedHeight
        {
            get => _unCrouchedHeight;
            set => _unCrouchedHeight = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// If canEverCrouch == true, determines the character crouched height.
        /// </summary>

        public float crouchedHeight
        {
            get => _crouchedHeight;
            set => _crouchedHeight = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The walk speed modifier while crouching.
        /// </summary>

        public float crouchingSpeedModifier
        {
            get => _crouchingSpeedModifier;
            set => _crouchingSpeedModifier = Mathf.Clamp01(value);
        }

        /// <summary>
        /// Is the character able to jump ?
        /// </summary>

        public bool canEverJump
        {
            get => _canEverJump;
            set => _canEverJump = value;
        }

        /// <summary>
        /// Can jump while crouching ?
        /// </summary>

        public bool jumpWhileCrouching
        {
            get => _jumpWhileCrouching;
            set => _jumpWhileCrouching = value;
        }

        /// <summary>
        /// The max number of jumps the Character can perform.
        /// </summary>

        public int jumpMaxCount
        {
            get => _jumpMaxCount;
            set => _jumpMaxCount = Mathf.Max(0, value);
        }

        /// <summary>
        /// Initial velocity (instantaneous vertical velocity) when jumping.
        /// </summary>

        public float jumpImpulse
        {
            get => _jumpImpulse;
            set => _jumpImpulse = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// The maximum time (in seconds) to hold the jump. eg: Variable height jump.
        /// </summary>

        public float jumpMaxHoldTime
        {
            get => _jumpMaxHoldTime;
            set => _jumpMaxHoldTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// How early before hitting the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpPreGroundedTime
        {
            get => _jumpPreGroundedTime;
            set => _jumpPreGroundedTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// How long after leaving the ground you can trigger a jump (in seconds).
        /// </summary>

        public float jumpPostGroundedTime
        {
            get => _jumpPostGroundedTime;
            set => _jumpPostGroundedTime = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// True is _jumpButtonPressed is true, false otherwise.
        /// </summary>

        public bool jumpButtonPressed => _jumpButtonPressed;

        /// <summary>
        /// This is the time (in seconds) that the player has held the jump button.
        /// </summary>

        public float jumpButtonHeldDownTime => _jumpButtonHeldDownTime;

        /// <summary>
        /// Tracks the current number of jumps performed.
        /// </summary>

        public int jumpCount => _jumpCount;

        /// <summary>
        /// This is the time (in seconds) that the player has been holding the jump.
        /// Eg: Variable height jump.
        /// </summary>

        public float jumpHoldTime => _jumpHoldTime;

        /// <summary>
        /// Should notify a jump apex ?
        /// Set to true to receive OnReachedJumpApex event.
        /// </summary>

        public bool notifyJumpApex { get; set; }

        /// <summary>
        /// The Character's gravity (modified by gravityScale). Defaults to Physics.gravity.
        /// </summary>

        public Vector3 gravity
        {
            get => _gravity * _gravityScale;
            set => _gravity = value;
        }

        /// <summary>
        /// The degree to which this object is affected by gravity.
        /// Can be negative allowing to change gravity direction.
        /// </summary>

        public float gravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        /// <summary>
        /// Should animation determines the Character' movement ?
        /// </summary>

        public bool useRootMotion
        {
            get => _useRootMotion;
            set => _useRootMotion = value;
        }
        
        /// <summary>
        /// If enabled, the player will interact with dynamic rigidbodies when walking into them.
        /// </summary>

        public bool enablePhysicsInteraction
        {
            get => _enablePhysicsInteraction;
            set
            {
                _enablePhysicsInteraction = value;

                if (_characterMovement)
                    _characterMovement.enablePhysicsInteraction = _enablePhysicsInteraction;
            }
        }

        /// <summary>
        /// Should apply push force to other characters when walking into them ?
        /// </summary>

        public bool applyPushForceToCharacters
        {
            get => _applyPushForceToCharacters;
            set
            {
                _applyPushForceToCharacters = value;

                if (_characterMovement)
                    _characterMovement.physicsInteractionAffectsCharacters = _applyPushForceToCharacters;
            }
        }

        /// <summary>
        /// Should apply a downward force to rigidbodies we stand on ?
        /// </summary>

        public bool applyStandingDownwardForce
        {
            get => _applyStandingDownwardForce;
            set => _applyStandingDownwardForce = value;
        }

        /// <summary>
        /// This Character's mass (in Kg).
        /// </summary>

        public float mass
        {
            get => _mass;
            set
            {
                _mass = Mathf.Max(1e-07f, value);

                if (_characterMovement && _characterMovement.rigidbody)
                    _characterMovement.rigidbody.mass = _mass;
            }
        }

        /// <summary>
        /// Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.
        /// </summary>

        public float pushForceScale
        {
            get => _pushForceScale;
            set
            {
                _pushForceScale = Mathf.Max(0.0f, value);

                if (_characterMovement)
                    _characterMovement.pushForceScale = _pushForceScale;
            }
        }

        /// <summary>
        /// Force applied to rigidbodies we stand on (due to mass and gravity) is scaled by this amount.
        /// </summary>

        public float standingDownwardForceScale
        {
            get => _standingDownwardForceScale;
            set => _standingDownwardForceScale = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// If true, impart the platform's velocity when jumping or falling off it.
        /// </summary>

        public bool impartPlatformVelocity
        {
            get => _impartPlatformVelocity;
            set
            {
                _impartPlatformVelocity = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformVelocity = _impartPlatformVelocity;
            }
        }

        /// <summary>
        /// Whether the Character moves with the moving platform it is standing on.
        /// If true, the Character moves with the moving platform.
        /// </summary>

        public bool impartPlatformMovement
        {
            get => _impartPlatformMovement;
            set
            {
                _impartPlatformMovement = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformMovement = _impartPlatformMovement;
            }
        }

        /// <summary>
        /// Whether the Character receives the changes in rotation of the platform it is standing on.
        /// If true, the Character rotates with the moving platform.
        /// </summary>

        public bool impartPlatformRotation
        {
            get => _impartPlatformRotation;
            set
            {
                _impartPlatformRotation = value;

                if (_characterMovement)
                    _characterMovement.impartPlatformRotation = _impartPlatformRotation;
            }
        }
        
        /// <summary>
        /// PhysicsVolume overlapping this component. NULL if none.
        /// </summary>

        public PhysicsVolume physicsVolume { get; set; }

        /// <summary>
        /// Enable / Disable the LateFixedUpdate Coroutine.
        /// Enabled by default.
        /// </summary>

        public bool enableLateFixedUpdate
        {
            get => _enableLateFixedUpdateCoroutine;
            set
            {
                _enableLateFixedUpdateCoroutine = value;
                EnableLateFixedUpdate(_enableLateFixedUpdateCoroutine);
            }
        }

        #endregion

        #region INPUT ACTIONS

        /// <summary>
        /// InputActions assets.
        /// </summary>

        public InputActionAsset inputActions
        {
            get => _inputActions;
            set => _inputActions = value;
        }

        /// <summary>
        /// Movement InputAction.
        /// </summary>

        protected InputAction movementInputAction { get; set; }

        /// <summary>
        /// Sprint InputAction.
        /// </summary>

        protected InputAction sprintInputAction { get; set; }

        /// <summary>
        /// Crouch InputAction.
        /// </summary>

        protected InputAction crouchInputAction { get; set; }

        /// <summary>
        /// Jump InputAction.
        /// </summary>

        protected InputAction jumpInputAction { get; set; }

        #endregion

        #region INPUT ACTION HANDLERS

        /// <summary>
        /// Polls movement InputAction (if any).
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>

        protected virtual Vector2 GetMovementInput()
        {
            return movementInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
        }

        /// <summary>
        /// Sprint input action handler.
        /// </summary>

        protected virtual void OnSprint(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
                Sprint();
            else if (context.canceled)
                StopSprinting();
        }

        /// <summary>
        /// Crouch input action handler.
        /// </summary>

        protected virtual void OnCrouch(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
                Crouch();
            else if (context.canceled)
                StopCrouching();
        }

        /// <summary>
        /// Jump input action handler.
        /// </summary>

        protected virtual void OnJump(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
                Jump();
            else if (context.canceled)
                StopJumping();
        }

        #endregion

        #region EVENTS

        public delegate void PhysicsVolumeChangedEventHandler(PhysicsVolume newPhysicsVolume);

        public delegate void MovementModeChangedEventHandler(MovementMode prevMovementMode, int prevCustomMode);

        public delegate void SprintedEventHandler();
        public delegate void StoppedSprintingEventHandler();

        public delegate void CrouchedEventHandler();
        public delegate void UnCrouchedEventHandler();

        public delegate void JumpedEventHandler();
        public delegate void ReachedJumpApexEventHandler();

        public delegate void WillLandEventHandler();
        public delegate void LandedEventHandler();

        /// <summary>
        /// Event triggered when a character enter or leaves a PhysicsVolume.
        /// </summary>

        public event PhysicsVolumeChangedEventHandler PhysicsVolumeChanged;

        /// <summary>
        /// Event triggered after a MovementMode change.
        /// </summary>

        public event MovementModeChangedEventHandler MovementModeChanged;

        /// <summary>
        /// Event triggered when Character start sprinting.
        /// </summary>

        public event SprintedEventHandler Sprinted;

        /// <summary>
        /// Event triggered when Character stops sprinting.
        /// </summary>

        public event StoppedSprintingEventHandler StoppedSprinting;

        /// <summary>
        /// Event triggered when Character enters crouching state.
        /// </summary>

        public event CrouchedEventHandler Crouched;

        /// <summary>
        /// Event triggered when character exits crouching state.
        /// </summary>

        public event UnCrouchedEventHandler UnCrouched;

        /// <summary>
        /// Event triggered when character jumps.
        /// </summary>

        public event JumpedEventHandler Jumped;

        /// <summary>
        /// Triggered when Character reaches jump apex (eg: change in vertical speed from positive to negative).
        /// Only triggered if notifyJumpApex == true.
        /// </summary>

        public event ReachedJumpApexEventHandler ReachedJumpApex;

        /// <summary>
        /// Triggered when the Character will hit walkable ground.
        /// </summary>

        public event WillLandEventHandler WillLand;

        /// <summary>
        /// Event triggered when character enter isGrounded state (isOnWalkableGround AND isConstrainedToGround)
        /// </summary>

        public event LandedEventHandler Landed;

        /// <summary>
        /// Event triggered when characters collides with other during a Move.
        /// Can be called multiple times.
        /// </summary>

        protected virtual void OnCollided(ref CollisionResult collisionResult)
        {
            // If found walkable ground during movement, trigger will land event

            if (!characterMovement.wasGrounded && collisionResult.isWalkable)
                OnWillLand();
        }

        /// <summary>
        /// Event triggered when character find ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        protected virtual void OnFoundGround(ref FindGroundResult foundGround)
        {
            // If found ground is walkable, trigger Landed event

            if (foundGround.isWalkable)
                OnLanded();
        }

        /// <summary>
        /// Event triggered when character found walkable ground during its movement phase.
        /// </summary>

        protected virtual void OnWillLand()
        {
            WillLand?.Invoke();
        }

        /// <summary>
        /// Event triggered when character enter isGrounded state (isOnWalkableGround AND isConstrainedToGround)
        /// </summary>

        protected virtual void OnLanded()
        {
            Landed?.Invoke();
        }

        /// <summary>
        /// Event triggered when Character start sprinting.
        /// </summary>

        protected virtual void OnSprinted()
        {
            Sprinted?.Invoke();
        }

        /// <summary>
        /// Event triggered when Character stops sprinting.
        /// </summary>

        protected virtual void OnStoppedSprinting()
        {
            StoppedSprinting?.Invoke();
        }

        /// <summary>
        /// Called when Character crouches.
        /// </summary>

        protected virtual void OnCrouched()
        {
            // Trigger crouched event

            Crouched?.Invoke();
        }

        /// <summary>
        /// Called when Character stops crouching.
        /// </summary>

        protected virtual void OnUnCrouched()
        {
            // Trigger un crouched event

            UnCrouched?.Invoke();
        }

        /// <summary>
        /// Called when a jump has been successfully triggered.
        /// </summary>

        protected virtual void OnJumped()
        {
            // Trigger Jumped event

            Jumped?.Invoke();
        }

        /// <summary>
        /// Called when Character reaches jump apex (eg: change in vertical speed from positive to negative).
        /// Only triggered if notifyJumpApex == true.
        /// </summary>

        protected virtual void OnReachedJumpApex()
        {
            // Trigger ReachedJumpApex event

            ReachedJumpApex?.Invoke();
        }

        /// <summary>
        /// Called when PhysicsVolume has been changed.
        /// </summary>

        protected virtual void OnPhysicsVolumeChanged(PhysicsVolume newPhysicsVolume)
        {
            if (newPhysicsVolume && newPhysicsVolume.waterVolume)
            {
                // Entering a water volume

                Swim();
            }
            else if (IsInWater() && newPhysicsVolume == null)
            {
                // Left a water volume

                StopSwimming();
            }

            // Trigger PhysicsVolumeChanged event

            PhysicsVolumeChanged?.Invoke(newPhysicsVolume);
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Start / Stops LateFixedUpdate coroutine.
        /// </summary>

        private void EnableLateFixedUpdate(bool enable)
        {
            if (enable)
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);

                _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
            }
            else
            {
                if (_lateFixedUpdateCoroutine != null)
                    StopCoroutine(_lateFixedUpdateCoroutine);
            }
        }

        /// <summary>
        /// Sets the give new volume as our current Physics Volume.
        /// Will call PhysicsVolumeChanged delegate.
        /// </summary>

        protected virtual void SetPhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            if (newPhysicsVolume == physicsVolume)
                return;

            // Trigger PhysicsVolumeChanged event

            OnPhysicsVolumeChanged(newPhysicsVolume);

            // Updates current physics volume

            physicsVolume = newPhysicsVolume;
        }

        /// <summary>
        /// Update character's current physics volume.
        /// </summary>

        protected virtual void UpdatePhysicsVolume(PhysicsVolume newPhysicsVolume)
        {
            // Check if Character is inside or outside a PhysicsVolume,
            // It uses the Character's center as reference point

            Vector3 characterCenter = characterMovement.worldCenter;

            if (newPhysicsVolume && newPhysicsVolume.boxCollider.ClosestPoint(characterCenter) == characterCenter)
            {
                // Entering physics volume

                SetPhysicsVolume(newPhysicsVolume);
            }
            else
            {
                // Leaving physics volume

                SetPhysicsVolume(null);
            }
        }

        /// <summary>
        /// Attempts to add a new physics volume to our volumes list.
        /// </summary>

        protected virtual void AddPhysicsVolume(Collider other)
        {
            if (other.TryGetComponent(out PhysicsVolume volume) && !_volumes.Contains(volume))
                _volumes.Insert(0, volume);
        }

        /// <summary>
        /// Attempts to remove a physics volume from our volumes list.
        /// </summary>

        protected virtual void RemovePhysicsVolume(Collider other)
        {
            if (other.TryGetComponent(out PhysicsVolume volume) && _volumes.Contains(volume))
                _volumes.Remove(volume);
        }

        /// <summary>
        /// Sets as current physics volume the one with higher priority.
        /// </summary>

        protected virtual void UpdatePhysicsVolumes()
        {
            // Is Character movement is disabled, return

            if (IsDisabled())
                return;

            // Find volume with higher priority

            PhysicsVolume volume = null;
            int maxPriority = int.MinValue;

            for (int i = 0, c = _volumes.Count; i < c; i++)
            {
                PhysicsVolume vol = _volumes[i];
                if (vol.priority <= maxPriority)
                    continue;

                maxPriority = vol.priority;
                volume = vol;
            }

            // Update character's current volume

            UpdatePhysicsVolume(volume);
        }

        /// <summary>
        /// Adds a force to the Character.
        /// This forces will be accumulated and applied during Move method call.
        /// </summary>

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            characterMovement.AddForce(force, forceMode);
        }

        /// <summary>
        /// Applies a force to a rigidbody that simulates explosion effects.
        /// The explosion is modeled as a sphere with a certain centre position and radius in world space;
        /// normally, anything outside the sphere is not affected by the explosion and the force decreases in proportion to distance from the centre.
        /// However, if a value of zero is passed for the radius then the full force will be applied regardless of how far the centre is from the rigidbody.
        /// </summary>

        public void AddExplosionForce(float forceMagnitude, Vector3 origin, float explosionRadius,
            ForceMode forceMode = ForceMode.Force)
        {
            characterMovement.AddExplosionForce(forceMagnitude, origin, explosionRadius, forceMode);
        }

        /// <summary>
        /// Set a pending launch velocity on the Character. This velocity will be processed next Move call.
        /// </summary>
        /// <param name="launchVelocity">The desired launch velocity.</param>
        /// <param name="overrideVerticalVelocity">If true replace the vertical component of the Character's velocity instead of adding to it.</param>
        /// <param name="overrideLateralVelocity">If true replace the XY part of the Character's velocity instead of adding to it.</param>

        public void LaunchCharacter(Vector3 launchVelocity, bool overrideVerticalVelocity = false,
            bool overrideLateralVelocity = false)
        {
            characterMovement.LaunchCharacter(launchVelocity, overrideVerticalVelocity, overrideLateralVelocity);
        }

        /// <summary>
        /// Should collision detection be enabled ?
        /// </summary>

        public virtual void DetectCollisions(bool detectCollisions)
        {
            characterMovement.detectCollisions = detectCollisions;
        }

        /// <summary>
        /// Makes the character to ignore all collisions vs otherCollider.
        /// </summary>

        public virtual void IgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            characterMovement.IgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Makes the character to ignore collisions vs all colliders attached to the otherRigidbody.
        /// </summary>

        public virtual void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
        {
            characterMovement.IgnoreCollision(otherRigidbody, ignore);
        }

        /// <summary>
        /// Makes the character's collider (eg: CapsuleCollider) to ignore all collisions vs otherCollider.
        /// NOTE: The character can still collide with other during a Move call if otherCollider is in CollisionLayers mask.
        /// </summary>

        public virtual void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            characterMovement.CapsuleIgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Jump, etc.
        /// </summary>

        public virtual void PauseGroundConstraint(float seconds = 0.1f)
        {
            characterMovement.PauseGroundConstraint(seconds);
        }

        /// <summary>
        /// Was the character on ground last Move call ?
        /// </summary>

        public virtual bool WasOnGround()
        {
            return characterMovement.wasOnGround;
        }

        /// <summary>
        /// Is the character on ground ?
        /// </summary>

        public virtual bool IsOnGround()
        {
            return characterMovement.isOnGround;
        }

        /// <summary>
        /// Was the character on walkable ground last Move call ?
        /// </summary>

        public virtual bool WasOnWalkableGround()
        {
            return characterMovement.wasOnWalkableGround;
        }

        /// <summary>
        /// Is the character on walkable ground ?
        /// </summary>

        public virtual bool IsOnWalkableGround()
        {
            return characterMovement.isOnWalkableGround;
        }

        /// <summary>
        /// Was the character on walkable ground AND constrained to ground last Move call ?
        /// </summary>

        public virtual bool WasGrounded()
        {
            return characterMovement.wasGrounded;
        }

        /// <summary>
        /// Is the character on walkable ground AND constrained to ground.
        /// </summary>

        public virtual bool IsGrounded()
        {
            return characterMovement.isGrounded;
        }

        /// <summary>
        /// Return the CharacterMovement component. This is guaranteed to be not null.
        /// </summary>

        public virtual CharacterMovement GetCharacterMovement()
        {
            return characterMovement;
        }

        /// <summary>
        /// Return the Animator component or null is not found.
        /// </summary>

        public virtual Animator GetAnimator()
        {
            return animator;
        }

        /// <summary>
        /// Return the RootMotionController or null is not found.
        /// </summary>

        public virtual RootMotionController GetRootMotionController()
        {
            return rootMotionController;
        }

        /// <summary>
        /// Return the Character's current PhysicsVolume, null if none.
        /// </summary>

        public virtual PhysicsVolume GetPhysicsVolume()
        {
            return physicsVolume;
        }

        /// <summary>
        /// The character's current height (accounting crouching if crouching).
        /// </summary>
        /// <returns></returns>

        public virtual float GetHeight()
        {
            float actualHeight = IsCrouching() ? crouchedHeight : unCrouchedHeight;

            return actualHeight;
        }

        /// <summary>
        /// The character's radius
        /// </summary>

        public virtual float GetRadius()
        {
            return characterMovement.radius;
        }

        /// <summary>
        /// The Character's current position.
        /// </summary>

        public virtual Vector3 GetPosition()
        {
            return characterMovement.position;
        }

        /// <summary>
        /// Sets the Character's position.
        /// This complies with the interpolation resulting in a smooth transition between the two positions in any intermediate frames rendered.
        /// </summary>

        public virtual void SetPosition(Vector3 position, bool updateGround = false)
        {
            characterMovement.SetPosition(position, updateGround);
        }

        /// <summary>
        /// The Character's current rotation.
        /// </summary>

        public Quaternion GetRotation()
        {
            return characterMovement.rotation;
        }

        /// <summary>
        /// Sets the Character's current rotation.
        /// </summary>

        public void SetRotation(Quaternion newRotation)
        {
            characterMovement.rotation = newRotation;
        }

        /// <summary>
        /// Orient the character's towards the given direction using rotationRate as the rate of rotation change.
        /// </summary>
        /// <param name="worldDirection">The target direction in world space.</param>
        /// <param name="isPlanar">If True, the rotation will be performed on the Character's plane (defined by its up-axis).</param>

        public virtual void RotateTowards(Vector3 worldDirection, bool isPlanar = true)
        {
            characterMovement.RotateTowards(worldDirection, rotationRate * deltaTime, isPlanar);
        }

        /// <summary>
        /// Orient the character's towards the given direction using rotationRate as the rate of rotation change.
        /// </summary>
        /// <param name="worldDirection">The target direction in world space.</param>
        /// <param name="isPlanar">If True, the rotation will be performed on the Character's plane (defined by its up-axis).</param>

        public virtual void RotateTowardsWithSlerp(Vector3 worldDirection, bool isPlanar = true)
        {
            Vector3 characterUp = GetUpVector();

            if (isPlanar)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection.isZero())
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            characterMovement.rotation =
                Quaternion.Slerp(characterMovement.rotation, targetRotation, rotationRate * Mathf.Deg2Rad * deltaTime);
        }

        /// <summary>
        /// Append root motion rotation to Character's rotation.
        /// </summary>

        protected virtual void RotateWithRootMotion()
        {
            if (_rotationMode == RotationMode.OrientWithRootMotion && rootMotionController != null)
                characterMovement.rotation *= rootMotionController.animDeltaRotation;
        }

        /// <summary>
        /// Sets the yaw value.
        /// This will reset the current pitch and roll values.
        /// </summary>

        public virtual void SetYaw(float value)
        {
            characterMovement.rotation = Quaternion.Euler(0.0f, value, 0.0f);
        }

        /// <summary>
        /// The Character's current up vector.
        /// </summary>

        public virtual Vector3 GetUpVector()
        {
            return transform.up;
        }

        /// <summary>
        /// The Character's current right vector.
        /// </summary>

        public virtual Vector3 GetRightVector()
        {
            return transform.right;
        }

        /// <summary>
        /// The Character's current forward vector.
        /// </summary>

        public virtual Vector3 GetForwardVector()
        {
            return transform.forward;
        }

        /// <summary>
        /// The current relative velocity of the Character.
        /// The velocity is relative because it won't track movements to the transform that happen outside of this,
        /// e.g. character parented under another moving Transform, such as a moving vehicle.
        /// </summary>

        public virtual Vector3 GetVelocity()
        {
            return characterMovement.velocity;
        }

        /// <summary>
        /// Sets the character's velocity.
        /// </summary>

        public virtual void SetVelocity(Vector3 newVelocity)
        {
            characterMovement.velocity = newVelocity;
        }

        /// <summary>
        /// The Character's current speed.
        /// </summary>

        public virtual float GetSpeed()
        {
            return characterMovement.velocity.magnitude;
        }

        /// <summary>
        /// Returns the Character's animation velocity if using root motion, otherwise returns Character's current velocity.
        /// </summary>

        public virtual Vector3 GetRootMotionVelocity()
        {
            if (useRootMotion && rootMotionController)
                return rootMotionController.animRootMotionVelocity;

            return characterMovement.velocity;
        }

        /// <summary>
        /// Returns the Character's current rotation mode.
        /// </summary>

        public virtual RotationMode GetRotationMode()
        {
            return _rotationMode;
        }

        /// <summary>
        /// Sets the Character's current rotation mode:
        ///     - None:                        Disables character's rotation.
        ///     - OrientToMovement:            Orient the Character towards the given input move direction vector, using rotationRate as the rate of rotation change.
        ///     - OrientToCameraViewDirection: Rotates the character towards the camera's current view direction (eg: forward vector), using rotationRate as the rate of rotation change.
        ///     OrientWithRootMotion:          Append root motion rotation to Character's rotation.
        /// </summary>

        public virtual void SetRotationMode(RotationMode rotationMode)
        {
            _rotationMode = rotationMode;
        }

        /// <summary>
        /// The Character's current movement mode.
        /// </summary>

        public virtual MovementMode GetMovementMode()
        {
            return _movementMode;
        }

        /// <summary>
        /// Change movement mode.
        /// The new custom sub-mode (newCustomMode), is only applicable if newMovementMode == Custom.
        ///
        /// Trigger OnMovementModeChanged event.
        /// </summary>

        public virtual void SetMovementMode(MovementMode newMovementMode, int newCustomMode = 0)
        {
            // Do nothing if nothing is changing

            if (newMovementMode == _movementMode)
            {
                // Allow changes in custom sub-modes

                if (newMovementMode != MovementMode.Custom || newCustomMode == _customMovementMode)
                    return;
            }

            // Performs movement mode change

            MovementMode prevMovementMode = _movementMode;
            int prevCustomMode = _customMovementMode;

            _movementMode = newMovementMode;
            _customMovementMode = newCustomMode;

            OnMovementModeChanged(prevMovementMode, prevCustomMode);
        }

        /// <summary>
        /// Called after MovementMode has changed.
        /// Does special handling for starting certain modes, eg: enable / disable ground constraint, etc.
        /// If overridden, must call base.OnMovementModeChanged.
        /// </summary>

        protected virtual void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Perform additional tasks on mode change

            switch (_movementMode)
            {
                case MovementMode.None:
                {
                    // Entering None mode...

                    // Disable Character's movement and clear any pending forces

                    characterMovement.velocity = Vector3.zero;
                    characterMovement.ClearAccumulatedForces();

                    break;
                }

                case MovementMode.Walking:
                {
                    // Entering Walking mode...

                    // Reset jump count and clear apex notification flag

                    _jumpCount = 0;
                    notifyJumpApex = false;

                    // If was flying or swimming, enable ground constraint

                    if (prevMovementMode == MovementMode.Flying || prevMovementMode == MovementMode.Swimming)
                        characterMovement.constrainToGround = true;

                    break;
                }

                case MovementMode.Falling:
                {
                    // Entering Falling mode...

                    // If was flying or swimming, enable ground constraint as it could lands on walkable ground

                    if (prevMovementMode == MovementMode.Flying || prevMovementMode == MovementMode.Swimming)
                        characterMovement.constrainToGround = true;

                    break;
                }

                case MovementMode.Swimming:
                {
                    // Entering Swimming mode...

                    // Stop the Character from holding jump.

                    StopJumping();

                    // Disable ground constraint

                    characterMovement.constrainToGround = false;

                    break;
                }

                case MovementMode.Flying:
                {
                    // Entering Flying mode...

                    // Stop the Character from holding jump.

                    StopJumping();

                    // Disable ground constraint

                    characterMovement.constrainToGround = false;

                    break;
                }
            }

            // Left Falling mode, reset falling timer

            if (!IsFalling())
                _fallingTime = 0.0f;

            // Attempts to UnCrouch if not walking

            if (IsCrouching() && !IsWalking())
                StopCrouching();

            // Trigger movement mode changed event

            MovementModeChanged?.Invoke(prevMovementMode, prevCustomMode);
        }

        /// <summary>
        /// Returns true if the Character's movement mode is None (eg: is disabled).
        /// </summary>

        public virtual bool IsDisabled()
        {
            return _movementMode == MovementMode.None;
        }

        /// <summary>
        /// Returns true if the Character is in the 'Walking' movement mode (eg: on walkable ground).
        /// </summary>

        public virtual bool IsWalking()
        {
            return _movementMode == MovementMode.Walking;
        }

        /// <summary>
        /// Returns true if currently falling, eg: on air (not flying) or in not walkable ground.
        /// </summary>

        public virtual bool IsFalling()
        {
            return _movementMode == MovementMode.Falling;
        }

        /// <summary>
        /// Returns true if currently flying (moving through a non-water volume without resting on the ground).
        /// </summary>

        public virtual bool IsFlying()
        {
            return _movementMode == MovementMode.Flying;
        }

        /// <summary>
        /// Returns true if currently swimming (moving through a water volume).
        /// </summary>

        public virtual bool IsSwimming()
        {
            return _movementMode == MovementMode.Swimming;
        }

        /// <summary>
        /// The maximum speed for current movement mode (accounting crouching / sprinting state).
        /// </summary>

        public virtual float GetMaxSpeed()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                {
                    if (IsCrouching())
                        return maxWalkSpeed * crouchingSpeedModifier;

                    return IsSprinting() ? maxWalkSpeed * sprintSpeedModifier : maxWalkSpeed;
                }

                case MovementMode.Falling:
                    return maxWalkSpeed;

                case MovementMode.Swimming:
                    return maxSwimSpeed;

                case MovementMode.Flying:
                    return maxFlySpeed;

                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// The ground speed that we should accelerate up to when walking at minimum analog stick tilt.
        /// </summary>

        public virtual float GetMinAnalogSpeed()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                case MovementMode.Falling:
                    return minAnalogWalkSpeed;

                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// The acceleration for current movement mode.
        /// </summary>

        public virtual float GetMaxAcceleration()
        {
            if (IsFalling())
                return maxAcceleration * airControl;

            return IsSprinting() ? maxAcceleration * sprintAccelerationModifier : maxAcceleration;
        }

        /// <summary>
        /// The braking deceleration for current movement mode.
        /// </summary>

        public virtual float GetMaxBrakingDeceleration()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                    return brakingDecelerationWalking;

                case MovementMode.Falling:
                {
                    // Falling,
                    // BUT ON non-walkable ground, bypass braking deceleration to force slide off

                    return characterMovement.isOnGround ? 0.0f : brakingDecelerationFalling;
                }

                case MovementMode.Swimming:
                    return brakingDecelerationSwimming;

                case MovementMode.Flying:
                    return brakingDecelerationFlying;

                default:
                    return 0.0f;
            }
        }

        /// <summary>
        /// Computes the analog input modifier (0.0f to 1.0f) based on current input vector and desired velocity.
        /// </summary>

        protected virtual float ComputeAnalogInputModifier(Vector3 desiredVelocity)
        {
            float actualMaxSpeed = GetMaxSpeed();

            if (actualMaxSpeed > 0.0f && desiredVelocity.sqrMagnitude > 0.0f)
                return Mathf.Clamp01(desiredVelocity.magnitude / actualMaxSpeed);

            return 0.0f;
        }

        /// <summary>
        /// Apply friction and braking deceleration to given velocity.
        /// Returns modified input velocity.
        /// </summary>

        protected virtual Vector3 ApplyVelocityBraking(Vector3 velocity, float friction, float deceleration)
        {
            // If no friction or no deceleration, return

            bool isZeroFriction = friction == 0.0f;
            bool isZeroBraking = deceleration == 0.0f;

            if (isZeroFriction && isZeroBraking)
                return velocity;

            // Decelerate to brake to a stop

            Vector3 oldVel = velocity;
            Vector3 revAcceleration = isZeroBraking ? Vector3.zero : -deceleration * velocity.normalized;

            // Apply friction and braking

            velocity += (-friction * velocity + revAcceleration) * deltaTime;

            // Don't reverse direction

            if (Vector3.Dot(velocity, oldVel) <= 0.0f)
                return Vector3.zero;

            // Clamp to zero if nearly zero, or if below min threshold and braking

            float sqrSpeed = velocity.sqrMagnitude;
            if (sqrSpeed <= 0.00001f || !isZeroBraking && sqrSpeed <= 0.01f)
                return Vector3.zero;

            return velocity;
        }

        /// <summary>
        /// Calculates a new velocity for the given state, applying the effects of friction or braking friction and acceleration or deceleration.
        /// Calls GetMaxSpeed(), GetMaxAcceleration(), GetBrakingDeceleration().
        /// </summary>

        protected virtual Vector3 CalcVelocity(Vector3 velocity, Vector3 desiredVelocity, float friction, bool isFluid = false)
        {
            // Compute requested move direction

            float desiredSpeed = desiredVelocity.magnitude;
            Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

            // Requested acceleration (factoring analog input)

            float analogInputModifier = ComputeAnalogInputModifier(desiredVelocity);
            Vector3 requestedAcceleration = GetMaxAcceleration() * analogInputModifier * desiredMoveDirection;

            // Actual max speed (factoring analog input)

            float actualMaxSpeed = Mathf.Max(GetMinAnalogSpeed(), GetMaxSpeed() * analogInputModifier);

            // Friction
            // Only apply braking if there is no input acceleration,
            // or we are over our max speed and need to slow down to it

            bool isZeroAcceleration = requestedAcceleration.isZero();
            bool isVelocityOverMax = velocity.isExceeding(actualMaxSpeed);

            if (isZeroAcceleration || isVelocityOverMax)
            {
                // Pre-braking velocity

                Vector3 oldVelocity = velocity;

                // Apply friction and braking

                float actualBrakingFriction = useSeparateBrakingFriction ? brakingFriction : friction;
                velocity = ApplyVelocityBraking(velocity, actualBrakingFriction, GetMaxBrakingDeceleration());

                // Don't allow braking to lower us below max speed if we started above it

                if (isVelocityOverMax && velocity.sqrMagnitude < actualMaxSpeed.square() && Vector3.Dot(requestedAcceleration, oldVelocity) > 0.0f)
                    velocity = oldVelocity.normalized * actualMaxSpeed;
            }
            else
            {
                // Friction, this affects our ability to change direction

                velocity -= (velocity - desiredMoveDirection * velocity.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
            }

            // Apply fluid friction

            if (isFluid)
                velocity *= 1.0f - Mathf.Min(friction * deltaTime, 1.0f);

            // Apply acceleration

            if (!isZeroAcceleration)
            {
                float newMaxSpeed = velocity.isExceeding(actualMaxSpeed) ? velocity.magnitude : actualMaxSpeed;

                velocity += requestedAcceleration * deltaTime;
                velocity = velocity.clampedTo(newMaxSpeed);
            }

            // Return new velocity

            return velocity;
        }

        /// <summary>
        /// Apply a downward force when standing on top of non-kinematic physics objects (if applyStandingDownwardForce == true).
        /// The force applied is: mass * gravity * standingDownwardForceScale
        /// </summary>

        protected virtual void ApplyDownwardsForce()
        {
            Rigidbody groundRigidbody = characterMovement.groundRigidbody;
            if (!groundRigidbody || groundRigidbody.isKinematic)
                return;

            Vector3 downwardForce = mass * GetGravityVector();

            groundRigidbody.AddForceAtPosition(downwardForce * standingDownwardForceScale, GetPosition());
        }

        /// <summary>
        /// Determines the Character's movement (e.g: its velocity) when moving on walkable ground.
        /// </summary>

        protected virtual void Walking(Vector3 desiredVelocity)
        {
            // If using root motion output animation velocity

            if (useRootMotion && rootMotionController)
                characterMovement.velocity = desiredVelocity;
            else
            {
                // Calculate new velocity

                float actualFriction = useSeparateBrakingFriction ? brakingFriction : groundFriction;
                characterMovement.velocity = CalcVelocity(characterMovement.velocity, desiredVelocity, actualFriction);
            }

            // Apply downwards force

            if (applyStandingDownwardForce)
                ApplyDownwardsForce();
        }

        /// <summary>
        /// Returns the Character's gravity vector modified by gravityScale.
        /// </summary>

        public virtual Vector3 GetGravityVector()
        {
            return gravity;
        }

        /// <summary>
        /// Returns the gravity direction (normalized).
        /// </summary>

        public virtual Vector3 GetGravityDirection()
        {
            return gravity.normalized;
        }

        /// <summary>
        /// Sets the Character's gravity vector
        /// </summary>

        public virtual void SetGravityVector(Vector3 newGravityVector)
        {
            _gravity = newGravityVector;
        }
        
        /// <summary>
        /// Toggle gravity acceleration while falling.
        /// </summary>

        public virtual void EnableGravity(bool enable)
        {
            _applyGravity = enable;
        }

        /// <summary>
        /// Apply gravity and clamps the current falling velocity (vertical component) to maxFallSpeed,
        /// or if within a PhysicsVolume, PhysicsVolume.maxFallSpeed
        /// </summary>

        protected virtual Vector3 LimitFallingVelocity(Vector3 currentVelocity)
        {
            // Output velocity

            Vector3 terminalVelocity = currentVelocity;

            // Don't exceed terminal velocity.

            float terminalLimit = maxFallSpeed;
            if (physicsVolume)
                terminalLimit = physicsVolume.maxFallSpeed;

            if (terminalVelocity.sqrMagnitude > terminalLimit.square())
            {
                Vector3 gravityDir = GetGravityVector().normalized;

                if (Vector3.Dot(terminalVelocity, gravityDir) > terminalLimit)
                    terminalVelocity = terminalVelocity.projectedOnPlane(gravityDir) + gravityDir * terminalLimit;
            }

            return terminalVelocity;
        }

        /// <summary>
        /// Determines the Character's movement (e.g: its velocity) when falling on air or sliding off non-walkable ground.
        /// </summary>

        protected virtual void Falling(Vector3 desiredVelocity)
        {
            // Current 'world' up vector defined by gravity direction

            Vector3 worldUp = -1.0f * GetGravityDirection();

            // Update falling time

            _fallingTime += deltaTime;
            
            // On not walkable ground

            if (IsOnGround())
            {
                // If moving into the 'wall', limit contribution

                Vector3 groundNormal = characterMovement.groundNormal;

                if (desiredVelocity.dot(groundNormal) < 0.0f)
                {
                    // Allow movement parallel to the wall, but not into it because that may push us up.

                    groundNormal = groundNormal.projectedOnPlane(worldUp).normalized;
                    desiredVelocity = desiredVelocity.projectedOnPlane(groundNormal);
                }
            }

            // Calc new velocity
            
            // Separate velocity into its components

            Vector3 verticalVelocity = Vector3.Project(characterMovement.velocity, worldUp);
            Vector3 lateralVelocity = characterMovement.velocity - verticalVelocity;

            // Compute new lateral velocity

            float actualFriction = useSeparateBrakingFriction ? brakingFriction : fallingLateralFriction;
            lateralVelocity = CalcVelocity(lateralVelocity, desiredVelocity, actualFriction);

            // Update new velocity

            characterMovement.velocity = lateralVelocity + verticalVelocity;

            // Apply gravity (if enabled)

            if (_applyGravity)
                characterMovement.velocity += GetGravityVector() * deltaTime;

            // Clamp to terminal velocity

            characterMovement.velocity = LimitFallingVelocity(characterMovement.velocity);
        }

        /// <summary>
        /// Determines the Character's movement when 'flying'.
        /// Ground-Unconstrained movement with full desiredVelocity (lateral AND vertical) and gravity-less.
        /// </summary>

        protected virtual void Flying(Vector3 desiredVelocity)
        {
            if (useRootMotion && rootMotionController)
                characterMovement.velocity = desiredVelocity;
            else
            {
                float actualFriction = IsInWater() ? 0.5f * physicsVolume.friction : 0.5f * flyingFriction;

                characterMovement.velocity =
                    CalcVelocity(characterMovement.velocity, desiredVelocity, actualFriction, true);
            }
        }

        /// <summary>
        /// Is the character in a water physics volume ?
        /// </summary>

        public virtual bool IsInWater()
        {
            return physicsVolume && physicsVolume.waterVolume;
        }

        /// <summary>
        /// Attempts to enter Swimming mode.
        /// Called when Character enters a water physics volume.
        /// </summary>

        public virtual void Swim()
        {
            // Is the Character able to swim ?

            if (!canEverSwim)
                return;

            // Already swimming ?

            if (IsSwimming())
                return;

            // Change to Swimming mode

            SetMovementMode(MovementMode.Swimming);
        }

        /// <summary>
        /// Exits swimming mode.
        /// Called when Character leaves a water physics volume.
        /// </summary>

        public virtual void StopSwimming()
        {
            // If Swimming, change to Falling mode

            if (IsSwimming())
                SetMovementMode(MovementMode.Falling);
        }

        /// <summary>
        /// How deep in water the character is immersed.
        /// Returns a float in range 0.0 = not in water, 1.0 = fully immersed.
        /// </summary>

        public virtual float ImmersionDepth()
        {
            if (!IsInWater())
                return 0.0f;

            Vector3 characterUp = GetUpVector();

            Vector3 rayOrigin = GetPosition() + characterUp * characterMovement.height;
            Vector3 rayDirection = -characterUp;

            float rayLength = characterMovement.height;

            BoxCollider waterVolumeCollider = physicsVolume.boxCollider;
            if (waterVolumeCollider.Raycast(new Ray(rayOrigin, rayDirection), out RaycastHit hitInfo, rayLength))
                return 1.0f - Mathf.InverseLerp(0.0f, rayLength, hitInfo.distance);

            return 1.0f;
        }

        /// <summary>
        /// Determines the Character's movement when Swimming through a fluid volume, under the effects of gravity and buoyancy.
        /// Ground-Unconstrained movement with full desiredVelocity (lateral AND vertical) applies gravity but scaled by (1.0f - buoyancy).
        /// </summary>

        protected virtual void Swimming(Vector3 desiredVelocity)
        {
            // Compute actual buoyancy factoring current immersion depth

            float depth = ImmersionDepth();
            float actualBuoyancy = buoyancy * depth;

            // Calculate new velocity

            Vector3 newVelocity = characterMovement.velocity;

            Vector3 worldUp = -1.0f * GetGravityDirection();
            float verticalSpeed = Vector3.Dot(newVelocity, worldUp);

            if (verticalSpeed > maxSwimSpeed * 0.33f && actualBuoyancy > 0.0f)
            {
                // Damp positive vertical speed (out of water)

                verticalSpeed = Mathf.Max(maxSwimSpeed * 0.33f, verticalSpeed * depth * depth);

                newVelocity = newVelocity.projectedOnPlane(worldUp) + worldUp * verticalSpeed;
            }
            else if (depth < 0.65f)
            {
                // Cancel vertical movement (to out of water)

                desiredVelocity = desiredVelocity.projectedOnPlane(worldUp);
            }

            // Using root motion...

            if (useRootMotion && rootMotionController)
            {
                // Preserve current vertical velocity as we want to keep the effect of gravity

                Vector3 verticalVelocity = newVelocity.projectedOn(worldUp);

                // Updates new velocity

                newVelocity = desiredVelocity.projectedOnPlane(worldUp) + verticalVelocity;
            }
            else
            {
                // Actual friction

                float actualFriction =
                    IsInWater() ? 0.5f * physicsVolume.friction * depth : 0.5f * swimmingFriction * depth;

                newVelocity = CalcVelocity(newVelocity, desiredVelocity, actualFriction, true);
            }

            // If swimming freely, apply gravity acceleration scaled by (1.0f - actualBuoyancy)

            Vector3 gravityVector = GetGravityVector();
            Vector3 actualGravity = gravityVector * (1.0f - actualBuoyancy);

            newVelocity += actualGravity * deltaTime;

            // Update velocity

            characterMovement.velocity = newVelocity;
        }

        /// <summary>
        /// Allows to implement a custom movement.
        /// </summary>

        protected virtual void CustomMovementMode(Vector3 desiredVelocity)
        {
            // EMPTY BY DEFAULT
        }

        /// <summary>
        /// Is the Character's sprinting ?
        /// </summary>

        public virtual bool IsSprinting()
        {
            return canEverSprint && _sprintButtonPressed;
        }

        /// <summary>
        /// Request the character to sprint.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public virtual void Sprint()
        {
            bool wasSprinting = IsSprinting();

            if (canEverSprint)
                _sprintButtonPressed = true;

            if (!wasSprinting && IsSprinting())
                OnSprinted();
        }

        /// <summary>
        /// Request the character to stop sprinting.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public virtual void StopSprinting()
        {
            bool wasSprinting = IsSprinting();

            _sprintButtonPressed = false;

            if (wasSprinting && !IsSprinting())
                OnStoppedSprinting();
        }

        /// <summary>
        /// Returns true if character is actually crouching.
        /// </summary>

        public virtual bool IsCrouching()
        {
            return _isCrouching;
        }

        /// <summary>
        /// Request the Character to start crouching.
        /// The request is processed on the next FixedUpdate.
        /// </summary>

        public virtual void Crouch()
        {
            _crouchButtonPressed = true;
        }

        /// <summary>
        /// Request the Character to stop crouching.
        /// The request is processed on the next FixedUpdate.
        /// </summary>

        public virtual void StopCrouching()
        {
            _crouchButtonPressed = false;
        }

        /// <summary>
        /// Determines if the Character is able to crouch in its current movement mode.
        /// Defaults to Walking mode only.
        /// </summary>

        protected virtual bool CanCrouch()
        {
            return canEverCrouch && IsWalking();
        }

        /// <summary>
        /// Determines if the Character is able to un crouch.
        /// Eg. Check if there's room to expand capsule, etc.
        /// </summary>

        public virtual bool CanUnCrouch()
        {
            bool overlapped = characterMovement.CheckHeight(unCrouchedHeight);

            return !overlapped;
        }

        /// <summary>
        /// Handle crouching state, e.g. crouch / un crouch logic.
        /// </summary>

        protected virtual void Crouching()
        {
            // Wants to crouch and not already crouching ?

            if (_crouchButtonPressed && !IsCrouching())
            {
                // Its allowed to crouch ?

                if (!CanCrouch())
                    return;

                // Do crouch

                characterMovement.SetHeight(crouchedHeight);
                _isCrouching = true;

                // Trigger Crouched event

                OnCrouched();
            }
            else if (IsCrouching() && _crouchButtonPressed == false)
            {
                // Can UnCrouch ?

                if (!CanUnCrouch())
                    return;

                // UnCrouch

                characterMovement.SetHeight(unCrouchedHeight);
                _isCrouching = false;

                // Trigger UnCrouched event

                OnUnCrouched();
            }
        }

        /// <summary>
        /// Is the Character jumping ?
        /// </summary>

        public virtual bool IsJumping()
        {
            return _isJumping;
        }

        /// <summary>
        /// Start a jump.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>

        public void Jump()
        {
            _jumpButtonPressed = true;
        }

        /// <summary>
        /// Stop the Character from jumping.
        /// Call this from an input event (such as a button 'up' event).
        /// </summary>

        public void StopJumping()
        {
            // Input state

            _jumpButtonPressed = false;
            _jumpButtonHeldDownTime = 0.0f;

            // Jump holding state

            _isJumping = false;
            _jumpHoldTime = 0.0f;
        }

        /// <summary>
        /// Returns the current jump count.
        /// </summary>

        public virtual int GetJumpCount()
        {
            return _jumpCount;
        }

        /// <summary>
        /// Determines if the Character is able to perform the requested jump.
        /// </summary>

        public virtual bool CanJump()
        {
            // Is character even able to jump ?

            if (!canEverJump)
                return false;

            // Can jump while crouching ?

            if (IsCrouching() && !jumpWhileCrouching)
                return false;

            // Cant jump if no jumps available

            if (jumpMaxCount == 0 || _jumpCount >= jumpMaxCount)
                return false;

            // Is fist jump ?

            if (_jumpCount == 0)
            {
                // On first jump,
                // can jump if is walking or is falling BUT withing post grounded time

                bool canJump = IsWalking() ||
                               IsFalling() && jumpPostGroundedTime > 0.0f && fallingTime < jumpPostGroundedTime;

                // Missed post grounded time ?

                if (IsFalling() && !canJump)
                {
                    // Missed post grounded time,
                    // can jump if have any 'in-air' jumps but the first jump counts as the in-air jump

                    canJump = jumpMaxCount > 1;
                    if (canJump)
                        _jumpCount++;
                }

                return canJump;
            }

            // In air jump conditions...

            return IsFalling();
        }

        /// <summary>
        /// Determines the jump impulse vector.
        /// </summary>

        protected virtual Vector3 CalcJumpImpulse()
        {
            Vector3 characterUp = GetUpVector();

            float verticalSpeed = Vector3.Dot(GetVelocity(), characterUp);
            float actualJumpImpulse = Mathf.Max(verticalSpeed, jumpImpulse);

            return characterUp * actualJumpImpulse;
        }

        /// <summary>
        /// Attempts to perform a requested jump.
        /// </summary>

        protected virtual void DoJump()
        {
            // Update held down timer

            if (_jumpButtonPressed)
                _jumpButtonHeldDownTime += deltaTime;

            // Wants to jump and not already jumping..

            if (_jumpButtonPressed && !IsJumping())
            {
                // If jumpPreGroundedTime is enabled,
                // allow to jump only if held down time is less than tolerance

                if (jumpPreGroundedTime > 0.0f)
                {
                    bool canJump = _jumpButtonHeldDownTime <= jumpPreGroundedTime;
                    if (!canJump)
                        return;
                }

                // Can perform the requested jump ?

                if (CanJump())
                {
                    // Jump!

                    SetMovementMode(MovementMode.Falling);

                    characterMovement.PauseGroundConstraint();
                    characterMovement.LaunchCharacter(CalcJumpImpulse(), true);

                    _jumpCount++;
                    _isJumping = true;

                    // Trigger Jumped event

                    OnJumped();
                }
            }
        }

        /// <summary>
        /// Handle jumping state.
        /// Eg: check input, perform jump hold (jumpMaxHoldTime > 0), etc. 
        /// </summary>

        protected virtual void Jumping()
        {
            // Is character allowed to jump ?

            if (!canEverJump)
            {
                // If not allowed but was jumping, stop jump

                if (IsJumping())
                    StopJumping();

                return;
            }

            // Check jump input state and attempts to do the requested jump

            DoJump();

            // Perform jump hold, applies an opposite gravity force proportional to _jumpHoldTime.

            if (IsJumping() && _jumpButtonPressed && jumpMaxHoldTime > 0.0f && _jumpHoldTime < jumpMaxHoldTime)
            {
                Vector3 actualGravity = GetGravityVector();

                float actualGravityMagnitude = actualGravity.magnitude;
                Vector3 actualGravityDirection = actualGravityMagnitude > 0.0f
                    ? actualGravity / actualGravityMagnitude
                    : Vector3.zero;

                float jumpProgress = Mathf.InverseLerp(0.0f, jumpMaxHoldTime, _jumpHoldTime);
                float proportionalForce = Mathf.LerpUnclamped(actualGravityMagnitude, 0.0f, jumpProgress);

                Vector3 proportionalJumpForce = -actualGravityDirection * proportionalForce;
                characterMovement.AddForce(proportionalJumpForce);

                _jumpHoldTime += deltaTime;
            }

            // Should notify jump apex ?

            if (!notifyJumpApex)
                return;

            // Notify jump apex (eg: a change in vertical speed from positive to negative)

            Vector3 upAxis = -GetGravityVector();
            float verticalSpeed = Vector3.Dot(GetVelocity(), upAxis.normalized);

            if (verticalSpeed < 0.0f)
            {
                OnReachedJumpApex();
                notifyJumpApex = false;
            }
        }

        /// <summary>
        /// Calculate the desired velocity for current movement mode.
        /// </summary>

        protected virtual Vector3 CalcDesiredVelocity()
        {
            // Current movement direction

            Vector3 movementDirection = GetMovementDirection();

            // The desired velocity from animation (if using root motion) or from input movement vector

            Vector3 desiredVelocity = useRootMotion && rootMotionController
                ? rootMotionController.animRootMotionVelocity
                : movementDirection * GetMaxSpeed();

            // Return desired velocity (constrained to constraint plane if any)

            return characterMovement.ConstrainVectorToPlane(desiredVelocity);
        }

        /// <summary>
        /// Perform character's movement based on its current MovementMode.
        /// </summary>

        protected virtual void Move()
        {
            // If Character movement is disabled, return

            if (IsDisabled())
                return;

            // Toggle walking / falling mode based on ground status

            if (IsWalking() && !characterMovement.isGrounded)
                SetMovementMode(MovementMode.Falling);

            if (IsFalling() && characterMovement.isGrounded)
                SetMovementMode(MovementMode.Walking);
            
            // Compute new velocity based on Character's movement mode

            Vector3 desiredVelocity = CalcDesiredVelocity();

            switch (_movementMode)
            {
                case MovementMode.None:
                    characterMovement.velocity = Vector3.zero;
                    break;

                case MovementMode.Walking:
                    Walking(desiredVelocity);
                    break;

                case MovementMode.Falling:
                    Falling(desiredVelocity);
                    break;

                case MovementMode.Flying:
                    Flying(desiredVelocity);
                    break;

                case MovementMode.Swimming:
                    Swimming(desiredVelocity);
                    break;

                case MovementMode.Custom:
                    CustomMovementMode(desiredVelocity);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Move the character (perform collision constrained movement) with velocity updated by movement mode

            characterMovement.Move(deltaTime);

            // Handle crouching state

            Crouching();

            // Handle jumping state

            Jumping();
        }

        /// <summary>
        /// Allows to implement a custom rotation mode.
        /// </summary>

        protected virtual void CustomRotationMode()
        {
            // EMPTY BY DEFAULT
        }

        /// <summary>
        /// Updates the Character's rotation based on its current RotationMode.
        /// </summary>

        protected virtual void UpdateRotation()
        {
            // If Character movement is disabled, return

            if (IsDisabled())
                return;

            // Should update Character's rotation ?

            RotationMode rotationMode = GetRotationMode();

            switch (rotationMode)
            {
                case RotationMode.None:
                    return;

                case RotationMode.OrientToMovement:
                {
                    // Orient towards current movement direction vector

                    RotateTowards(_movementDirection);

                    break;
                }

                case RotationMode.OrientToCameraViewDirection:
                {
                    // Orient towards camera view direction

                    if (camera)
                        RotateTowards(cameraTransform.forward);

                    break;
                }

                case RotationMode.Custom:
                {
                    // Custom rotation mode

                    CustomRotationMode();
                    break;
                }
            }
        }

        /// <summary>
        /// Update character's state. Eg: Update its rotation, call move, etc.
        /// This MUST be called after physics simulation so the character can read world current state (eg: OnLateFixedUpdate)
        /// This allows you to simulate a character manually (eg: autoSimulation = false).
        /// </summary>

        public virtual void Simulate(float deltaTime)
        {
            // Assign the used deltaTime

            this.deltaTime = deltaTime;

            // Update character's rotation

            UpdateRotation();

            // Append input rotation (eg: AddYawInput, etc)

            ConsumeRotationInput();

            // Moves the character

            Move();

            // Update active physics volume

            UpdatePhysicsVolumes();
        }

        /// <summary>
        /// The current movement direction (in world space), eg: the movement direction used to move this Character.
        /// </summary>

        public Vector3 GetMovementDirection()
        {
            return _movementDirection;
        }

        /// <summary>
        /// Assigns the Character's movement direction (in world space), eg: our desired movement direction vector.
        /// </summary>
        /// <param name="movementDirection">The movement direction in world space. Typical from input</param>

        public void SetMovementDirection(Vector3 movementDirection)
        {
            _movementDirection = movementDirection;
        }
        
        /// <summary>
        /// Amount to add to Yaw (up axis).
        /// </summary>

        public virtual void AddYawInput(float value)
        {
            _rotationInput.y += value;
        }

        /// <summary>
        /// Amount to add to Pitch (right axis).
        /// </summary>

        public virtual void AddPitchInput(float value)
        {
            _rotationInput.x += value;
        }

        /// <summary>
        /// Amount to add to Roll (forward axis).
        /// </summary>

        public virtual void AddRollInput(float value)
        {
            _rotationInput.z += value;
        }

        /// <summary>
        /// Append input rotation (eg: AddPitchInput, AddYawInput, AddRollInput) to character rotation.
        /// </summary>

        protected virtual void ConsumeRotationInput()
        {
            // If Character movement is disabled, return

            if (IsDisabled())
                return;

            // Apply rotation input (if any)

            if (_rotationInput != Vector3.zero)
            {
                // Consumes rotation input (e.g. apply and clear it)

                characterMovement.rotation *= Quaternion.Euler(_rotationInput);

                _rotationInput = Vector3.zero;
            }
        }

        /// <summary>
        /// Initialize player InputActions (if any).
        /// E.g. Subscribe to input action events and enable input actions here.
        /// </summary>
        
        protected virtual void InitPlayerInput()
        {
            // Attempts to cache Character InputActions (if any)

            if (inputActions == null)
                return;
            
            // Movement input action (no handler, this is polled, e.g. GetMovementInput())

            movementInputAction = inputActions.FindAction("Movement");
            movementInputAction?.Enable();

            // Setup Sprint input action handlers

            sprintInputAction = inputActions.FindAction("Sprint");
            if (sprintInputAction != null)
            {
                sprintInputAction.started += OnSprint;
                sprintInputAction.performed += OnSprint;
                sprintInputAction.canceled += OnSprint;

                sprintInputAction.Enable();
            }
            
            // Setup Crouch input action handlers

            crouchInputAction = inputActions.FindAction("Crouch");
            if (crouchInputAction != null)
            {
                crouchInputAction.started += OnCrouch;
                crouchInputAction.performed += OnCrouch;
                crouchInputAction.canceled += OnCrouch;

                crouchInputAction.Enable();
            }

            // Setup Jump input action handlers

            jumpInputAction = inputActions.FindAction("Jump");
            if (jumpInputAction != null)
            {
                jumpInputAction.started += OnJump;
                jumpInputAction.performed += OnJump;
                jumpInputAction.canceled += OnJump;

                jumpInputAction.Enable();
            }
        }

        /// <summary>
        /// Unsubscribe from input action events and disable input actions.
        /// </summary>

        protected virtual void DeinitPlayerInput()
        {
            // Unsubscribe from input action events and disable input actions

            if (movementInputAction != null)
            {
                movementInputAction.Disable();
                movementInputAction = null;
            }

            if (sprintInputAction != null)
            {
                sprintInputAction.started -= OnSprint;
                sprintInputAction.performed -= OnSprint;
                sprintInputAction.canceled -= OnSprint;

                sprintInputAction.Disable();
                sprintInputAction = null;
            }

            if (crouchInputAction != null)
            {
                crouchInputAction.started -= OnCrouch;
                crouchInputAction.performed -= OnCrouch;
                crouchInputAction.canceled -= OnCrouch;

                crouchInputAction.Disable();
                crouchInputAction = null;
            }

            if (jumpInputAction != null)
            {
                jumpInputAction.started -= OnJump;
                jumpInputAction.performed -= OnJump;
                jumpInputAction.canceled -= OnJump;

                jumpInputAction.Disable();
                jumpInputAction = null;
            }
        }

        /// <summary>
        /// Handle Player input, only if actions are assigned (eg: actions != null).
        /// </summary>

        protected virtual void HandleInput()
        {
            // Should this character handle input ?

            if (inputActions == null)
                return;

            // Poll movement InputAction

            Vector2 movementInput = GetMovementInput();

            if (camera)
            {
                // If Camera is assigned, add input movement relative to camera look direction

                Vector3 movementDirection = Vector3.zero;

                movementDirection += Vector3.right * movementInput.x;
                movementDirection += Vector3.forward * movementInput.y;
                
                movementDirection = movementDirection.relativeTo(cameraTransform);

                SetMovementDirection(movementDirection);

            }
            else
            {
                // If Camera is not assigned, add movement input relative to world axis

                Vector3 movementDirection = Vector3.zero;

                movementDirection += Vector3.right * movementInput.x;
                movementDirection += Vector3.forward * movementInput.y;

                SetMovementDirection(movementDirection);
            }
        }

        /// <summary>
        /// Helper method used to feed the Character's animator.
        /// Optional as you can prefer to externally animate your character.
        /// </summary>

        protected virtual void Animate()
        {
            // EMPTY BY DEFAULT
        }

        /// <summary>
        /// Our Reset method. Set this default values.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnReset()
        {
            _inputActions = null;

            _defaultMovementMode = MovementMode.Walking;

            _rotationRate = 540.0f;
            _rotationMode = RotationMode.OrientToMovement;

            _maxWalkSpeed = 6.0f;
            _minAnalogWalkSpeed = 0.0f;
            _maxAcceleration = 20.0f;
            _brakingDecelerationWalking = 20.0f;
            _groundFriction = 8.0f;

            _maxFallSpeed = 40.0f;
            _brakingDecelerationFalling = 0.0f;
            _fallingLateralFriction = 0.3f;
            _airControl = 0.3f;

            _maxFlySpeed = 10.0f;
            _brakingDecelerationFlying = 0.0f;
            _flyingFriction = 0.5f;

            _canEverSwim = true;
            _maxSwimSpeed = 3.0f;
            _brakingDecelerationSwimming = 0.0f;
            _swimmingFriction = 0.5f;
            _buoyancy = 1.0f;

            _useSeparateBrakingFriction = false;
            _brakingFriction = 0.0f;

            _canEverSprint = true;
            _sprintSpeedModifier = 1.5f;
            _sprintAccelerationModifier = 1.0f;

            _canEverCrouch = true;
            _unCrouchedHeight = 2.0f;
            _crouchedHeight = 1.25f;
            _crouchingSpeedModifier = 0.5f;

            _canEverJump = true;
            _jumpWhileCrouching = false;
            _jumpMaxCount = 1;
            _jumpImpulse = 6.0f;
            _jumpMaxHoldTime = 0.35f;
            _jumpPreGroundedTime = 0.15f;
            _jumpPostGroundedTime = 0.15f;

            _gravity = Physics.gravity;
            _gravityScale = 1.0f;

            _useRootMotion = false;

            _mass = 1.0f;
            _impartPlatformVelocity = false;
            _impartPlatformMovement = false;
            _impartPlatformRotation = false;
            _enablePhysicsInteraction = false;
            _applyPushForceToCharacters = false;
            _applyStandingDownwardForce = false;
            _pushForceScale = 1.0f;
            _standingDownwardForceScale = 1.0f;
        }

        /// <summary>
        /// Our OnValidate method.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnOnValidate()
        {
            rotationRate = _rotationRate;

            maxWalkSpeed = _maxWalkSpeed;
            minAnalogWalkSpeed = _minAnalogWalkSpeed;
            maxAcceleration = _maxAcceleration;
            brakingDecelerationWalking = _brakingDecelerationWalking;
            groundFriction = _groundFriction;

            maxFallSpeed = _maxFallSpeed;
            brakingDecelerationFalling = _brakingDecelerationFalling;
            fallingLateralFriction = _fallingLateralFriction;
            airControl = _airControl;

            maxFlySpeed = _maxFlySpeed;
            brakingDecelerationFlying = _brakingDecelerationFlying;
            flyingFriction = _flyingFriction;

            maxSwimSpeed = _maxSwimSpeed;
            brakingDecelerationSwimming = _brakingDecelerationSwimming;
            swimmingFriction = _swimmingFriction;
            buoyancy = _buoyancy;

            brakingFriction = _brakingFriction;

            sprintSpeedModifier = _sprintSpeedModifier;
            sprintAccelerationModifier = _sprintAccelerationModifier;

            unCrouchedHeight = _unCrouchedHeight;
            crouchedHeight = _crouchedHeight;
            crouchingSpeedModifier = _crouchingSpeedModifier;

            jumpMaxCount = _jumpMaxCount;
            jumpImpulse = _jumpImpulse;
            jumpMaxHoldTime = _jumpMaxHoldTime;
            jumpPreGroundedTime = _jumpPreGroundedTime;
            jumpPostGroundedTime = _jumpPostGroundedTime;

            gravityScale = _gravityScale;

            mass = _mass;
            impartPlatformVelocity = _impartPlatformVelocity;
            impartPlatformMovement = _impartPlatformMovement;
            impartPlatformRotation = _impartPlatformRotation;
            enablePhysicsInteraction = _enablePhysicsInteraction;
            applyPushForceToCharacters = _applyPushForceToCharacters;
            applyStandingDownwardForce = _applyStandingDownwardForce;
            pushForceScale = _pushForceScale;
            standingDownwardForceScale = _standingDownwardForceScale;
        }

        /// <summary>
        /// Called when the script instance is being loaded (Awake).
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnAwake()
        {
            // Cache components

            _transform = GetComponent<Transform>();
            _characterMovement = GetComponent<CharacterMovement>();
            _animator = GetComponentInChildren<Animator>();
            _rootMotionController = GetComponentInChildren<RootMotionController>();

            // Enable late fixed update (default)
            
            enableLateFixedUpdate = true;
        }

        /// <summary>
        /// Our OnEnable method.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnOnEnable()
        {
            // Setup player InputActions (if any).

            InitPlayerInput();
            
            // Subscribe to CharacterMovement events

            characterMovement.Collided += OnCollided;
            characterMovement.FoundGround += OnFoundGround;

            // If enabled, start LateFixedUpdate coroutine

            if (_enableLateFixedUpdateCoroutine)
                EnableLateFixedUpdate(true);

            // Update CharacterMovement flags

            characterMovement.enablePhysicsInteraction = enablePhysicsInteraction;
            characterMovement.physicsInteractionAffectsCharacters = applyPushForceToCharacters;
            characterMovement.pushForceScale = pushForceScale;

            characterMovement.impartPlatformMovement = impartPlatformMovement;
            characterMovement.impartPlatformRotation = impartPlatformRotation;
            characterMovement.impartPlatformVelocity = impartPlatformVelocity;
        }

        /// <summary>
        /// Called when the behaviour becomes disabled (OnDisable).
        /// If overridden, must call base method in order to fully de-initialize the class.
        /// </summary>

        protected virtual void OnOnDisable()
        {
            // Unsubscribe from input action events and disable input actions (if any, e.g. inputActions != null)

            DeinitPlayerInput();

            // Unsubscribe from CharacterMovement events

            characterMovement.Collided -= OnCollided;
            characterMovement.FoundGround -= OnFoundGround;

            // If enabled, stops LateFixedUpdate coroutine

            if (_enableLateFixedUpdateCoroutine)
                EnableLateFixedUpdate(false);
        }

        /// <summary>
        /// Called on the frame when a script is enabled just before any of the Update methods are called the first time (Start).
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnStart()
        {
            // Sets character's startup movement mode

            SetMovementMode(defaultMovementMode);
        }

        /// <summary>
        /// Our FixedUpdate method.
        /// </summary>

        protected virtual void OnFixedUpdate()
        {
            // EMPTY BY DEFAULT
        }

        /// <summary>
        /// Our LateFixedUpdate method. E.g called AFTER Physics internal update.
        /// </summary>

        protected virtual void OnLateFixedUpdate()
        {
            // Simulate this character

            Simulate(Time.deltaTime);
        }

        /// <summary>
        /// Our Update method.
        /// </summary>

        protected virtual void OnUpdate()
        {
            HandleInput();

            Animate();

            RotateWithRootMotion();
        }

        /// <summary>
        /// Our LateUpdate method.
        /// </summary>

        protected virtual void OnLateUpdate()
        {
            // EMPTY BY DEFAULT
        }

        #endregion

        #region MONOBEHAVIOR

        protected virtual void Reset()
        {
            OnReset();
        }

        protected virtual void OnValidate()
        {
            OnOnValidate();
        }

        protected virtual void Awake()
        {
            OnAwake();
        }

        protected virtual void OnEnable()
        {
            OnOnEnable();
        }

        protected virtual void OnDisable()
        {
            OnOnDisable();
        }

        protected virtual void Start()
        {
            OnStart();
        }

        protected virtual void FixedUpdate()
        {
            OnFixedUpdate();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            AddPhysicsVolume(other);
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            RemovePhysicsVolume(other);
        }

        private IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitTime = new WaitForFixedUpdate();

            while (true)
            {
                yield return waitTime;

                OnLateFixedUpdate();
            }
        }

        protected virtual void Update()
        {
            OnUpdate();
        }

        protected virtual void LateUpdate()
        {
            OnLateUpdate();
        }

        #endregion
    }
}