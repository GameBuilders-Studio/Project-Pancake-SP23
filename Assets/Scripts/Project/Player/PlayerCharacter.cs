using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement
{
    [RequireComponent(typeof(CharacterMovement), typeof(PlayerInteraction))]
    public class PlayerCharacter : MonoBehaviour
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Player Input associated with this Character." +
                 " If not assigned, this Character wont process any input so you can externally take control of this Character (e.g. a Controller).")]
        [SerializeField]
        private PlayerInputHandler _input;

        [SerializeField]
        private PlayerInteraction _playerInteraction;

        private RotationMode _rotationMode = RotationMode.None;

        [Space(15f)]
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
        [SerializeField]
        private bool _useSeparateBrakingFriction;

        [SerializeField]
        private float _brakingFriction;

        [Space(15f)]
        [SerializeField]
        private AnimationCurve _dashCurve;

        [SerializeField]
        private float _dashDuration;

        [SerializeField]
        private float _dashDistance;

        [SerializeField]
        private float _dashCooldown;

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
        private Camera _camera;

        #endregion

        #region FIELDS

        private bool _enableLateFixedUpdateCoroutine;
        private Coroutine _lateFixedUpdateCoroutine;

        protected List<PhysicsVolume> _volumes = new();

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

        private Vector3 _movementDirection = Vector3.zero;

        private float _dashTime = 0.0f;
        private float _dashProgress = 0.0f;
        private float _dashCooldownTime = 0.0f;

        private float _deltaTime;

        #endregion

        #region PROPERTIES

        public float deltaTime
        {
            get => _deltaTime == 0.0f ? Time.deltaTime : _deltaTime;
            set => _deltaTime = value;
        }

        public new Camera camera
        {
            get => _camera;
            set => _camera = value;
        }

        public Transform cameraTransform
        {
            get
            {
                if (_camera != null)
                    _cameraTransform = _camera.transform;

                return _cameraTransform;
            }
        }

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

        public float rotationRate
        {
            get => _rotationRate;
            set => _rotationRate = Mathf.Max(0.0f, value);
        }

        public MovementMode defaultMovementMode
        {
            get => _defaultMovementMode;
            set => _defaultMovementMode = value;
        }

        public float maxWalkSpeed
        {
            get => _maxWalkSpeed;
            set => _maxWalkSpeed = Mathf.Max(0.0f, value);
        }

        public float minAnalogWalkSpeed
        {
            get => _minAnalogWalkSpeed;
            set => _minAnalogWalkSpeed = Mathf.Max(0.0f, value);
        }

        public float maxAcceleration
        {
            get => _maxAcceleration;
            set => _maxAcceleration = Mathf.Max(0.0f, value);
        }

        public float brakingDecelerationWalking
        {
            get => _brakingDecelerationWalking;
            set => _brakingDecelerationWalking = Mathf.Max(0.0f, value);
        }

        public float groundFriction
        {
            get => _groundFriction;
            set => _groundFriction  = Mathf.Max(0.0f, value);
        }

        public bool canDash
        {
            get => _dashCooldown < 0.0f;
        }

        public float maxFallSpeed
        {
            get => _maxFallSpeed;
            set => _maxFallSpeed = Mathf.Max(0.0f, value);
        }

        public float brakingDecelerationFalling
        {
            get => _brakingDecelerationFalling;
            set => _brakingDecelerationFalling = Mathf.Max(0.0f, value);
        }

        public float fallingLateralFriction
        {
            get => _fallingLateralFriction;
            set => _fallingLateralFriction = Mathf.Max(0.0f, value);
        }

        public float fallingTime => _fallingTime;

        public float airControl
        {
            get => _airControl;
            set => _airControl = Mathf.Clamp01(value);
        }

        public bool useSeparateBrakingFriction
        {
            get => _useSeparateBrakingFriction;
            set => _useSeparateBrakingFriction = value;
        }

        public float brakingFriction
        {
            get => _brakingFriction;
            set => _brakingFriction = Mathf.Max(0.0f, value);
        }

        public Vector3 gravity
        {
            get => _gravity * _gravityScale;
            set => _gravity = value;
        }

        public float gravityScale
        {
            get => _gravityScale;
            set => _gravityScale = value;
        }

        public bool useRootMotion
        {
            get => _useRootMotion;
            set => _useRootMotion = value;
        }

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

        public bool applyStandingDownwardForce
        {
            get => _applyStandingDownwardForce;
            set => _applyStandingDownwardForce = value;
        }

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

        public float standingDownwardForceScale
        {
            get => _standingDownwardForceScale;
            set => _standingDownwardForceScale = Mathf.Max(0.0f, value);
        }

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

        protected InputAction movementInputAction { get; set; }
        protected InputAction sprintInputAction { get; set; }
        protected InputAction crouchInputAction { get; set; }
        protected InputAction dashInputAction { get; set; }
        protected InputAction pickUpInputAction { get; set; }
        protected InputAction interactInputAction { get; set; }

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

        protected virtual void OnDash(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                BeginDash();
            } 
        }

        protected virtual void OnPickUp(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                _playerInteraction.OnPickUpPressed();
            }
        }

        protected virtual void OnInteract(InputAction.CallbackContext context)
        {
            if (context.started || context.performed)
            {
                //Player cannot move while interacting with an object
                SetMovementMode(MovementMode.None);
                _playerInteraction.OnInteractStart();
            }

            if (context.canceled)
            {
                //Player can move again after interacting with an object
                SetMovementMode(MovementMode.Walking);
                _playerInteraction.OnInteractEnd();
            }
        }

        #endregion

        #region EVENTS

        public delegate void MovementModeChangedEventHandler(MovementMode prevMovementMode, int prevCustomMode);
        public delegate void DashEventHandler();
        public delegate void WillLandEventHandler();
        public delegate void LandedEventHandler();

        /// <summary>
        /// Event triggered after a MovementMode change.
        /// </summary>
        public event MovementModeChangedEventHandler MovementModeChanged;

        /// <summary>
        /// Event triggered when character dashes.
        /// </summary>
        public event DashEventHandler Dashed;

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
            {
                OnWillLand();
            }
        }

        /// <summary>
        /// Event triggered when character find ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>
        protected virtual void OnFoundGround(ref FindGroundResult foundGround)
        {
            // If found ground is walkable, trigger Landed event
            if (foundGround.isWalkable)
            {
                OnLanded();
            }
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
                {
                    StopCoroutine(_lateFixedUpdateCoroutine);
                }
                _lateFixedUpdateCoroutine = StartCoroutine(LateFixedUpdate());
            }
            else
            {
                if (_lateFixedUpdateCoroutine != null)
                {
                    StopCoroutine(_lateFixedUpdateCoroutine);
                }
            }
        }

        /// <summary>
        /// Adds a force to the Character.
        /// This forces will be accumulated and applied during Move method call.
        /// </summary>
        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            characterMovement.AddForce(force, forceMode);
        }

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

        public virtual void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            characterMovement.CapsuleIgnoreCollision(otherCollider, ignore);
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Dash, etc.
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
        /// Returns the Character's current rotation mode.
        /// </summary>

        public virtual RotationMode GetRotationMode()
        {
            return _rotationMode;
        }

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
            if (newMovementMode == _movementMode)
            {
                if (newMovementMode != MovementMode.Custom || newCustomMode == _customMovementMode)
                {
                    return;
                }
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
                    characterMovement.velocity = Vector3.zero;
                    characterMovement.ClearAccumulatedForces();
                    break;

                case MovementMode.Walking:
                    SetRotationMode(RotationMode.OrientToMovement);
                    break;

                case MovementMode.Falling:
                    break;

                case MovementMode.Dash:
                    SetRotationMode(RotationMode.None);
                    break;
            }

            // Left Falling mode, reset falling timer

            if (!IsFalling())
            {
                _fallingTime = 0.0f;
            }

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
        /// The maximum speed for current movement mode (accounting crouching / sprinting state).
        /// </summary>
        public virtual float GetMaxSpeed()
        {
            switch (_movementMode)
            {
                case MovementMode.Walking:
                    return maxWalkSpeed;

                case MovementMode.Falling:
                    return maxWalkSpeed;

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
            {
                return maxAcceleration * airControl;
            }
            return maxAcceleration;
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
            {
                return Mathf.Clamp01(desiredVelocity.magnitude / actualMaxSpeed);
            }
                

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
            {
                return velocity;
            }

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
            {
                characterMovement.velocity += GetGravityVector() * deltaTime;
            }

            // Clamp to terminal velocity
            characterMovement.velocity = LimitFallingVelocity(characterMovement.velocity);
        }

        /// <summary>
        /// Allows to implement a custom movement.
        /// </summary>
        protected virtual void Dashing(Vector3 desiredVelocity)
        {
            _dashTime += deltaTime;

            float previousDashProgress = _dashProgress;

            _dashProgress = _dashCurve.Evaluate(_dashTime / _dashDuration);

            var horizontalVelocity = GetForwardVector() * (_dashProgress - previousDashProgress) * _dashDistance;
            horizontalVelocity *= 1.0f / deltaTime;

            characterMovement.velocity = new Vector3(
                horizontalVelocity.x,
                characterMovement.velocity.y,
                horizontalVelocity.z
            );

            characterMovement.velocity += GetGravityVector() * deltaTime;
            
            if (_dashProgress >= 1.0f)
            {
                SetMovementMode(MovementMode.Walking);
            }
        }

        /// <summary>
        /// Start a dash.
        /// Call this from an input event (such as a button 'down' event).
        /// </summary>
        public void BeginDash()
        {
            if (_dashCooldownTime > 0.0f)
                return;
            
            _dashTime = 0.0f;
            _dashProgress = 0.0f;
            _dashCooldownTime = _dashCooldown;

            Dashed?.Invoke();

            if (GetMovementDirection() != Vector3.zero)
                SetRotation(Quaternion.LookRotation(GetMovementDirection().normalized, GetUpVector()));

            SetMovementMode(MovementMode.Dash);
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

                case MovementMode.Dash:
                    Dashing(desiredVelocity);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Move the character (perform collision constrained movement) with velocity updated by movement mode

            characterMovement.Move(deltaTime);
        }

        /// <summary>
        /// Updates the Character's rotation based on its current RotationMode.
        /// </summary>

        protected virtual void UpdateRotation()
        {
            // If Character movement is disabled, return

            if (IsDisabled()) { return; }

            // Should update Character's rotation ?

            RotationMode rotationMode = GetRotationMode();

            switch (rotationMode)
            {
                case RotationMode.None:
                    return;

                case RotationMode.OrientToMovement:
                    // Orient towards current movement direction vector
                    RotateTowards(_movementDirection);
                    break;

                case RotationMode.OrientToCameraViewDirection:
                    // Orient towards camera view direction
                    if (camera)
                    {
                        RotateTowards(cameraTransform.forward);
                    }
                    break;
            }
        }

        void UpdateDashTimer()
        {
            _dashCooldownTime = Mathf.MoveTowards(_dashCooldownTime, 0.0f, Time.deltaTime);
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

            // Moves the character
            Move();
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
        /// Initialize player InputActions (if any).
        /// E.g. Subscribe to input action events and enable input actions here.
        /// </summary>

        protected virtual void InitPlayerInput()
        {
            if (_input.Actions == null) { return; }

            var actions = _input.Actions;
            
            movementInputAction = actions.FindAction("Move");
            movementInputAction?.Enable();

            dashInputAction = actions.FindAction("Dash");
            if (dashInputAction != null)
            {
                dashInputAction.started += OnDash;
                dashInputAction.Enable();
            }

            pickUpInputAction = actions.FindAction("PickUp");
            if (pickUpInputAction != null)
            {
                pickUpInputAction.started += OnPickUp;
                pickUpInputAction.Enable();
            }

            interactInputAction = actions.FindAction("Interact");
            if (interactInputAction != null)
            {
                interactInputAction.started += OnInteract;
                interactInputAction.canceled += OnInteract;
                interactInputAction.Enable();
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

            if (dashInputAction != null)
            {
                dashInputAction.started -= OnDash;
                dashInputAction.Disable();
                dashInputAction = null;
            }

            if (pickUpInputAction != null)
            {
                pickUpInputAction.started -= OnPickUp;
                pickUpInputAction.Disable();
                pickUpInputAction = null;
            }

            if (interactInputAction != null)
            {
                interactInputAction.started -= OnInteract;
                interactInputAction.canceled -= OnInteract;
                interactInputAction.Disable();
                interactInputAction = null;
            }
        }

        /// <summary>
        /// Handle Player input, only if actions are assigned (eg: actions != null).
        /// </summary>

        protected virtual void HandleInput()
        {
            // Should this character handle input ?

            if (_input == null) { return; }

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
        /// Our Reset method. Set this default values.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected virtual void OnReset()
        {
            _input = null;

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

            _useSeparateBrakingFriction = false;
            _brakingFriction = 0.0f;

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

            brakingFriction = _brakingFriction;

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

            _playerInteraction = GetComponent<PlayerInteraction>();
            _input.OnActionsAssigned += InitPlayerInput;

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
            UpdateDashTimer();
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

        private IEnumerator LateFixedUpdate()
        {
            WaitForFixedUpdate waitTime = new();

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