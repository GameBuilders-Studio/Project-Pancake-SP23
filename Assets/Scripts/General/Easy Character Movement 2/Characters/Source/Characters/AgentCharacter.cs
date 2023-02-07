using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace EasyCharacterMovement
{
    /// <summary>
    /// AgentCharacter.    
    /// The AgentCharacter is a customized Character derived class adding navigation and path finding capabilities (NavMesh, NavMeshAgent).
    /// This allows you to create characters that can intelligently move around the game world, using navigation meshes (player or AI controlled).

    /// By default, this implements a typical click-to-move behavior.
    /// </summary>

    [RequireComponent(typeof(NavMeshAgent))]
    public class AgentCharacter : Character
    {
        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Should the agent brake automatically to avoid overshooting the destination point? \n" +
                 "If true, the agent will brake automatically as it nears the destination.")]
        [SerializeField]
        private bool _autoBraking = true;

        [Tooltip("Distance from target position to start braking.")]
        [SerializeField]
        private float _brakingDistance;

        [Tooltip("Stop within this distance from the target position.")]
        [SerializeField]
        private float _stoppingDistance = 1.0f;

        #endregion

        #region FIELDS

        private NavMeshAgent _agent;
        
        protected bool _mouseButtonPressed;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Cached NavMeshAgent component.
        /// </summary>

        protected NavMeshAgent agent
        {
            get
            {
                if (_agent == null)
                    _agent = GetComponent<NavMeshAgent>();

                return _agent;
            }
        }

        /// <summary>
        /// Should the agent brake automatically to avoid overshooting the destination point?
        /// If this property is set to true, the agent will brake automatically as it nears the destination.
        /// </summary>

        public bool autoBraking
        {
            get => _autoBraking;
            set
            {
                _autoBraking = value;
                
                agent.autoBraking = _autoBraking;
            }
        }

        /// <summary>
        /// Distance from target position to start braking.
        /// </summary>

        public float brakingDistance
        {
            get => _brakingDistance;
            set => _brakingDistance = Mathf.Max(0.0001f, value);
        }

        /// <summary>
        /// The ratio (0 - 1 range) of the agent's remaining distance and the braking distance.
        /// 1 If no auto braking or if agent's remaining distance is greater than brakingDistance.
        /// less than 1, if agent's remaining distance is less than brakingDistance.
        /// </summary>

        public float brakingRatio
        {
            get
            {
                if (!autoBraking)
                    return 1f;

                return agent.hasPath ? Mathf.Clamp(agent.remainingDistance / brakingDistance, 0.1f, 1f) : 1f;
            }
        }

        /// <summary>
        /// Stop within this distance from the target position.
        /// </summary>

        public float stoppingDistance
        {
            get => _stoppingDistance;
            set
            {
                _stoppingDistance = Mathf.Max(0.0f, value);

                if (agent != null)
                    agent.stoppingDistance = _stoppingDistance;
            }
        }

        #endregion

        #region INPUT ACTIONS

        /// <summary>
        /// Mouse Cursor InputAction.
        /// </summary>

        protected InputAction mousePositionInputAction { get; set; }

        /// <summary>
        /// Mouse Click InputAction.
        /// </summary>

        protected InputAction mouseClickInputAction { get; set; }

        #endregion

        #region INPUT ACTIONS HANDLERS

        /// <summary>
        /// Get the mouse position value.
        /// Return its current value or zero if no valid InputAction found.
        /// </summary>
        
        protected virtual Vector2 GetMousePosition()
        {
            if (mousePositionInputAction != null)
                return mousePositionInputAction.ReadValue<Vector2>();

            return Vector2.zero;
        }
        
        /// <summary>
        /// Mouse click input action handler.
        /// </summary>

        protected virtual void OnMouseClick(InputAction.CallbackContext context)
        {
            _mouseButtonPressed = context.started || context.performed;
        }

        #endregion

        #region EVENTS

        public delegate void ArrivedEventHandler();

        /// <summary>
        /// Event triggered when agent reaches its destination.
        /// </summary>

        public event ArrivedEventHandler Arrived;

        /// <summary>
        /// Trigger Arrived event.
        /// Called when agent reaches its destination.
        /// </summary>

        public virtual void OnArrive()
        {
            Arrived?.Invoke();
        }

        #endregion

        #region METHODS

        /// <summary>
        /// Called after MovementMode has changed.
        /// Does special handling for starting certain modes, eg: enable / disable ground constraint, etc.
        /// If overridden, must call base.OnMovementModeChanged.
        /// </summary>

        protected override void OnMovementModeChanged(MovementMode prevMovementMode, int prevCustomMode)
        {
            // Call base implementation

            base.OnMovementModeChanged(prevMovementMode, prevCustomMode);

            // If movement mode has ben changed to None (eg: disabled movement),
            // stop path following movement

            if (IsDisabled())
                StopMovement();
        }

        /// <summary>
        /// Return the NavMeshAgent component. This is guaranteed to be not null.
        /// </summary>

        public virtual NavMeshAgent GetNavMeshAgent()
        {
            return agent;
        }

        /// <summary>
        /// Synchronize the NavMeshAgent with Character (eg: speed, acceleration, velocity, etc) as we moves the Agent.
        /// Called on OnLateFixedUpdate.
        /// </summary>

        protected virtual void SyncNavMeshAgent()
        {
            agent.angularSpeed = rotationRate;

            agent.speed = GetMaxSpeed();
            agent.acceleration = GetMaxAcceleration();
            
            agent.velocity = GetVelocity();
            agent.nextPosition = GetPosition();

            agent.radius = characterMovement.radius;
            agent.height = IsCrouching() ? crouchedHeight : unCrouchedHeight;
        }

        /// <summary>
        /// Does the Agent currently has a path?
        /// </summary>
        
        public virtual bool HasPath()
        {
            return agent.hasPath;
        }

        /// <summary>
        /// True if Agent is following a path, false otherwise.
        /// </summary>
        
        public virtual bool IsPathFollowing()
        {
            return agent.hasPath && !agent.isStopped;
        }

        /// <summary>
        /// Issue the Agent to move to desired location (in world space). 
        /// </summary>
        
        public virtual void MoveToLocation(Vector3 location)
        {
            Vector3 toLocation = (location - GetPosition()).projectedOnPlane(GetUpVector());
            if (toLocation.sqrMagnitude >= MathLib.Square(stoppingDistance))
                agent.SetDestination(location);
        }

        /// <summary>
        /// Halts Character's current path following movement.
        /// </summary>
        
        public virtual void StopMovement()
        {
            agent.ResetPath();

            SetMovementDirection(Vector3.zero);
        }

        /// <summary>
        /// Makes the character's follow Agent's path (if any).
        /// Eg: Keep updating Character's movement direction vector to steer towards Agent's destination until reached.
        /// </summary>
        
        protected virtual void PathFollowing()
        {
            // Is movement is disabled, return

            if (IsDisabled())
                return;

            // If agent has no path or not following it (eg: paused), return

            if (!IsPathFollowing())
                return;

            // Is destination reached ?

            if (agent.remainingDistance <= stoppingDistance)
            {
                // Destination is reached, stop movement

                StopMovement();

                // Trigger event

                OnArrive();
            }
            else
            {
                // If destination not reached, feed agent's desired velocity (lateral only) as the character move direction

                // Do not allow to move at a speed less than minAnalogWalkSpeed

                Vector3 planarDesiredVelocity = agent.desiredVelocity.projectedOnPlane(GetUpVector()) * brakingRatio;

                if (planarDesiredVelocity.sqrMagnitude < MathLib.Square(minAnalogWalkSpeed))
                    planarDesiredVelocity = planarDesiredVelocity.normalized * minAnalogWalkSpeed;

                SetMovementDirection(planarDesiredVelocity.normalized * ComputeAnalogInputModifier(planarDesiredVelocity));
            }
        }

        /// <summary>
        /// Extends Move method to handle PathFollowing state.
        /// </summary>

        protected override void Move()
        {
            // Handle PathFollowing state

            PathFollowing();

            // Call base implementation (e.g. Default movement modes and states).

            base.Move();
        }

        /// <summary>
        /// Setup player InputActions (if any).
        /// </summary>

        protected override void InitPlayerInput()
        {
            // Init base character controller input actions (if any)

            base.InitPlayerInput();

            // Attempts to cache and init this InputActions (if any)

            if (inputActions == null)
                return;

            mousePositionInputAction = inputActions["Mouse Position"];
            mousePositionInputAction?.Enable();
            
            mouseClickInputAction = inputActions["Mouse Click"];
            if (mouseClickInputAction != null)
            {
                mouseClickInputAction.started += OnMouseClick;
                mouseClickInputAction.performed += OnMouseClick;
                mouseClickInputAction.canceled += OnMouseClick;

                mouseClickInputAction.Enable();
            }
        }

        /// <summary>
        /// Unsubscribe from input action events and disable input actions.
        /// </summary>

        protected override void DeinitPlayerInput()
        {
            base.DeinitPlayerInput();

            if (mousePositionInputAction != null)
            {
                mousePositionInputAction.Disable();
                mousePositionInputAction = null;
            }
            
            if (mouseClickInputAction != null)
            {
                mouseClickInputAction.started -= OnMouseClick;
                mouseClickInputAction.performed -= OnMouseClick;
                mouseClickInputAction.canceled -= OnMouseClick;

                mouseClickInputAction.Disable();
                mouseClickInputAction = null;
            }
        }

        /// <summary>
        /// Overrides HandleInput method, to perform custom input code, in this case, click-to-move.
        /// </summary>

        protected override void HandleInput()
        {
            // Should handle input here ?

            if (inputActions == null)
                return;

            // Movement (click-to-move)

            if (_mouseButtonPressed)
            {
                Vector2 mousePosition = GetMousePosition();

                Ray ray = camera.ScreenPointToRay(mousePosition);

                LayerMask groundMask = characterMovement.collisionLayers;

                QueryTriggerInteraction triggerInteraction = characterMovement.triggerInteraction;

                if (Physics.Raycast(ray, out RaycastHit hitResult, Mathf.Infinity, groundMask, triggerInteraction))
                    MoveToLocation(hitResult.point);
            }
            else if (!IsPathFollowing())
            {
                // Default movement input. Allow to control the agent with keyboard or controller too

                base.HandleInput();
            }
        }
        
        /// <summary>
        /// Our Reset method. Set this default values.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnReset()
        {
            // Base class defaults

            base.OnReset();

            // This defaults

            autoBraking = true;

            brakingDistance = 2.0f;
            stoppingDistance = 1.0f;
        }

        /// <summary>
        /// Our OnValidate method.
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnOnValidate()
        {
            // Validate base class

            base.OnOnValidate();

            // Validate this editor exposed fields

            brakingDistance = _brakingDistance;
            stoppingDistance = _stoppingDistance;
        }

        /// <summary>
        /// Called when the script instance is being loaded (Awake).
        /// If overridden, must call base method in order to fully initialize the class.
        /// </summary>

        protected override void OnAwake()
        {
            // Init base class

            base.OnAwake();

            // Initialize NavMeshAgent

            agent.autoBraking = autoBraking;
            agent.stoppingDistance = stoppingDistance;

            // Turn-off NavMeshAgent control, we control it, not the other way
            
            agent.updatePosition = false;
            agent.updateRotation = false;

            agent.updateUpAxis = false;
        }

        /// <summary>
        /// Our LateUpdate. Calls SyncNavMeshAgent.
        /// </summary>
        
        protected override void OnLateUpdate()
        {
            // Move Agent with us

            SyncNavMeshAgent();
        }

        #endregion
    }
}