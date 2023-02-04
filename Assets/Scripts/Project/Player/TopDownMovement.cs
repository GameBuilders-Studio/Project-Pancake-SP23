using UnityEngine;
using UnityEngine.InputSystem;
using CustomAttributes;

public enum MovementState { Dash, Walk }

[RequireComponent(typeof(CharacterController))]
public class TopDownMovement : MonoBehaviour
{
    [Space(15f)]
    [Tooltip("Input actions associated with this Character")]
    [SerializeField]
    private InputActionAsset _inputActions;

    [Space(15f)]
    [Header("Movement")]
    [SerializeField]
    private float _maxRunSpeed;

    [SerializeField]
    private float _maxAcceleration;

    [SerializeField]
    private float _friction;

    [SerializeField]
    private float _brakingFriction;

    [Space(15f)]
    [CurveRange(0f, 0f, 1f, 1f)]
    [SerializeField]
    private AnimationCurve _dashCurve;

    [SerializeField]
    private float _dashDuration;

    [SerializeField]
    private float _dashDistance;

    [SerializeField]
    private float _gravity;

    [Space(15f)]
    [SerializeField]
    [Tooltip("Turn speed in degrees/second")]
    private float _turnSpeed;

    private float _dashTime = 0.0f;
    private float _dashProgress = 0.0f;

    public InputActionAsset InputActions
    {
        get => _inputActions;
        set => _inputActions = value;
    }
    protected InputAction movementInputAction { get; set; }
    protected InputAction dashInputAction { get; set; }
    protected InputAction interactInputAction { get; set; }

    private CharacterController _character;
    private MovementState _movementState = MovementState.Walk;
    private int platformMask;

    private Vector3 _velocity;
    public Vector3 Velocity
    {
        get => _velocity;
        set => _velocity = value;
    }

    #region INPUT

    protected virtual void EnablePlayerInput()
    {
        if (InputActions == null)
            return;

        // Movement input action (no handler, this is polled, e.g. GetMovementInput())
        movementInputAction = InputActions.FindAction("Movement");
        movementInputAction?.Enable();

        dashInputAction = InputActions.FindAction("Dash");
        if (dashInputAction != null)
        {
            dashInputAction.started += OnDash;
            dashInputAction.Enable();
        }

        interactInputAction = InputActions.FindAction("Dash");
        if (interactInputAction != null)
        {
            interactInputAction.started += OnInteract;
            interactInputAction.Enable();
        }
    }

    protected virtual void DisablePlayerInput()
    {
        if (InputActions == null)
            return;

        if (movementInputAction != null)
        {
            movementInputAction?.Disable();
            movementInputAction = null;
        }

        if (dashInputAction != null)
        {
            dashInputAction.started -= OnDash;
            dashInputAction.Disable();
            dashInputAction = null;
        }

        if (interactInputAction != null)
        {
            interactInputAction.started -= OnInteract;
            interactInputAction.Disable();
            interactInputAction = null;
        }
    }

    protected virtual void OnDash(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            Dash();
    }

    protected virtual void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
            Interact();
    }

    #endregion

    public float GetMaxSpeed()
    {
        return _maxRunSpeed;
    }

    public virtual float GetMaxAcceleration()
    {
        return _maxAcceleration;
    }

    /// <summary>
    /// Apply friction and braking deceleration to given velocity.
    /// Returns modified input velocity.
    /// </summary>
    protected virtual Vector3 ApplyVelocityBraking(
        Vector3 velocity, float friction, float deceleration, float deltaTime)
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
    protected virtual Vector3 CalcVelocity(Vector3 velocity, Vector3 desiredVelocity, float friction, float deltaTime)
    {
        // Compute requested move direction
        float desiredSpeed = desiredVelocity.magnitude;
        Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

        // Requested acceleration (factoring analog input?)
        float analogInputModifier = 1f;
        Vector3 requestedAcceleration = GetMaxAcceleration() * analogInputModifier * desiredMoveDirection;

        // Actual max speed (factoring analog input)
        float actualMaxSpeed = GetMaxSpeed() * analogInputModifier;

        // Friction
        // Only apply braking if there is no input acceleration,
        // or we are over our max speed and need to slow down to it
        bool isZeroAcceleration = requestedAcceleration == Vector3.zero;
        bool isVelocityOverMax = velocity.magnitude > actualMaxSpeed;

        if (isZeroAcceleration || isVelocityOverMax)
        {
            // Pre-braking velocity
            Vector3 oldVelocity = velocity;

            // Apply friction and braking
            velocity = ApplyVelocityBraking(velocity, friction, _brakingFriction, deltaTime);

            // Don't allow braking to lower us below max speed if we started above it
            if (isVelocityOverMax && velocity.sqrMagnitude < Mathf.Pow(actualMaxSpeed, 2f) && Vector3.Dot(requestedAcceleration, oldVelocity) > 0.0f)
                velocity = oldVelocity.normalized * actualMaxSpeed;
        }
        else
        {
            // Friction, this affects our ability to change direction
            velocity -= (velocity - desiredMoveDirection * velocity.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
        }

        // Apply acceleration
        if (!isZeroAcceleration)
        {
            float newMaxSpeed = velocity.magnitude > actualMaxSpeed ? velocity.magnitude : actualMaxSpeed;

            velocity += requestedAcceleration * deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, newMaxSpeed);
        }

        // Return new velocity
        return velocity;
    }


    void Dash()
    {
        _dashTime = 0.0f;
        _dashProgress = 0.0f;
        _movementState = MovementState.Dash;
    }

    void Interact()
    {
        // something here
    }

    public bool IsGrounded()
    {
        return _character.isGrounded;
    }

    void Move(Vector3 velocity)
    {
        velocity.y -= _gravity * Time.deltaTime;
        _character.Move(velocity);
    }

    #region MONOBEHAVIOUR

    void OnEnable()
    {
        EnablePlayerInput();
    }

    void OnDisable()
    {
        DisablePlayerInput();
    }

    void Awake()
    {
        _character = GetComponent<CharacterController>();
        platformMask = ~LayerMask.NameToLayer("Platform");
    }

    protected virtual Vector2 GetMovementInput()
    {
        return movementInputAction?.ReadValue<Vector2>() ?? Vector2.zero;
    }

    public MovementState GetMovementState()
    {
        return _movementState;
    }

    public void SetMovementState(MovementState state)
    {
        _movementState = state;
    }

    public void FaceTowards(Vector3 direction, float degreesPerSecond, bool instant = false)
    {
        direction.y = 0.0f;

        if (direction == Vector3.zero) 
            return;
    
        var radiansPerSecond = Mathf.Deg2Rad * degreesPerSecond * Time.deltaTime;

        var targetDirection = Vector3.RotateTowards(transform.forward, direction, radiansPerSecond, 0.0f);
        
        transform.rotation = Quaternion.LookRotation(targetDirection);
    }

    void Update()
    {
        Vector2 movementInput = GetMovementInput();

        Vector3 movementDirection = Vector3.zero;

        movementDirection += Vector3.right * movementInput.x;
        movementDirection += Vector3.forward * movementInput.y;
        movementDirection.Normalize();

        if (movementDirection != Vector3.zero)
            FaceTowards(movementDirection, _turnSpeed);

        if (_movementState == MovementState.Walk)
        {
            Velocity = CalcVelocity(Velocity, movementDirection * _maxRunSpeed, _friction, Time.deltaTime);
            Move(Velocity * Time.deltaTime);
        }
        else if (_movementState == MovementState.Dash)
        {
            _dashTime += Time.deltaTime;

            var previousDashProgress = _dashProgress;

            _dashProgress = _dashCurve.Evaluate(_dashTime / _dashDuration);

            Velocity = transform.forward * (_dashProgress - previousDashProgress) * _dashDistance;
            
            if (_dashProgress >= 1.0f)
            {
                SetMovementState(MovementState.Walk);
            }

            Move(Velocity);
        }
    }

    #endregion
}
