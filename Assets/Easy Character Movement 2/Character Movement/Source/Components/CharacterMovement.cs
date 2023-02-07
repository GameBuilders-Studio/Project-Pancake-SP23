using System;
using System.Collections.Generic;
using UnityEngine;

namespace EasyCharacterMovement
{
    #region ENUMS

    /// <summary>
    /// The axis that constraints movement.
    /// </summary>

    public enum PlaneConstraint
    {
        None,
        ConstrainXAxis,
        ConstrainYAxis,
        ConstrainZAxis,
        Custom
    }

    /// <summary>
    /// The hit location WRT Character's capsule, eg: Sides, Above, Below.
    /// </summary>
    
    public enum HitLocation
    {
        None = 0,
        Sides = 1,
        Above = 2,
        Below = 4,
    }

    /// <summary>
    /// The character collision behavior.
    /// </summary>

    public enum CollisionBehavior
    {
        Default = 0,
        
        /// <summary>
        /// Determines if the character can walk on the other collider.
        /// </summary>

        Walkable = 1 << 0,
        NotWalkable = 1 << 1,

        /// <summary>
        /// Determines if the character can perch on the other collider.
        /// </summary>

        CanPerchOn = 1 << 2,
        CanNotPerchOn = 1 << 3,

        /// <summary>
        /// Defines if the character can step up onto the other collider.
        /// </summary>

        CanStepOn = 1 << 4,
        CanNotStepOn = 1 << 5,

        /// <summary>
        /// Defines if the character can effectively travel with the object it is standing on.
        /// </summary>

        CanRideOn = 1 << 6,
        CanNotRideOn = 1 << 7
    }

    #endregion

    #region STRUCTS

    /// <summary>
    /// RaycastHitComparer used to sort a RayCastHit array by distance (ASC order).
    /// </summary>

    public class RaycastHitComparer : IComparer<RaycastHit>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            if (x.distance < y.distance)
                return -1;
            else if (x.distance > y.distance)
                return 1;

            return 0;
        }
    }

    /// <summary>
    /// Holds information about found ground (if any).
    /// </summary>

    public struct FindGroundResult
    {
        /// <summary>
        /// Did we hit ground ? Eg. impacted capsule's bottom sphere.
        /// </summary>

        public bool hitGround;

        /// <summary>
        /// Is the found ground walkable ?
        /// </summary>

        public bool isWalkable;

        /// <summary>
        /// Is walkable ground ? (eg: hitGround == true && isWalkable == true).
        /// </summary>

        public bool isWalkableGround => hitGround && isWalkable;

        /// <summary>
        /// The Character's position, in case of a raycast result this equals to point.
        /// </summary>

        public Vector3 position;

        /// <summary>
        /// The impact point in world space.
        /// </summary>

        public Vector3 point => hitResult.point;

        /// <summary>
        /// The normal of the hit surface.
        /// </summary>

        public Vector3 normal => hitResult.normal;

        /// <summary>
        /// Normal of the hit in world space, for the object that was hit by the sweep, if any.
        /// For example if a capsule hits a flat plane, this is a normalized vector pointing out from the plane.
        /// In the case of impact with a corner or edge of a surface, usually the "most opposing" normal (opposed to the query direction) is chosen.
        /// </summary>

        public Vector3 surfaceNormal;

        /// <summary>
        /// The collider of the hit object.
        /// </summary>

        public Collider collider;

        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to a rigidbody then it is null.
        /// </summary>

        public Rigidbody rigidbody => collider ? collider.attachedRigidbody : null;

        /// <summary>
        /// The Transform of the rigidbody or collider that was hit.
        /// </summary>

        public Transform transform
        {
            get
            {
                if (collider == null)
                    return null;

                Rigidbody attachedRigidbody = collider.attachedRigidbody;
                return attachedRigidbody ? attachedRigidbody.transform : collider.transform;
            }
        }
        
        /// <summary>
        /// The distance to the ground, computed from the swept capsule.
        /// </summary>

        public float groundDistance;

        /// <summary>
        /// True if the hit found a valid walkable ground using a raycast (rather than a sweep test, which happens when the sweep test fails to yield a walkable surface).
        /// </summary>

        public bool isRaycastResult;

        /// <summary>
        /// The distance to the ground, computed from a raycast. Only valid if isRaycast is true.
        /// </summary>

        public float raycastDistance;
        
        /// <summary>
        /// Hit result of the test that found ground.
        /// </summary>

        public RaycastHit hitResult;

        /// <summary>
        /// Gets the distance to ground, either raycastDistance or distance.
        /// </summary>

        public float GetDistanceToGround()
        {
            return isRaycastResult ? raycastDistance : groundDistance;
        }

        /// <summary>
        /// Initialize this with a sweep test result.
        /// </summary>

        public void SetFromSweepResult(bool hitGround, bool isWalkable, Vector3 position, float sweepDistance,
            ref RaycastHit inHit, Vector3 surfaceNormal)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;
            
            this.position = position;

            collider = inHit.collider;

            groundDistance = sweepDistance;

            isRaycastResult = false;
            raycastDistance = 0.0f;

            hitResult = inHit;

            this.surfaceNormal = surfaceNormal;
        }

        public void SetFromSweepResult(bool hitGround, bool isWalkable, Vector3 position, Vector3 point, Vector3 normal,
            Vector3 surfaceNormal, Collider collider, float sweepDistance)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;
            
            this.position = position;

            this.collider = collider;

            groundDistance = sweepDistance;

            isRaycastResult = false;
            raycastDistance = 0.0f;

            hitResult = new RaycastHit
            {
                point = point,
                normal = normal,

                distance = sweepDistance
            };

            this.surfaceNormal = surfaceNormal;
        }

        /// <summary>
        /// Initialize this with a raycast result.
        /// </summary>

        public void SetFromRaycastResult(bool hitGround, bool isWalkable, Vector3 position, float sweepDistance,
            float castDistance, ref RaycastHit inHit)
        {
            this.hitGround = hitGround;
            this.isWalkable = isWalkable;

            this.position = position;

            collider = inHit.collider;

            groundDistance = sweepDistance;

            isRaycastResult = true;
            raycastDistance = castDistance;

            float oldDistance = hitResult.distance;

            hitResult = inHit;
            hitResult.distance = oldDistance;

            surfaceNormal = hitResult.normal;
        }
    }
    
    /// <summary>
    /// Describes a collision of this Character.
    /// </summary>

    public struct CollisionResult
    {
        /// <summary>
        /// True if character is overlapping.
        /// </summary>

        public bool startPenetrating;

        /// <summary>
        /// The hit location WRT Character's capsule, eg: Below, Sides, Top.
        /// </summary>

        public HitLocation hitLocation;

        /// <summary>
        /// Is the hit walkable ground ?
        /// </summary>

        public bool isWalkable;

        /// <summary>
        /// The character position at this collision.
        /// </summary>

        public Vector3 position;

        /// <summary>
        /// The character's velocity at this collision.
        /// </summary>

        public Vector3 velocity;

        /// <summary>
        /// The collided object's velocity.
        /// </summary>

        public Vector3 otherVelocity;

        /// <summary>
        /// The impact point in world space.
        /// </summary>

        public Vector3 point;

        /// <summary>
        /// The impact normal in world space.
        /// </summary>

        public Vector3 normal;

        /// <summary>
        /// Normal of the hit in world space, for the object that was hit by the sweep, if any.
        /// For example if a capsule hits a flat plane, this is a normalized vector pointing out from the plane.
        /// In the case of impact with a corner or edge of a surface, usually the "most opposing" normal (opposed to the query direction) is chosen.
        /// </summary>

        public Vector3 surfaceNormal;

        /// <summary>
        /// The character's displacement up to this hit.
        /// </summary>

        public Vector3 displacementToHit;

        /// <summary>
        /// Remaining displacement after hit.
        /// </summary>

        public Vector3 remainingDisplacement;

        /// <summary>
        /// The collider of the hit object.
        /// </summary>

        public Collider collider;
        
        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to a rigidbody then it is null.
        /// </summary>

        public Rigidbody rigidbody => collider ? collider.attachedRigidbody : null;

        /// <summary>
        /// The Transform of the rigidbody or collider that was hit.
        /// </summary>

        public Transform transform
        {
            get
            {
                if (collider == null)
                    return null;

                Rigidbody rb = collider.attachedRigidbody;
                return rb ? rb.transform : collider.transform;
            }
        }

        /// <summary>
        /// Structure containing information about this hit.
        /// </summary>

        public RaycastHit hitResult;
    }

    #endregion

    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public sealed class CharacterMovement : MonoBehaviour
    {
        #region ENUMS

        /// <summary>
        /// The depenetration behavior.
        /// </summary>

        private enum DepenetrationBehavior
        {
            IgnoreNone = 0,

            IgnoreStatic = 1 << 0,
            IgnoreDynamic = 1 << 1,
            IgnoreKinematic = 1 << 2
        }

        #endregion

        #region STRUCTS

        /// <summary>
        /// Structure containing advanced settings.
        /// </summary>

        [Serializable]
        public struct Advanced
        {
            [Tooltip("The minimum move distance of the character controller." +
                     " If the character tries to move less than this distance, it will not move at all. This can be used to reduce jitter. In most situations this value should be left at 0.")]
            public float minMoveDistance;
            public float minMoveDistanceSqr => minMoveDistance * minMoveDistance;
            
            [Tooltip("Max number of iterations used during movement.")]
            public int maxMovementIterations;

            [Tooltip("Max number of iterations used to resolve penetrations.")]
            public int maxDepenetrationIterations;

            [Space(15f)]
            [Tooltip("If enabled, the character will interact with dynamic rigidbodies when walking into them.")]
            public bool enablePhysicsInteraction;

            [Tooltip("If enabled, the character will interact with other characters when walking into them.")]
            public bool allowPushCharacters;

            [Tooltip("If enabled, the character will move with the moving platform it is standing on.")]
            public bool impartPlatformMovement;

            [Tooltip("If enabled, the character will rotate (yaw-only) with the moving platform it is standing on.")]
            public bool impartPlatformRotation;

            [Tooltip("If enabled, impart the platform's velocity when jumping or falling off it.")]
            public bool impartPlatformVelocity;

            public void Reset()
            {
                minMoveDistance = 0.0f;

                maxMovementIterations = 5;
                maxDepenetrationIterations = 2;

                enablePhysicsInteraction = false;
                allowPushCharacters = false;
                impartPlatformMovement = true;
                impartPlatformRotation = true;
                impartPlatformVelocity = true;
            }

            public void OnValidate()
            {
                minMoveDistance = Mathf.Max(minMoveDistance, 0.0f);

                maxMovementIterations = Mathf.Max(maxMovementIterations, 1);
                maxDepenetrationIterations = Mathf.Max(maxDepenetrationIterations, 1);
            }
        }

        /// <summary>
        /// Structure containing information about platform.
        /// </summary>

        public struct MovingPlatform
        {
            /// <summary>
            /// The last frame active platform.
            /// </summary>

            public Rigidbody lastPlatform;

            /// <summary>
            /// The current active platform.
            /// </summary>

            public Rigidbody platform;

            /// <summary>
            /// The character's last position on active platform.
            /// </summary>

            public Vector3 position;

            /// <summary>
            /// The character's last position on active platform in platform's local space.
            /// </summary>

            public Vector3 localPosition;

            /// <summary>
            /// The character's last rotation on active platform.
            /// </summary>

            public Quaternion rotation;

            /// <summary>
            /// The character's last rotation on active platform in platform's local space.
            /// </summary>

            public Quaternion localRotation;

            /// <summary>
            /// The current active platform velocity.
            /// </summary>

            public Vector3 platformVelocity;
        }

        #endregion

        #region CONSTANTS

        private const float kKindaSmallNumber = 0.0001f;
        private const float kHemisphereLimit = 0.01f;

        private const int kMaxCollisionCount = 8;
        private const int kMaxOverlapCount = 8;

        private const float kSweepEdgeRejectDistance = 0.0015f;

        private const float kMinGroundDistance = 0.019f;
        private const float kMaxGroundDistance = 0.024f;
        private const float kAvgGroundDistance = (kMinGroundDistance + kMaxGroundDistance) * 0.5f;

        private const float kMinWalkableSlopeLimit = 1.000000f;
        private const float kMaxWalkableSlopeLimit = 0.017452f;

        private const float kPenetrationOffset = 0.00125f;

        private const float kContactOffset = 0.01f;
        private const float kSmallContactOffset = 0.001f;

        #endregion

        #region EDITOR EXPOSED FIELDS

        [Space(15f)]
        [Tooltip("Allow to constrain the Character so movement along the locked axis is not possible.")]
        [SerializeField]
        private PlaneConstraint _planeConstraint;

        [Space(15f)]
        [SerializeField, Tooltip("The root transform in the avatar.")]
        private Transform _rootTransform;

        [SerializeField, Tooltip("The root transform will be positioned at this offset from foot position.")]
        private Vector3 _rootTransformOffset = new Vector3(0, 0, 0);

        [Space(15f)]
        [Tooltip("The Character's capsule collider radius.")]
        [SerializeField]
        private float _radius;

        [Tooltip("The Character's capsule collider height")]
        [SerializeField]
        private float _height;
        
        [Space(15f)]
        [Tooltip("The maximum angle (in degrees) for a walkable surface.")]
        [SerializeField]
        private float _slopeLimit;

        [Tooltip("The maximum height (in meters) for a valid step (up to Character's height).")]
        [SerializeField]
        private float _stepOffset;

        [Tooltip("Allow a Character to perch on the edge of a surface if the horizontal distance from the Character's position to the edge is closer than this.\n" +
                 "Note that characters will not fall off if they are within stepOffset of a walkable surface below.")]
        [SerializeField]
        private float _perchOffset;

        [Tooltip("When perching on a ledge, add this additional distance to stepOffset when determining how high above a walkable ground we can perch.\n" +
                 "Note that we still enforce stepOffset to start the step up, this just allows the Character to hang off the edge or step slightly higher off the ground.")]
        [SerializeField]
        private float _perchAdditionalHeight;

        [Space(15f)]
        [Tooltip("If enabled, colliders with SlopeLimitBehavior component will be able to override this slope limit.")]
        [SerializeField]
        private bool _slopeLimitOverride;

        [Tooltip("When enabled, will treat head collisions as if the character is using a shape with a flat top.")]
        [SerializeField]
        private bool _useFlatTop;

        [Tooltip("Performs ground checks as if the character is using a shape with a flat base." +
                 "This avoids the situation where characters slowly lower off the side of a ledge (as their capsule 'balances' on the edge).")]
        [SerializeField]
        private bool _useFlatBaseForGroundChecks;
        
        [Space(15f)]
        [Tooltip("Character collision layers mask.")]
        [SerializeField]
        private LayerMask _collisionLayers = 1;
        
        [Tooltip("Overrides the global Physics.queriesHitTriggers to specify whether queries (raycasts, spherecasts, overlap tests, etc.) hit Triggers by default." +
                 " Use Ignore for queries to ignore trigger Colliders.")]
        [SerializeField]
        private QueryTriggerInteraction _triggerInteraction = QueryTriggerInteraction.Ignore;

        [Space(15f)]
        [SerializeField]
        private Advanced _advanced;

        #endregion

        #region FIELDS

        private Transform _transform;

        private Rigidbody _rigidbody;

        private CapsuleCollider _capsuleCollider;

        private Vector3 _capsuleCenter;
        private Vector3 _capsuleTopCenter;
        private Vector3 _capsuleBottomCenter;

        private readonly HashSet<Rigidbody> _ignoredRigidbodies = new HashSet<Rigidbody>();
        private readonly HashSet<Collider> _ignoredColliders = new HashSet<Collider>();

        private readonly RaycastHitComparer _hitComparer = new RaycastHitComparer();

        private readonly RaycastHit[] _hits = new RaycastHit[kMaxCollisionCount];
        private readonly Collider[] _overlaps = new Collider[kMaxOverlapCount];

        private int _collisionCount;
        private readonly CollisionResult[] _collisionResults = new CollisionResult[kMaxCollisionCount];

        [SerializeField, HideInInspector]
        private float _minSlopeLimit;

        private bool _detectCollisions = true;

        private bool _isConstrainedToGround = true;
        private float _unconstrainedTimer;

        private Vector3 _constraintPlaneNormal;

        private Vector3 _characterUp;

        private Vector3 _transformedCapsuleCenter;
        private Vector3 _transformedCapsuleTopCenter;
        private Vector3 _transformedCapsuleBottomCenter;

        private Vector3 _velocity;
        
        private Vector3 _pendingForces;
        private Vector3 _pendingImpulses;
        private Vector3 _pendingLaunchVelocity;

        private float _pushForceScale = 1.0f;

        private bool _hasLanded;

        private FindGroundResult _foundGround;
        private FindGroundResult _currentGround;

        private Rigidbody _parentPlatform;
        private MovingPlatform _movingPlatform;

        #endregion

        #region PROPERTIES
        
        /// <summary>
        /// Cached character's transform.
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
        /// The Character's rigidbody.
        /// </summary>

        public new Rigidbody rigidbody
        {
            get
            {
#if UNITY_EDITOR
                if (_rigidbody == null)
                    _rigidbody = GetComponent<Rigidbody>();
#endif

                return _rigidbody;
            }
        }

        /// <summary>
        /// The Rigidbody interpolation setting.
        /// </summary>

        public RigidbodyInterpolation interpolation
        {
            get => rigidbody.interpolation;
            set => rigidbody.interpolation = value;
        }

        /// <summary>
        /// The Character's collider.
        /// </summary>

        public new Collider collider
        {
            get
            {
#if UNITY_EDITOR
                if (_capsuleCollider == null)
                    _capsuleCollider = GetComponent<CapsuleCollider>();
#endif

                return _capsuleCollider;
            }
        }

        /// <summary>
        /// The root bone in the avatar.
        /// </summary>

        public Transform rootTransform
        {
            get => _rootTransform;
            set => _rootTransform = value;
        }

        /// <summary>
        /// The root transform will be positioned at this offset.
        /// </summary>

        public Vector3 rootTransformOffset
        {
            get => _rootTransformOffset;
            set => _rootTransformOffset = value;
        }

        /// <summary>
        /// The character's current position.
        /// </summary>

        public Vector3 position
        {
            get => GetPosition();
            set => SetPosition(value);
        }

        /// <summary>
        /// The character's current rotation.
        /// </summary>

        public Quaternion rotation
        {
            get => GetRotation();
            set => SetRotation(value);
        }

        /// <summary>
        /// The character's center in world space.
        /// </summary>

        public Vector3 worldCenter => position + rotation * _capsuleCenter;

        /// <summary>
        /// The character's updated position.
        /// </summary>

        public Vector3 updatedPosition { get; private set; }


        /// <summary>
        /// The character's updated rotation.
        /// </summary>

        public Quaternion updatedRotation { get; private set; }

        /// <summary>
        /// The current relative velocity of the Character.
        /// The velocity is relative because it won't track movements to the transform that happen outside of this,
        /// e.g. character parented under another moving Transform, such as a moving vehicle.
        /// </summary>

        public Vector3 velocity
        {
            get => _velocity;
            set => _velocity = value;
        }

        /// <summary>
        /// The character's speed.
        /// </summary>

        public float speed => _velocity.magnitude;

        /// <summary>
        /// The character's speed along its forward vector (e.g: in local space).
        /// </summary>

        public float forwardSpeed => _velocity.dot(transform.forward);

        /// <summary>
        /// The character's speed along its right vector (e.g: in local space).
        /// </summary>

        public float sidewaysSpeed => _velocity.dot(transform.right);

        /// <summary>
        /// The Character's capsule collider radius.
        /// </summary>

        public float radius
        {
            get => _radius;
            set => SetDimensions(value, _height);
        }

        /// <summary>
        /// The Character's capsule collider height.
        /// </summary>

        public float height
        {
            get => _height;
            set => SetDimensions(_radius, value);
        }
        
        /// <summary>
        /// The maximum angle (in degrees) for a walkable slope.
        /// </summary>

        public float slopeLimit
        {
            get => _slopeLimit;

            set
            {
                _slopeLimit = Mathf.Clamp(value, 0.0f, 89.0f);

                // Add 0.01f to avoid numerical precision errors

                _minSlopeLimit = Mathf.Cos((_slopeLimit + 0.01f) * Mathf.Deg2Rad);
            }
        }

        /// <summary>
        /// The maximum height (in meters) for a valid step.
        /// </summary>

        public float stepOffset
        {
            get => _stepOffset;
            set => _stepOffset = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Allow a Character to perch on the edge of a surface if the horizontal distance from the Character's position to the edge is closer than this.
        /// Note that we still enforce stepOffset to start the step up, this just allows the Character to hang off the edge or step slightly higher off the ground.
        /// </summary>

        public float perchOffset
        {
            get => _perchOffset;
            set => _perchOffset = Mathf.Clamp(value, 0.0f, _radius);
        }

        /// <summary>
        /// When perching on a ledge, add this additional distance to stepOffset when determining how high above a walkable ground we can perch.
        /// </summary>

        public float perchAdditionalHeight
        {
            get => _perchAdditionalHeight;
            set => _perchAdditionalHeight = Mathf.Max(0.0f, value);
        }

        /// <summary>
        /// Should allow external slope limit override ?
        /// </summary>

        public bool slopeLimitOverride
        {
            get => _slopeLimitOverride;
            set => _slopeLimitOverride = value;
        }

        /// <summary>
        /// When enabled, will treat head collisions as if the character is using a shape with a flat top.
        /// </summary>

        public bool useFlatTop
        {
            get => _useFlatTop;
            set => _useFlatTop = value;
        }

        /// <summary>
        /// Performs ground checks as if the character is using a shape with a flat base.
        /// This avoids the situation where characters slowly lower off the side of a ledge (as their capsule 'balances' on the edge).
        /// </summary>

        public bool useFlatBaseForGroundChecks
        {
            get => _useFlatBaseForGroundChecks;
            set => _useFlatBaseForGroundChecks = value;
        }

        /// <summary>
        /// Layers to be considered during collision detection.
        /// </summary>

        public LayerMask collisionLayers
        {
            get => _collisionLayers;
            set => _collisionLayers = value;
        }

        /// <summary>
        /// Determines how the Character should interact with triggers.
        /// </summary>

        public QueryTriggerInteraction triggerInteraction
        {
            get => _triggerInteraction;
            set => _triggerInteraction = value;
        }

        /// <summary>
        /// Should perform collision detection ?
        /// </summary>

        public bool detectCollisions
        {
            get => _detectCollisions;
            set
            {
                _detectCollisions = value;

                if (_capsuleCollider)
                    _capsuleCollider.enabled = _detectCollisions;
            }
        } 

        /// <summary>
        /// What part of the capsule collided with the environment during the last Move call.
        /// </summary>

        public CollisionFlags collisionFlags { get; private set; }

        /// <summary>
        /// Is the Character's movement constrained to a plane ?
        /// </summary>

        public bool isConstrainedToPlane => _planeConstraint != PlaneConstraint.None;

        /// <summary>
        /// Should movement be constrained to ground when on walkable ground ?
        /// Toggles ground constraint. 
        /// </summary>

        public bool constrainToGround
        {
            get => isConstrainedToGround;
            set => _isConstrainedToGround = value;
        }

        /// <summary>
        /// Is the Character constrained to walkable ground ?
        /// </summary>

        public bool isConstrainedToGround => _isConstrainedToGround && _unconstrainedTimer == 0.0f;

        /// <summary>
        /// Was the character on ground last Move call ?
        /// </summary>

        public bool wasOnGround { get; private set; }

        /// <summary>
        /// Is the character on ground ?
        /// </summary>

        public bool isOnGround => _currentGround.hitGround;

        /// <summary>
        /// Was the character on walkable ground last Move call ?
        /// </summary>

        public bool wasOnWalkableGround { get; private set; }

        /// <summary>
        /// Is the character on walkable ground ?
        /// </summary>

        public bool isOnWalkableGround => _currentGround.isWalkableGround;

        /// <summary>
        /// Was the character on walkable ground AND constrained to ground last Move call ?
        /// </summary>

        public bool wasGrounded { get; private set; }

        /// <summary>
        /// Is the character on walkable ground AND constrained to ground.
        /// </summary>

        public bool isGrounded => isOnWalkableGround && isConstrainedToGround;

        /// <summary>
        /// The signed distance to ground.
        /// </summary>

        public float groundDistance => _currentGround.groundDistance;

        /// <summary>
        /// The current ground impact point.
        /// </summary>

        public Vector3 groundPoint => _currentGround.point;

        /// <summary>
        /// The current ground normal.
        /// </summary>

        public Vector3 groundNormal => _currentGround.normal;

        /// <summary>
        /// The current ground surface normal.
        /// </summary>

        public Vector3 groundSurfaceNormal => _currentGround.surfaceNormal;

        /// <summary>
        /// The current ground collider.
        /// </summary>

        public Collider groundCollider => _currentGround.collider;

        /// <summary>
        /// The current ground transform.
        /// </summary>

        public Transform groundTransform => _currentGround.transform;

        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to a rigidbody then it is null.
        /// </summary>

        public Rigidbody groundRigidbody => _currentGround.rigidbody;

        /// <summary>
        /// Structure containing information about current ground.
        /// </summary>

        public FindGroundResult currentGround => _currentGround;

        /// <summary>
        /// Structure containing information about current moving platform (if any).
        /// </summary>

        public MovingPlatform movingPlatform => _movingPlatform;

        /// <summary>
        /// The terminal velocity when landed (eg: isGrounded).
        /// </summary>

        public Vector3 landedVelocity { get; private set; }

        /// <summary>
        /// Set this to true if riding on a moving platform that you know is clear from non-moving world obstructions.
        /// Optimization to avoid sweeps during based movement, USE WITH CARE.
        /// </summary>

        public bool fastPlatformMove { get; set; }

        /// <summary>
        /// Whether the Character moves with the moving platform it is standing on.
        /// If true, the Character moves with the moving platform.
        /// </summary>

        public bool impartPlatformMovement
        {
            get => _advanced.impartPlatformMovement;
            set => _advanced.impartPlatformMovement = value;
        }

        /// <summary>
        /// Whether the Character receives the changes in rotation of the platform it is standing on.
        /// If true, the Character rotates with the moving platform.
        /// </summary>

        public bool impartPlatformRotation
        {
            get => _advanced.impartPlatformRotation;
            set => _advanced.impartPlatformRotation = value;
        }

        /// <summary>
        /// If true, impart the platform's velocity when jumping or falling off it.
        /// </summary>

        public bool impartPlatformVelocity
        {
            get => _advanced.impartPlatformVelocity;
            set => _advanced.impartPlatformVelocity = value;
        }

        /// <summary>
        /// If enabled, the player will interact with dynamic rigidbodies when walking into them.
        /// </summary>

        public bool enablePhysicsInteraction
        {
            get => _advanced.enablePhysicsInteraction;
            set => _advanced.enablePhysicsInteraction = value;
        }

        /// <summary>
        /// If enabled, the player will interact with other characters when walking into them.
        /// </summary>

        public bool physicsInteractionAffectsCharacters
        {
            get => _advanced.allowPushCharacters;
            set => _advanced.allowPushCharacters = value;
        }

        /// <summary>
        /// Force applied to rigidbodies when walking into them (due to mass and relative velocity) is scaled by this amount.
        /// </summary>

        public float pushForceScale
        {
            get => _pushForceScale;
            set => _pushForceScale = Mathf.Max(0.0f, value);
        }

        #endregion

        #region CALLBACKS

        /// <summary>
        /// Let you define if the character should collide with given collider.
        /// </summary>
        /// <param name="collider">The collider.</param>
        /// <returns>True to filter (ignore) given collider, false to collide with given collider.</returns>

        public delegate bool ColliderFilterCallback(Collider collider);

        /// <summary>
        /// Let you define the character behavior when collides with collider.
        /// </summary>
        /// <param name="collider">The collided collider</param>
        /// <returns>The desired collision behavior flags.</returns>

        public delegate CollisionBehavior CollisionBehaviorCallback(Collider collider);

        /// <summary>
        /// Let you modify the collision response vs dynamic objects,
        /// eg: compute resultant impulse and / or application point (CollisionResult.point).
        /// </summary>

        public delegate void CollisionResponseCallback(ref CollisionResult inCollisionResult, ref Vector3 characterImpulse, ref Vector3 otherImpulse);

        /// <summary>
        /// Let you define if the character should collide with given collider.
        /// Return true to filter (ignore) collider, false otherwise.
        /// </summary>

        public ColliderFilterCallback colliderFilterCallback { get; set; }

        /// <summary>
        /// Let you define the character behavior when collides with collider.
        /// </summary>

        public CollisionBehaviorCallback collisionBehaviorCallback { get; set; }

        /// <summary>
        /// Let you modify the collision response vs dynamic objects,
        /// eg: compute resultant impulse and / or application point (CollisionResult.point).
        /// </summary>

        public CollisionResponseCallback collisionResponseCallback { get; set; }

        #endregion

        #region EVENTS

        public delegate void CollidedEventHandler(ref CollisionResult collisionResult);
        public delegate void FoundGroundEventHandler(ref FindGroundResult foundGround);

        /// <summary>
        /// Event triggered when characters collides with other during a Move.
        /// Can be called multiple times.
        /// </summary>

        public event CollidedEventHandler Collided;

        /// <summary>
        /// Event triggered when a character finds ground (walkable or non-walkable) as a result of a downcast sweep (eg: FindGround method).
        /// </summary>

        public event FoundGroundEventHandler FoundGround;

        /// <summary>
        /// Trigger Collided events.
        /// </summary>

        private void OnCollided()
        {
            if (Collided == null)
                return;

            for (int i = 0; i < _collisionCount; i++)
                Collided.Invoke(ref _collisionResults[i]);
        }

        /// <summary>
        /// Trigger FoundGround event.
        /// </summary>

        private void OnFoundGround()
        {
            FoundGround?.Invoke(ref _currentGround);
        }

        #endregion

        #region GEOM_NOMRAL_METHODS

        private Vector3 FindOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            const float kThickness = (kContactOffset - kSweepEdgeRejectDistance) * 0.5f;

            Vector3 result = inHit.normal;
            
            Vector3 rayOrigin = inHit.point - sweepDirDenorm;
            
            float rayLength = sweepDirDenorm.magnitude * 2f;
            Vector3 rayDirection = sweepDirDenorm / sweepDirDenorm.magnitude;

            if (Raycast(rayOrigin, rayDirection, rayLength, _collisionLayers, out RaycastHit hitResult, kThickness))
                result = hitResult.normal;

            return result;
        }

        private static Vector3 FindBoxOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            Transform localToWorld = inHit.transform;

            Vector3 localContactNormal = localToWorld.InverseTransformDirection(inHit.normal);
            Vector3 localTraceDirDenorm = localToWorld.InverseTransformDirection(sweepDirDenorm);

            Vector3 bestLocalNormal = localContactNormal;
            float bestOpposingDot = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                if (localContactNormal[i] > kKindaSmallNumber)
                {
                    float traceDotFaceNormal = localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = 1.0f;
                    }
                }
                else if (localContactNormal[i] < -kKindaSmallNumber)
                {
                    float traceDotFaceNormal = -localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = -1.0f;
                    }
                }
            }

            return localToWorld.TransformDirection(bestLocalNormal);
        }

        private static Vector3 FindBoxOpposingNormal(Vector3 displacement, Vector3 hitNormal, Transform hitTransform)
        {
            Transform localToWorld = hitTransform;

            Vector3 localContactNormal = localToWorld.InverseTransformDirection(hitNormal);
            Vector3 localTraceDirDenorm = localToWorld.InverseTransformDirection(displacement);

            Vector3 bestLocalNormal = localContactNormal;
            float bestOpposingDot = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                if (localContactNormal[i] > kKindaSmallNumber)
                {
                    float traceDotFaceNormal = localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = 1.0f;
                    }
                }
                else if (localContactNormal[i] < -kKindaSmallNumber)
                {
                    float traceDotFaceNormal = -localTraceDirDenorm[i];
                    if (traceDotFaceNormal < bestOpposingDot)
                    {
                        bestOpposingDot = traceDotFaceNormal;
                        bestLocalNormal = Vector3.zero;
                        bestLocalNormal[i] = -1.0f;
                    }
                }
            }

            return localToWorld.TransformDirection(bestLocalNormal);
        }

        private static Vector3 FindTerrainOpposingNormal(ref RaycastHit inHit)
        {
            TerrainCollider terrainCollider = inHit.collider as TerrainCollider;

            if (terrainCollider)
            {
                Vector3 localPoint = terrainCollider.transform.InverseTransformPoint(inHit.point);

                TerrainData terrainData = terrainCollider.terrainData;

                Vector3 interpolatedNormal = terrainData.GetInterpolatedNormal(localPoint.x / terrainData.size.x,
                    localPoint.z / terrainData.size.z);

                return interpolatedNormal;
            }

            return inHit.normal;
        }

        /// <summary>
        /// Helper method to retrieve real surface normal, usually the most 'opposing' to sweep direction.
        /// </summary>
        
        private Vector3 FindGeomOpposingNormal(Vector3 sweepDirDenorm, ref RaycastHit inHit)
        {
            // SphereCollider or CapsuleCollider

            if (inHit.collider is SphereCollider _ || inHit.collider is CapsuleCollider _)
            {
                // We don't compute anything special, inHit.normal is the correct one.

                return inHit.normal;
            }

            // BoxCollider

            if (inHit.collider is BoxCollider _)
            {
                return FindBoxOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Non-Convex MeshCollider (MUST BE read / write enabled!)

            if (inHit.collider is MeshCollider nonConvexMeshCollider && !nonConvexMeshCollider.convex)
            {
                Mesh sharedMesh = nonConvexMeshCollider.sharedMesh;
                if (sharedMesh && sharedMesh.isReadable)
                    return MeshUtility.FindMeshOpposingNormal(sharedMesh, ref inHit);

                // No read / write enabled, fallback to a raycast...

                return FindOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Convex MeshCollider

            if (inHit.collider is MeshCollider convexMeshCollider && convexMeshCollider.convex)
            {
                // No data exposed by Unity to compute normal. Fallback to a raycast...

                return FindOpposingNormal(sweepDirDenorm, ref inHit);
            }

            // Terrain collider
            
            if (inHit.collider is TerrainCollider)
            {
                return FindTerrainOpposingNormal(ref inHit);
            }
            
            return inHit.normal;
        }

        #endregion

        #region METHODS

        public static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        public static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        /// <summary>
        /// Apply friction and braking deceleration to given velocity.
        /// </summary>
        /// <param name="currentVelocity">Character's current velocity.</param>
        /// <param name="friction">Friction (drag) coefficient applied when braking.</param>
        /// <param name="deceleration">The rate at which the character slows down. This is a constant opposing force that directly lowers velocity by a constant value.</param>
        /// <param name="deltaTime">Simulation deltaTime</param>
        /// <returns>Returns the updated velocity</returns>
        
        private static Vector3 ApplyVelocityBraking(Vector3 currentVelocity, float friction, float deceleration, float deltaTime)
        {
            // If no friction or no deceleration, return

            bool isZeroFriction = friction == 0.0f;
            bool isZeroBraking = deceleration == 0.0f;

            if (isZeroFriction && isZeroBraking)
                return currentVelocity;
            
            // Decelerate to brake to a stop

            Vector3 oldVel = currentVelocity;
            Vector3 revAcceleration = isZeroBraking ? Vector3.zero : -deceleration * currentVelocity.normalized;

            // Apply friction and braking

            currentVelocity += (-friction * currentVelocity + revAcceleration) * deltaTime;

            // Don't reverse direction

            if (Vector3.Dot(currentVelocity, oldVel) <= 0.0f)
                return Vector3.zero;

            // Clamp to zero if nearly zero, or if below min threshold and braking

            float sqrSpeed = currentVelocity.sqrMagnitude;
            if (sqrSpeed <= 0.00001f || !isZeroBraking && sqrSpeed <= 0.01f)
                return Vector3.zero;

            return currentVelocity;
        }

        /// <summary>
        /// Determines how far is the desiredVelocity from maximum speed.
        /// </summary>
        /// <param name="desiredVelocity">The target velocity.</param>
        /// <param name="maxSpeed">The maximum allowed speed.</param>
        /// <returns>Returns the analog input modifier in the 0 - 1 range.</returns>

        private static float ComputeAnalogInputModifier(Vector3 desiredVelocity, float maxSpeed)
        {
            if (maxSpeed > 0.0f && desiredVelocity.sqrMagnitude > 0.0f)
                return Mathf.Clamp01(desiredVelocity.magnitude / maxSpeed);

            return 0.0f;
        }

        /// <summary>
        /// Calculates a new velocity for the given state, applying the effects of friction or braking friction and acceleration or deceleration.
        /// </summary>
        /// <param name="currentVelocity">Character's current velocity.</param>
        /// <param name="desiredVelocity">Target velocity</param>
        /// <param name="maxSpeed">The maximum speed when grounded. Also determines maximum horizontal speed when falling (i.e. not-grounded).</param>
        /// <param name="acceleration">The rate of change of velocity when accelerating (i.e desiredVelocity != Vector3.zero).</param>
        /// <param name="deceleration">The rate at which the character slows down when braking (i.e. not accelerating or if character is exceeding max speed).
        /// This is a constant opposing force that directly lowers velocity by a constant value.</param>
        /// <param name="friction">Setting that affects movement control. Higher values allow faster changes in direction.</param>
        /// <param name="brakingFriction">Friction (drag) coefficient applied when braking (whenever desiredVelocity == Vector3.zero, or if character is exceeding max speed).</param>
        /// <param name="deltaTime">The simulation deltaTime. Defaults to Time.deltaTime.</param>
        /// <returns>Returns the updated velocity</returns>
        
        private static Vector3 CalcVelocity(Vector3 currentVelocity, Vector3 desiredVelocity, float maxSpeed,
            float acceleration, float deceleration, float friction, float brakingFriction, float deltaTime)
        {
            // Compute requested move direction

            float desiredSpeed = desiredVelocity.magnitude;
            Vector3 desiredMoveDirection = desiredSpeed > 0.0f ? desiredVelocity / desiredSpeed : Vector3.zero;

            // Requested acceleration (factoring analog input)

            float analogInputModifier = ComputeAnalogInputModifier(desiredVelocity, maxSpeed);
            Vector3 requestedAcceleration = acceleration * analogInputModifier * desiredMoveDirection;

            // Actual max speed (factoring analog input)

            float actualMaxSpeed = Mathf.Max(0.0f, maxSpeed * analogInputModifier);

            // Friction
            // Only apply braking if there is no input acceleration,
            // or we are over our max speed and need to slow down to it

            bool isZeroAcceleration = requestedAcceleration.isZero();
            bool isVelocityOverMax = currentVelocity.isExceeding(actualMaxSpeed);

            if (isZeroAcceleration || isVelocityOverMax)
            {
                // Pre-braking currentVelocity

                Vector3 oldVelocity = currentVelocity;

                // Apply friction and braking

                currentVelocity = ApplyVelocityBraking(currentVelocity, brakingFriction, deceleration, deltaTime);

                // Don't allow braking to lower us below max speed if we started above it

                if (isVelocityOverMax && currentVelocity.sqrMagnitude < actualMaxSpeed.square() &&
                    Vector3.Dot(requestedAcceleration, oldVelocity) > 0.0f)
                    currentVelocity = oldVelocity.normalized * actualMaxSpeed;
            }
            else
            {
                // Friction, this affects our ability to change direction

                currentVelocity -= (currentVelocity - desiredMoveDirection * currentVelocity.magnitude) * Mathf.Min(friction * deltaTime, 1.0f);
            }

            // Apply acceleration

            if (!isZeroAcceleration)
            {
                float newMaxSpeed = currentVelocity.isExceeding(actualMaxSpeed) ? currentVelocity.magnitude : actualMaxSpeed;

                currentVelocity += requestedAcceleration * deltaTime;
                currentVelocity = currentVelocity.clampedTo(newMaxSpeed);
            }

            // Return new velocity

            return currentVelocity;
        }

        /// <summary>
        /// Helper method to get the velocity of the rigidbody at the worldPoint,
        /// will take the angularVelocity of the rigidbody into account when calculating the velocity.
        /// If the given Rigidbody is a character, will return character's velocity.
        /// </summary>

        private static Vector3 GetRigidbodyVelocity(Rigidbody rigidbody, Vector3 worldPoint)
        {
            if (rigidbody == null)
                return Vector3.zero;

            return rigidbody.TryGetComponent(out CharacterMovement controller)
                ? controller.velocity
                : rigidbody.GetPointVelocity(worldPoint);
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.Walkable value.
        /// </summary>

        private static bool IsWalkable(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.Walkable) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.NotWalkable value.
        /// </summary>

        private static bool IsNotWalkable(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.NotWalkable) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanPerchOn value.
        /// </summary>

        private static bool CanPerchOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanPerchOn) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanNotPerchOn value.
        /// </summary>

        private static bool CanNotPerchOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanNotPerchOn) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanStepOn value.
        /// </summary>

        private static bool CanStepOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanStepOn) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanNotStepOn value.
        /// </summary>

        private static bool CanNotStepOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanNotStepOn) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanRideOn value.
        /// </summary>

        private static bool CanRideOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanRideOn) != 0;
        }

        /// <summary>
        /// Helper method to test if given behavior flags contains CollisionBehavior.CanNotRideOn value.
        /// </summary>

        private static bool CanNotRideOn(CollisionBehavior behaviorFlags)
        {
            return (behaviorFlags & CollisionBehavior.CanNotRideOn) != 0;
        }

        /// <summary>
        /// Helper function to create a capsule of given dimensions.
        /// </summary>
        /// <param name="radius">The capsule radius.</param>
        /// <param name="height">The capsule height.</param>
        /// <param name="center">Output capsule center in local space.</param>
        /// <param name="bottomCenter">Output capsule bottom sphere center in local space.</param>
        /// <param name="topCenter">Output capsule top sphere center in local space.</param>

        private static void MakeCapsule(float radius, float height, out Vector3 center, out Vector3 bottomCenter, out Vector3 topCenter)
        {
            radius = Mathf.Max(radius, 0.0f);
            height = Mathf.Max(height, radius * 2.0f);

            center = height * 0.5f * Vector3.up;

            float sideHeight = height - radius * 2.0f;

            bottomCenter = center - sideHeight * 0.5f * Vector3.up;
            topCenter = center + sideHeight * 0.5f * Vector3.up;
        }

        /// <summary>
        /// Specifies the character's bounding volume (eg: capsule) dimensions.
        /// </summary>
        /// <param name="characterRadius">The character's volume radius.</param>
        /// <param name="characterHeight">The character's volume height</param>

        public void SetDimensions(float characterRadius, float characterHeight)
        {
            _radius = Mathf.Max(characterRadius, 0.0f);
            _height = Mathf.Max(characterHeight, characterRadius * 2.0f);

            MakeCapsule(_radius, _height, out _capsuleCenter, out _capsuleBottomCenter, out _capsuleTopCenter);

#if UNITY_EDITOR
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
#endif

            if (_capsuleCollider)
            {
                _capsuleCollider.radius = _radius;
                _capsuleCollider.height = _height;
                _capsuleCollider.center = _capsuleCenter;
            }
        }

        /// <summary>
        /// Specifies the character's bounding volume (eg: capsule) height.
        /// </summary>
        /// <param name="characterHeight">The character's volume height</param>

        public void SetHeight(float characterHeight)
        {
            _height = Mathf.Max(characterHeight, _radius * 2.0f);

            MakeCapsule(_radius, _height, out _capsuleCenter, out _capsuleBottomCenter, out _capsuleTopCenter);

#if UNITY_EDITOR
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
#endif

            if (_capsuleCollider)
            {
                _capsuleCollider.height = _height;
                _capsuleCollider.center = _capsuleCenter;
            }
        }

        /// <summary>
        /// Cache and initialize required components.
        /// </summary>

        private void CacheComponents()
        {
            _transform = GetComponent<Transform>();

            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody)
            {
                _rigidbody.drag = 0.0f;
                _rigidbody.angularDrag = 0.0f;

                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }

            _capsuleCollider = GetComponent<CapsuleCollider>();
        }

        /// <summary>
        /// Defines the axis that constraints movement, so movement along the given axis is not possible.
        /// </summary>

        public void SetPlaneConstraint(PlaneConstraint constrainAxis, Vector3 planeNormal)
        {
            _planeConstraint = constrainAxis;

            switch (_planeConstraint)
            {
                case PlaneConstraint.None:
                    {
                        _constraintPlaneNormal = Vector3.zero;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.None;

                        break;
                    }

                case PlaneConstraint.ConstrainXAxis:
                    {
                        _constraintPlaneNormal = Vector3.right;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionX;

                        break;
                    }

                case PlaneConstraint.ConstrainYAxis:
                    {
                        _constraintPlaneNormal = Vector3.up;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionY;

                        break;
                    }

                case PlaneConstraint.ConstrainZAxis:
                    {
                        _constraintPlaneNormal = Vector3.forward;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.FreezePositionZ;

                        break;
                    }

                case PlaneConstraint.Custom:
                    {
                        _constraintPlaneNormal = planeNormal;

                        if (_rigidbody)
                            _rigidbody.constraints = RigidbodyConstraints.None;

                        break;
                    }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Returns the given DIRECTION (Normalized) vector constrained to current constraint plane (if _constrainToPlane != None)
        /// or given vector (if _constrainToPlane == None).
        /// </summary>

        public Vector3 ConstrainDirectionToPlane(Vector3 direction)
        {
            return ConstrainVectorToPlane(direction).normalized;
        }

        /// <summary>
        /// Constrain the given vector to current PlaneConstraint (if any).
        /// </summary>

        public Vector3 ConstrainVectorToPlane(Vector3 vector)
        {
            return isConstrainedToPlane ? vector.projectedOnPlane(_constraintPlaneNormal) : vector;
        }

        /// <summary>
        /// Clear last move CollisionFlags.
        /// </summary>

        private void ResetCollisionFlags()
        {
            collisionFlags = CollisionFlags.None;
        }

        /// <summary>
        /// Append HitLocation to current CollisionFlags.
        /// </summary>

        private void UpdateCollisionFlags(HitLocation hitLocation)
        {
            collisionFlags |= (CollisionFlags) hitLocation;
        }

        /// <summary>
        /// Determines the hit location WRT capsule for the given normal.
        /// </summary>

        private HitLocation ComputeHitLocation(Vector3 inNormal)
        {
            float verticalComponent = inNormal.dot(_characterUp);

            if (verticalComponent > kHemisphereLimit)
                return HitLocation.Below;

            return verticalComponent < -kHemisphereLimit ? HitLocation.Above : HitLocation.Sides;
        }

        /// <summary>
        /// Determines if the given collider and impact normal should be considered as walkable ground.
        /// </summary>
        
        private bool IsWalkable(Collider inCollider, Vector3 inNormal)
        {
            // Do not bother if hit is not in capsule bottom sphere

            if (ComputeHitLocation(inNormal) != HitLocation.Below)
                return false;

            // If collision behavior callback is assigned, check walkable / not walkable flags

            if (collisionBehaviorCallback != null)
            {
                CollisionBehavior collisionBehavior = collisionBehaviorCallback.Invoke(inCollider);

                if (IsWalkable(collisionBehavior))
                    return Vector3.Dot(inNormal, _characterUp) > kMaxWalkableSlopeLimit;

                if (IsNotWalkable(collisionBehavior))
                    return Vector3.Dot(inNormal, _characterUp) > kMinWalkableSlopeLimit;
            }

            // If slopeLimitOverride enable, check for SlopeLimitBehavior component
            
            float actualSlopeLimit = _minSlopeLimit;

            if (_slopeLimitOverride && inCollider.TryGetComponent(out SlopeLimitBehavior slopeLimitOverrideComponent))
            {
                switch (slopeLimitOverrideComponent.walkableSlopeBehaviour)
                {
                    case SlopeBehaviour.Walkable:
                        actualSlopeLimit = kMaxWalkableSlopeLimit;
                        break;

                    case SlopeBehaviour.NotWalkable:
                        actualSlopeLimit = kMinWalkableSlopeLimit;
                        break;

                    case SlopeBehaviour.Override:
                        actualSlopeLimit = slopeLimitOverrideComponent.slopeLimitCos;
                        break;

                    case SlopeBehaviour.Default:
                        break;
                }
            }

            // Determine if the given normal is walkable

            return Vector3.Dot(inNormal, _characterUp) > actualSlopeLimit;
        }

        /// <summary>
        /// When moving on walkable ground, and hit a non-walkable, modify hit normal (eg: the blocking hit normal)
        /// since We don't want to be pushed up an unwalkable surface,
        /// or be pushed down into the ground when the impact is on the upper portion of the capsule.
        /// </summary>

        private Vector3 ComputeBlockingNormal(Vector3 inNormal, bool isWalkable)
        {
            if ((isGrounded || _hasLanded) && !isWalkable)
            {
                Vector3 actualGroundNormal = _hasLanded ? _foundGround.normal : _currentGround.normal;

                Vector3 forward = actualGroundNormal.perpendicularTo(inNormal);
                Vector3 blockingNormal = forward.perpendicularTo(_characterUp);

                if (Vector3.Dot(blockingNormal, inNormal) < 0.0f)
                    blockingNormal = -blockingNormal;

                if (!blockingNormal.isZero())
                    inNormal = blockingNormal;

                return inNormal;
            }

            return inNormal;

        }

        /// <summary>
        /// Determines if the given collider should be filtered (ignored) or not.
        /// Return true to filter collider (e.g. Ignore it), false otherwise.
        /// </summary>

        private bool ShouldFilter(Collider otherCollider)
        {
            if (otherCollider == _capsuleCollider || otherCollider.attachedRigidbody == rigidbody)
                return true;
            
            if (_ignoredColliders.Contains(otherCollider))
                return true;

            Rigidbody attachedRigidbody = otherCollider.attachedRigidbody;
            if (attachedRigidbody && _ignoredRigidbodies.Contains(attachedRigidbody))
                return true;
            
            return colliderFilterCallback != null && colliderFilterCallback.Invoke(otherCollider);
        }

        /// <summary>
        /// Makes the character's collider (eg: CapsuleCollider) to ignore all collisions vs otherCollider.
        /// NOTE: The character can still collide with other during a Move call if otherCollider is in CollisionLayers mask.
        /// </summary>

        public void CapsuleIgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            if (otherCollider == null)
                return;

            Physics.IgnoreCollision(_capsuleCollider, otherCollider, ignore);
        }

        /// <summary>
        /// Makes the character to ignore all collisions vs otherCollider.
        /// </summary>

        public void IgnoreCollision(Collider otherCollider, bool ignore = true)
        {
            if (otherCollider == null)
                return;

            if (ignore)
                _ignoredColliders.Add(otherCollider);
            else
                _ignoredColliders.Remove(otherCollider);
        }

        /// <summary>
        /// Makes the character to ignore collisions vs all colliders attached to the otherRigidbody.
        /// </summary>

        public void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
        {
            if (otherRigidbody == null)
                return;

            if (ignore)
                _ignoredRigidbodies.Add(otherRigidbody);
            else
                _ignoredRigidbodies.Remove(otherRigidbody);
        }

        /// <summary>
        /// Clear last Move collision results.
        /// </summary>

        private void ClearCollisionResults()
        {
            _collisionCount = 0;
        }

        /// <summary>
        /// Add a CollisionResult to collisions list found during Move.
        /// If CollisionResult is vs otherRigidbody add first one only.
        /// </summary>

        private void AddCollisionResult(ref CollisionResult collisionResult)
        {
            UpdateCollisionFlags(collisionResult.hitLocation);

            if (collisionResult.rigidbody)
            {
                // Do not process as dynamic collisions, any collision against current riding platform

                if (collisionResult.rigidbody == _movingPlatform.platform)
                    return;

                // We only care about the first collision with a rigidbody

                for (int i = 0; i < _collisionCount; i++)
                {
                    if (collisionResult.rigidbody == _collisionResults[i].rigidbody)
                        return;
                }
            }

            if (_collisionCount < kMaxCollisionCount)
                _collisionResults[_collisionCount++] = collisionResult;
        }

        /// <summary>
        /// Return the number of collisions found during last Move call.
        /// </summary>

        public int GetCollisionCount()
        {
            return _collisionCount;
        }

        /// <summary>
        /// Retrieves a CollisionResult from last Move call list.
        /// </summary>

        public CollisionResult GetCollisionResult(int index)
        {
            return _collisionResults[index];
        }

        /// <summary>
        /// Compute the minimal translation distance (MTD) required to separate the given colliders apart at specified poses.
        /// Uses an inflated capsule for better results.
        /// </summary>

        private bool ComputeInflatedMTD(Vector3 characterPosition, Quaternion characterRotation, float mtdInflation,
            Collider hitCollider, Transform hitTransform, out Vector3 mtdDirection, out float mtdDistance)
        {
            mtdDirection = Vector3.zero;
            mtdDistance = 0.0f;

            _capsuleCollider.radius = _radius + mtdInflation * 1.0f;
            _capsuleCollider.height = _height + mtdInflation * 2.0f;

            bool mtdResult = Physics.ComputePenetration(_capsuleCollider, characterPosition, characterRotation,
                hitCollider, hitTransform.position, hitTransform.rotation, out Vector3 recoverDirection, out float recoverDistance);

            if (mtdResult)
            {
                if (IsFinite(recoverDirection))
                {
                    mtdDirection = recoverDirection;
                    mtdDistance = Mathf.Max(Mathf.Abs(recoverDistance) - mtdInflation, 0.0f) + kKindaSmallNumber;
                }
                else
                {
                    Debug.LogWarning($"Warning: ComputeInflatedMTD_Internal: MTD returned NaN " + recoverDirection.ToString("F4"));
                }
            }

            _capsuleCollider.radius = _radius;
            _capsuleCollider.height = _height;

            return mtdResult;
        }

        /// <summary>
        /// Compute the minimal translation distance (MTD) required to separate the given colliders apart at specified poses.
        /// Uses an inflated capsule for better results, try MTD with a small inflation for better accuracy, then a larger one in case the first one fails due to precision issues.
        /// </summary>

        private bool ComputeMTD(Vector3 characterPosition, Quaternion characterRotation, Collider hitCollider, Transform hitTransform, out Vector3 mtdDirection, out float mtdDistance)
        {
            const float kSmallMTDInflation = 0.0025f;
            const float kLargeMTDInflation = 0.0175f;

            if (ComputeInflatedMTD(characterPosition, characterRotation, kSmallMTDInflation, hitCollider, hitTransform, out mtdDirection, out mtdDistance) ||
                ComputeInflatedMTD(characterPosition, characterRotation, kLargeMTDInflation, hitCollider, hitTransform, out mtdDirection, out mtdDistance))
            {
                // Success

                return true;
            }

            // Failure

            return false;
        }

        /// <summary>
        /// Resolves any character's volume overlaps against specified colliders.
        /// </summary>

        private void ResolveOverlaps(DepenetrationBehavior depenetrationBehavior = DepenetrationBehavior.IgnoreNone)
        {
            if (!detectCollisions)
                return;

            bool ignoreStatic = (depenetrationBehavior & DepenetrationBehavior.IgnoreStatic) != 0;
            bool ignoreDynamic = (depenetrationBehavior & DepenetrationBehavior.IgnoreDynamic) != 0;
            bool ignoreKinematic = (depenetrationBehavior & DepenetrationBehavior.IgnoreKinematic) != 0;

            for (int i = 0; i < _advanced.maxDepenetrationIterations; i++)
            {
                Vector3 top = updatedPosition + _transformedCapsuleTopCenter;
                Vector3 bottom = updatedPosition + _transformedCapsuleBottomCenter;

                int overlapCount = Physics.OverlapCapsuleNonAlloc(bottom, top, _radius, _overlaps, _collisionLayers, triggerInteraction);
                if (overlapCount == 0)
                    break;

                for (int j = 0; j < overlapCount; j++)
                {
                    Collider overlappedCollider = _overlaps[j];

                    if (ShouldFilter(overlappedCollider))
                        continue;

                    Rigidbody attachedRigidbody = overlappedCollider.attachedRigidbody;

                    if (ignoreStatic && attachedRigidbody == null)
                        continue;

                    if (attachedRigidbody)
                    {
                        bool isKinematic = attachedRigidbody.isKinematic;

                        if (ignoreKinematic && isKinematic)
                            continue;

                        if (ignoreDynamic && !isKinematic)
                            continue;
                    }

                    if (ComputeMTD(updatedPosition, updatedRotation, overlappedCollider, overlappedCollider.transform, out Vector3 recoverDirection, out float recoverDistance))
                    {
                        recoverDirection = ConstrainDirectionToPlane(recoverDirection);

                        HitLocation hitLocation = ComputeHitLocation(recoverDirection);

                        bool isWalkable = IsWalkable(overlappedCollider, recoverDirection);

                        Vector3 impactNormal = ComputeBlockingNormal(recoverDirection, isWalkable);

                        updatedPosition += impactNormal * (recoverDistance + kPenetrationOffset);

                        if (_collisionCount < kMaxCollisionCount)
                        {
                            Vector3 point;

                            if (hitLocation == HitLocation.Above)
                                point = updatedPosition + _transformedCapsuleTopCenter - recoverDirection * _radius;
                            else if (hitLocation == HitLocation.Below)
                                point = updatedPosition + _transformedCapsuleBottomCenter - recoverDirection * _radius;
                            else
                                point = updatedPosition + _transformedCapsuleCenter - recoverDirection * _radius;

                            CollisionResult collisionResult = new CollisionResult
                            {
                                startPenetrating = true,

                                hitLocation = hitLocation,
                                isWalkable = isWalkable,

                                position = updatedPosition,

                                velocity = _velocity,
                                otherVelocity = GetRigidbodyVelocity(attachedRigidbody, point),

                                point = point,
                                normal = impactNormal,

                                surfaceNormal = impactNormal,

                                collider = overlappedCollider
                            };

                            AddCollisionResult(ref collisionResult);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check the given capsule against the physics world and return all overlapping colliders.
        /// Return overlapped colliders count.
        /// </summary>

        public int OverlapTest(Vector3 characterPosition, Quaternion characterRotation, float testRadius,
            float testHeight, int layerMask, Collider[] results, QueryTriggerInteraction queryTriggerInteraction)
        {
            MakeCapsule(testRadius, testHeight, out Vector3 _, out Vector3 bottomCenter, out Vector3 topCenter);

            Vector3 top = characterPosition + characterRotation * topCenter;
            Vector3 bottom = characterPosition + characterRotation * bottomCenter;

            int rawOverlapCount =
                Physics.OverlapCapsuleNonAlloc(bottom, top, testRadius, results, layerMask, queryTriggerInteraction);

            if (rawOverlapCount == 0)
                return 0;

            int filteredOverlapCount = rawOverlapCount;

            for (int i = 0; i < rawOverlapCount; i++)
            {
                Collider overlappedCollider = results[i];

                if (ShouldFilter(overlappedCollider))
                {
                    if (i < --filteredOverlapCount)
                        results[i] = results[filteredOverlapCount];
                }
            }

            return filteredOverlapCount;
        }

        /// <summary>
        /// Check the given capsule against the physics world and return all overlapping colliders.
        /// Return an array of overlapped colliders.
        /// </summary>

        public Collider[] OverlapTest(Vector3 characterPosition, Quaternion characterRotation, float testRadius,
            float testHeight, int layerMask, QueryTriggerInteraction queryTriggerInteraction, out int overlapCount)
        {
            overlapCount = OverlapTest(characterPosition, characterRotation, testRadius, testHeight, layerMask,
                _overlaps, queryTriggerInteraction);

            return _overlaps;
        }

        /// <summary>
        /// Checks if any colliders overlaps the character's capsule-shaped volume in world space using testHeight as capsule's height.
        /// Returns true if there is a blocking overlap, false otherwise.
        /// </summary>

        public bool CheckHeight(float testHeight)
        {
            IgnoreCollision(_movingPlatform.platform);

            int overlapCount =
                OverlapTest(position, rotation, radius, testHeight, collisionLayers, _overlaps, triggerInteraction);

            IgnoreCollision(_movingPlatform.platform, false);

            return overlapCount > 0;
        }

        /// <summary>
        /// Return true if the 2D distance to the impact point is inside the edge tolerance (CapsuleRadius minus a small rejection threshold).
        /// Useful for rejecting adjacent hits when finding a ground or landing spot.
        /// </summary>
        
        public bool IsWithinEdgeTolerance(Vector3 characterPosition, Vector3 inPoint, float testRadius)
        {
            float distFromCenterSq = (inPoint - characterPosition).projectedOnPlane(_characterUp).sqrMagnitude;

            float reducedRadius = Mathf.Max(kSweepEdgeRejectDistance + kKindaSmallNumber,
                testRadius - kSweepEdgeRejectDistance);

            return distFromCenterSq < reducedRadius * reducedRadius;
        }

        /// <summary>
        /// Determine whether we should try to find a valid landing spot after an impact with an invalid one (based on the Hit result).
        /// For example, landing on the lower portion of the capsule on the edge of geometry may be a walkable surface, but could have reported an unwalkable surface normal.
        /// </summary>

        private bool ShouldCheckForValidLandingSpot(ref CollisionResult inCollision)
        {
            // See if we hit an edge of a surface on the lower portion of the capsule.
            // In this case the normal will not equal the surface normal, and a downward sweep may find a walkable surface on top of the edge.

            if (inCollision.hitLocation == HitLocation.Below && inCollision.normal != inCollision.surfaceNormal)
            {
                if (IsWithinEdgeTolerance(updatedPosition, inCollision.point, _radius))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Verify that the supplied CollisionResult is a valid landing spot when falling.
        /// </summary>

        private bool IsValidLandingSpot(Vector3 characterPosition, ref CollisionResult inCollision)
        {
            // Reject unwalkable ground normals.

            if (!inCollision.isWalkable)
                return false;

            // Reject hits that are above our lower hemisphere (can happen when sliding down a vertical surface).

            if (inCollision.hitLocation != HitLocation.Below)
                return false;

            // Reject hits that are barely on the cusp of the radius of the capsule

            if (!IsWithinEdgeTolerance(characterPosition, inCollision.point, _radius))
            {
                inCollision.isWalkable = false;

                return false;
            }

            FindGround(characterPosition, out FindGroundResult groundResult);
            {
                inCollision.isWalkable = groundResult.isWalkableGround;

                if (inCollision.isWalkable)
                {
                    _foundGround = groundResult;

                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Casts a ray, from point origin, in direction direction, of length distance, against specified colliders (by layerMask) in the Scene.
        /// </summary>
        
        public bool Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, out RaycastHit hitResult,
            float thickness = 0.0f)
        {
            hitResult = default;

            int rawHitCount = thickness == 0.0f
                ? Physics.RaycastNonAlloc(origin, direction, _hits, distance, layerMask, triggerInteraction)
                : Physics.SphereCastNonAlloc(origin - direction * thickness, thickness, direction, _hits, distance, layerMask, triggerInteraction);

            if (rawHitCount == 0)
                return false;
            
            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (hit.distance <= 0.0f || ShouldFilter(hit.collider))
                    continue;

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = _hits[hitIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts a capsule against all colliders in the Scene and returns detailed information on what was hit.
        /// Returns True when the capsule sweep intersects any collider, otherwise false. 
        /// </summary>
        
        private bool CapsuleCast(Vector3 characterPosition, float castRadius, Vector3 castDirection, float castDistance,
            int layerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = false;

            Vector3 top = characterPosition + _transformedCapsuleTopCenter;
            Vector3 bottom = characterPosition + _transformedCapsuleBottomCenter;

            int rawHitCount = Physics.CapsuleCastNonAlloc(bottom, top, castRadius, castDirection, _hits, castDistance,
                layerMask, triggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = _hits[hitIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts a capsule against all colliders in the Scene and returns detailed information on what was hit.
        /// Returns True when the capsule sweep intersects any collider, otherwise false. 
        /// Unlike previous version this correctly restun (if deried) valid hits for blocking overlaps along with MTD to resolve penetration.
        /// </summary>

        private bool CapsuleCastEx(Vector3 characterPosition, float castRadius, Vector3 castDirection, float castDistance, int layerMask,
            out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, bool ignoreNonBlockingOverlaps = false)
        {
            hitResult = default;

            startPenetrating = default;
            recoverDirection = default;
            recoverDistance = default;

            Vector3 top = characterPosition + _transformedCapsuleTopCenter;
            Vector3 bottom = characterPosition + _transformedCapsuleBottomCenter;

            int rawHitCount =
                Physics.CapsuleCastNonAlloc(bottom, top, castRadius, castDirection, _hits, castDistance, layerMask, triggerInteraction);

            if (rawHitCount == 0)
                return false;

            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                bool isOverlapping = hit.distance <= 0.0f;
                if (isOverlapping)
                {
                    if (ComputeMTD(characterPosition, updatedRotation, hit.collider, hit.collider.transform, out Vector3 mtdDirection, out float mtdDistance))
                    {
                        mtdDirection = ConstrainDirectionToPlane(mtdDirection);

                        HitLocation hitLocation = ComputeHitLocation(mtdDirection);

                        Vector3 point;
                        if (hitLocation == HitLocation.Above)
                            point = characterPosition + _transformedCapsuleTopCenter - mtdDirection * _radius;
                        else if (hitLocation == HitLocation.Below)
                            point = characterPosition + _transformedCapsuleBottomCenter - mtdDirection * _radius;
                        else
                            point = characterPosition + _transformedCapsuleCenter - mtdDirection * _radius;

                        Vector3 impactNormal = ComputeBlockingNormal(mtdDirection, IsWalkable(hit.collider, mtdDirection));

                        hit.point = point;
                        hit.normal = impactNormal;
                        hit.distance = -mtdDistance;
                    }
                }
            }

            Array.Sort(_hits, 0, rawHitCount, _hitComparer);

            float mostOpposingDot = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                bool isOverlapping = hit.distance <= 0.0f && !hit.point.isZero();
                if (isOverlapping)
                {
                    // Overlaps

                    float movementDotNormal = Vector3.Dot(castDirection, hit.normal);

                    if (ignoreNonBlockingOverlaps)
                    {
                        // If we started penetrating, we may want to ignore it if we are moving out of penetration.
                        // This helps prevent getting stuck in walls.

                        bool isMovingOut = movementDotNormal > 0.0f;
                        if (isMovingOut)
                            continue;
                    }
                    
                    if (movementDotNormal < mostOpposingDot)
                    {
                        mostOpposingDot = movementDotNormal;
                        hitIndex = i;
                    }
                }
                else if (hitIndex == -1)
                {
                    // Hits
                    // First non-overlapping blocking hit should be used, if no valid overlapping hit was found (ie, hitIndex == -1).

                    hitIndex = i;
                    break;
                }
            }

            if (hitIndex >= 0)
            {
                hitResult = _hits[hitIndex];

                if (hitResult.distance <= 0.0f)
                {
                    startPenetrating = true;
                    recoverDirection = hitResult.normal;
                    recoverDistance = Mathf.Abs(hitResult.distance);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tests if the character would collide with anything, if it was moved through the Scene.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        private bool SweepTest(Vector3 sweepOrigin, float sweepRadius, Vector3 sweepDirection, float sweepDistance,
            int sweepLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            // Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
            // when moving almost parallel to an obstacle for small distances).

            hitResult = default;

            bool innerCapsuleHit =
                CapsuleCast(sweepOrigin, sweepRadius, sweepDirection, sweepDistance + sweepRadius, sweepLayerMask,
                    out RaycastHit innerCapsuleHitResult, out startPenetrating) && innerCapsuleHitResult.distance <= sweepDistance;

            float outerCapsuleRadius = sweepRadius + kContactOffset;

            bool outerCapsuleHit =
                CapsuleCast(sweepOrigin, outerCapsuleRadius, sweepDirection, sweepDistance + outerCapsuleRadius,
                    sweepLayerMask, out RaycastHit outerCapsuleHitResult, out _) && outerCapsuleHitResult.distance <= sweepDistance;

            bool foundBlockingHit = innerCapsuleHit || outerCapsuleHit;
            if (!foundBlockingHit)
                return false;

            if (!outerCapsuleHit)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kContactOffset);
            }
            else if (innerCapsuleHit && innerCapsuleHitResult.distance < outerCapsuleHitResult.distance)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kContactOffset);
            }
            else
            {
                hitResult = outerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kSmallContactOffset);
            }
            
            return true;
        }

        /// <summary>
        /// Tests if the character would collide with anything, if it was moved through the Scene.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// Unlike previous version this correctly restun (if deried) valid hits for blocking overlaps along with MTD to resolve penetration.
        /// </summary>

        private bool SweepTestEx(Vector3 sweepOrigin, float sweepRadius, Vector3 sweepDirection, float sweepDistance, int sweepLayerMask,
            out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, bool ignoreBlockingOverlaps = false)
        {
            // Cast further than the distance we need, to try to take into account small edge cases (e.g. Casts fail 
            // when moving almost parallel to an obstacle for small distances).

            hitResult = default;

            bool innerCapsuleHit =
                CapsuleCastEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance + sweepRadius, sweepLayerMask,
                out RaycastHit innerCapsuleHitResult, out startPenetrating, out recoverDirection, out recoverDistance, ignoreBlockingOverlaps) && innerCapsuleHitResult.distance <= sweepDistance;

            if (innerCapsuleHit && startPenetrating)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kSmallContactOffset);

                return true;
            }

            float outerCapsuleRadius = sweepRadius + kContactOffset;

            bool outerCapsuleHit =
                CapsuleCast(sweepOrigin, outerCapsuleRadius, sweepDirection, sweepDistance + outerCapsuleRadius, sweepLayerMask,
                out RaycastHit outerCapsuleHitResult, out _) && outerCapsuleHitResult.distance <= sweepDistance;

            bool foundBlockingHit = innerCapsuleHit || outerCapsuleHit;
            if (!foundBlockingHit)
                return false;

            if (!outerCapsuleHit)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kContactOffset);
            }
            else if (innerCapsuleHit && innerCapsuleHitResult.distance < outerCapsuleHitResult.distance)
            {
                hitResult = innerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kContactOffset);
            }
            else
            {
                hitResult = outerCapsuleHitResult;
                hitResult.distance = Mathf.Max(0.0f, hitResult.distance - kSmallContactOffset);
            }

            return true;
        }

        private bool ResolvePenetration(Vector3 displacement, Vector3 proposedAdjustment)
        {
            Vector3 adjustment = ConstrainVectorToPlane(proposedAdjustment);
            if (adjustment.isZero())
                return false;

            // We really want to make sure that precision differences or differences between the overlap test and sweep tests don't put us into another overlap,
            // so make the overlap test a bit more restrictive.

            const float kOverlapInflation = 0.001f;

            if (!(OverlapTest(updatedPosition + adjustment, updatedRotation, _radius + kOverlapInflation, _height, _collisionLayers, _overlaps, triggerInteraction) > 0))
            {
                // Safe to move without sweeping

                updatedPosition += adjustment;

                return true;
            }
            else
            {
                Vector3 lastPosition = updatedPosition;

                // Try sweeping as far as possible, ignoring non-blocking overlaps, otherwise we wouldn't be able to sweep out of the object to fix the penetration.

                bool hit = CapsuleCastEx(updatedPosition, _radius, adjustment.normalized, adjustment.magnitude, _collisionLayers,
                    out RaycastHit sweepHitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance, true);

                if (!hit)
                    updatedPosition += adjustment;
                else
                    updatedPosition += adjustment.normalized * Mathf.Max(sweepHitResult.distance - kSmallContactOffset, 0.0f);

                // Still stuck?

                bool moved = updatedPosition != lastPosition;
                if (!moved && startPenetrating)
                {
                    // Combine two MTD results to get a new direction that gets out of multiple surfaces.

                    Vector3 secondMTD = recoverDirection * (recoverDistance + kContactOffset + kPenetrationOffset);
                    Vector3 combinedMTD = adjustment + secondMTD;
                    
                    if (secondMTD != adjustment && !combinedMTD.isZero())
                    {
                        lastPosition = updatedPosition;
                        
                        hit = CapsuleCastEx(updatedPosition, _radius, combinedMTD.normalized, combinedMTD.magnitude, 
                            _collisionLayers, out sweepHitResult, out _, out _, out _, true);

                        if (!hit)
                            updatedPosition += combinedMTD;
                        else
                            updatedPosition += combinedMTD.normalized * Mathf.Max(sweepHitResult.distance - kSmallContactOffset, 0.0f);

                        moved = updatedPosition != lastPosition;
                    }
                }

                // Still stuck?

                if (!moved)
                {
                    // Try moving the proposed adjustment plus the attempted move direction.
                    // This can sometimes get out of penetrations with multiple objects.

                    Vector3 moveDelta = ConstrainVectorToPlane(displacement);
                    if (!moveDelta.isZero())
                    {
                        lastPosition = updatedPosition;

                        Vector3 newAdjustment = adjustment + moveDelta;
                        hit = CapsuleCastEx(updatedPosition, _radius, newAdjustment.normalized, newAdjustment.magnitude, 
                            _collisionLayers, out sweepHitResult, out _, out _, out _, true);

                        if (!hit)
                            updatedPosition += newAdjustment;
                        else
                            updatedPosition += newAdjustment.normalized * Mathf.Max(sweepHitResult.distance - kSmallContactOffset, 0.0f);

                        moved = updatedPosition != lastPosition;

                        // Finally, try the original move without MTD adjustments, but allowing depenetration along the MTD normal.
                        // This was blocked because ignoreBlockingOverlaps was false for the original move to try a better depenetration normal, but we might be running in to other geometry in the attempt.
                        // This won't necessarily get us all the way out of penetration, but can in some cases and does make progress in exiting the penetration.

                        if (!moved && Vector3.Dot(moveDelta, adjustment) > 0.0f)
                        {
                            lastPosition = updatedPosition;

                            hit = CapsuleCastEx(updatedPosition, _radius, moveDelta.normalized, moveDelta.magnitude,
                                _collisionLayers, out sweepHitResult, out _, out _, out _, true);

                            if (!hit)
                                updatedPosition += moveDelta;
                            else
                                updatedPosition += moveDelta.normalized * Mathf.Max(sweepHitResult.distance - kSmallContactOffset, 0.0f);

                            moved = updatedPosition != lastPosition;
                        }
                    }
                }

                return moved;
            }
        }
        
        /// <summary>
        /// Sweeps the character's volume along its displacement vector, stopping at near hit point if collision is detected or applies full displacement if not.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        private bool MovementSweepTest(Vector3 characterPosition, Vector3 inVelocity, Vector3 displacement,
            out CollisionResult collisionResult)
        {
            collisionResult = default;

            Vector3 sweepOrigin = characterPosition;
            Vector3 sweepDirection = displacement.normalized;

            float sweepRadius = _radius;
            float sweepDistance = displacement.magnitude;

            int sweepLayerMask = _collisionLayers;
            
            bool hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask, 
                out RaycastHit hitResult, out bool startPenetrating, out Vector3 recoverDirection, out float recoverDistance);

            if (startPenetrating)
            {
                // Handle initial penetrations

                Vector3 requestedAdjustement = recoverDirection * (recoverDistance + kContactOffset + kPenetrationOffset);

                if (ResolvePenetration(displacement, requestedAdjustement))
                {
                    // Retry original movement

                    sweepOrigin = updatedPosition;
                    hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                        out hitResult, out startPenetrating, out _, out _);
                }
            }

            if (!hit)
                return false;

            HitLocation hitLocation = ComputeHitLocation(hitResult.normal);

            Vector3 displacementToHit = sweepDirection * hitResult.distance;
            Vector3 remainingDisplacement = displacement - displacementToHit;

            Vector3 hitPosition = sweepOrigin + displacementToHit;

            Vector3 surfaceNormal = hitResult.normal;

            bool isWalkable = false;
            bool hitGround = hitLocation == HitLocation.Below;
            
            if (hitGround)
            {
                surfaceNormal = FindGeomOpposingNormal(displacement, ref hitResult);

                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
            }

            collisionResult = new CollisionResult
            {
                startPenetrating = startPenetrating,

                hitLocation = hitLocation,
                isWalkable = isWalkable,

                position = hitPosition,

                velocity = inVelocity,
                otherVelocity = GetRigidbodyVelocity(hitResult.rigidbody, hitResult.point),

                point = hitResult.point,
                normal = hitResult.normal,

                surfaceNormal = surfaceNormal,

                displacementToHit = displacementToHit,
                remainingDisplacement = remainingDisplacement,

                collider = hitResult.collider,

                hitResult = hitResult
            };

            return true;
        }

        /// <summary>
        /// Sweeps the character's volume along its displacement vector, stopping at near hit point if collision is detected.
        /// Returns True when the rigidbody sweep intersects any collider, otherwise false.
        /// </summary>

        public bool MovementSweepTest(Vector3 characterPosition, Vector3 sweepDirection, float sweepDistance,
            out CollisionResult collisionResult)
        {
            return MovementSweepTest(characterPosition, velocity, sweepDirection * sweepDistance, out collisionResult);
        }

        /// <summary>
        /// Limit the slide vector when falling if the resulting slide might boost the character faster upwards.
        /// </summary>

        private Vector3 HandleSlopeBoosting(Vector3 slideResult, Vector3 displacement, Vector3 inNormal)
        {
            Vector3 result = slideResult;

            float yResult = Vector3.Dot(result, _characterUp);
            if (yResult > 0.0f)
            {
                // Don't move any higher than we originally intended.

                float yLimit = Vector3.Dot(displacement, _characterUp);
                if (yResult - yLimit > kKindaSmallNumber)
                {
                    if (yLimit > 0.0f)
                    {
                        // Rescale the entire vector (not just the Z component) otherwise we change the direction and likely head right back into the impact.

                        float upPercent = yLimit / yResult;
                        result *= upPercent;
                    }
                    else
                    {
                        // We were heading down but were going to deflect upwards. Just make the deflection horizontal.

                        result = Vector3.zero;
                    }

                    // Make remaining portion of original result horizontal and parallel to impact normal.

                    Vector3 lateralRemainder = (slideResult - result).projectedOnPlane(_characterUp);
                    Vector3 lateralNormal = inNormal.projectedOnPlane(_characterUp).normalized;
                    Vector3 adjust = lateralRemainder.projectedOnPlane(lateralNormal);

                    result += adjust;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculate slide vector along a surface.
        /// </summary>

        private Vector3 ComputeSlideVector(Vector3 displacement, Vector3 inNormal, bool isWalkable)
        {
            if (isGrounded)
            {
                if (isWalkable)
                    displacement = displacement.tangentTo(inNormal, _characterUp);
                else
                {
                    Vector3 right = inNormal.perpendicularTo(groundNormal);
                    Vector3 up = right.perpendicularTo(inNormal);

                    displacement = displacement.projectedOnPlane(inNormal);
                    displacement = displacement.tangentTo(up, _characterUp);
                }
            }
            else
            {
                if (isWalkable)
                {
                    if (_isConstrainedToGround)
                        displacement = displacement.projectedOnPlane(_characterUp);
                    
                    displacement = displacement.projectedOnPlane(inNormal);
                }
                else
                {
                    Vector3 slideResult = displacement.projectedOnPlane(inNormal);

                    if (_isConstrainedToGround)
                        slideResult = HandleSlopeBoosting(slideResult, displacement, inNormal);
                    
                    displacement = slideResult;
                }
            }

            return ConstrainVectorToPlane(displacement);
        }

        /// <summary>
        /// Resolve collisions of Character's bounding volume during a Move call.
        /// </summary>

        private int SlideAlongSurface(int iteration, Vector3 inputDisplacement, ref Vector3 inVelocity,
            ref Vector3 displacement, ref CollisionResult inHit, ref Vector3 prevNormal)
        {
            if (useFlatTop && inHit.hitLocation == HitLocation.Above)
            {
                Vector3 surfaceNormal = FindBoxOpposingNormal(displacement, inHit.normal, inHit.transform);

                if (inHit.normal != surfaceNormal)
                {
                    inHit.normal = surfaceNormal;
                    inHit.surfaceNormal = surfaceNormal;
                }
            }

            inHit.normal = ComputeBlockingNormal(inHit.normal, inHit.isWalkable);

            if (inHit.isWalkable && isConstrainedToGround)
            {
                inVelocity = ComputeSlideVector(inVelocity, inHit.normal, true);
                displacement = ComputeSlideVector(displacement, inHit.normal, true);
            }
            else
            {
                if (iteration == 0)
                {
                    inVelocity = ComputeSlideVector(inVelocity, inHit.normal, inHit.isWalkable);
                    displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);

                    iteration++;
                }
                else if (iteration == 1)
                {
                    Vector3 crease = prevNormal.perpendicularTo(inHit.normal);

                    Vector3 oVel = inputDisplacement.projectedOnPlane(crease);

                    Vector3 nVel = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                            nVel = nVel.projectedOnPlane(crease);

                    if (oVel.dot(nVel) <= 0.0f || prevNormal.dot(inHit.normal) < 0.0f)
                    {
                        inVelocity = ConstrainVectorToPlane(inVelocity.projectedOn(crease));
                        displacement = ConstrainVectorToPlane(displacement.projectedOn(crease));

                        ++iteration;
                    }
                    else
                    {
                        inVelocity = ComputeSlideVector(inVelocity, inHit.normal, inHit.isWalkable);
                        displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    }
                }
                else
                {
                    inVelocity = Vector3.zero;
                    displacement = Vector3.zero;
                }

                prevNormal = inHit.normal;
            }

            return iteration;
        }

        /// <summary>
        /// Performs collision constrained movement.
        /// This refers to the process of smoothly sliding a moving entity along any obstacles encountered.
        /// Updates _probingPosition.
        /// </summary>

        private void PerformMovement(float deltaTime)
        {
            // Resolve initial overlaps

            DepenetrationBehavior depenetrationFlags = !enablePhysicsInteraction
                ? DepenetrationBehavior.IgnoreDynamic
                : DepenetrationBehavior.IgnoreNone;

            ResolveOverlaps(depenetrationFlags);

            //
            // If grounded, discard velocity vertical component

            if (isGrounded)
                _velocity = _velocity.projectedOnPlane(_characterUp);

            // Compute displacement

            Vector3 displacement = _velocity * deltaTime;

            //
            // If grounded, reorient DISPLACEMENT along current ground normal

            if (isGrounded)
            {
                displacement = displacement.tangentTo(groundNormal, _characterUp);
                displacement = ConstrainVectorToPlane(displacement);
            }

            //
            // Cache pre movement displacement

            Vector3 inputDisplacement = displacement;

            //
            // Prevent moving into current BLOCKING overlaps, treat those as collisions and slide along 

            int iteration = 0;
            Vector3 prevNormal = default;

            for (int i = 0; i < _collisionCount; i++)
            {
                ref CollisionResult collisionResult = ref _collisionResults[i];

                bool opposesMovement = displacement.dot(collisionResult.normal) < 0.0f;
                if (!opposesMovement)
                    continue;
                
                // If falling, check if hit is a valid landing spot

                if (isConstrainedToGround && !isOnWalkableGround)
                {
                    if (IsValidLandingSpot(updatedPosition, ref collisionResult))
                    {
                        _hasLanded = true;
                        landedVelocity = collisionResult.velocity;
                    }
                    else
                    {
                        // See if we can convert a normally invalid landing spot (based on the hit result) to a usable one.

                        if (collisionResult.hitLocation == HitLocation.Below)
                        {
                            FindGround(updatedPosition, out FindGroundResult groundResult);

                            collisionResult.isWalkable = groundResult.isWalkableGround;
                            if (collisionResult.isWalkable)
                            {
                                _foundGround = groundResult;

                                _hasLanded = true;
                                landedVelocity = collisionResult.velocity;
                            }
                        }
                    }

                    // If failed to find a valid landing spot but hit ground, update _foundGround with sweep hit result

                    if (!_hasLanded && collisionResult.hitLocation == HitLocation.Below)
                    {
                        _foundGround.SetFromSweepResult(true, false, updatedPosition, collisionResult.point,
                            collisionResult.normal, collisionResult.surfaceNormal, collisionResult.collider,
                            collisionResult.hitResult.distance);
                    }
                }

                //
                // Slide along blocking overlap

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref _velocity, ref displacement,
                    ref collisionResult, ref prevNormal);
            }

            //
            // Perform collision constrained movement (aka: collide and slide)
            
            int maxSlideCount = _advanced.maxMovementIterations;
            while (detectCollisions && maxSlideCount-- > 0 && displacement.sqrMagnitude > _advanced.minMoveDistanceSqr)
            {
                bool collided = MovementSweepTest(updatedPosition, _velocity, displacement,
                    out CollisionResult collisionResult);

                if (!collided)
                    break;

                // Apply displacement up to hit (near position) and update displacement with remaining displacement

                updatedPosition += collisionResult.displacementToHit;

                displacement = collisionResult.remainingDisplacement;

                // Hit a 'barrier', try to step up

                if (isGrounded && !collisionResult.isWalkable)
                {
                    if (CanStepUp(collisionResult.collider) &&
                        StepUp(ref collisionResult, out CollisionResult stepResult))
                    {
                        updatedPosition = stepResult.position;

                        displacement = Vector3.zero;
                        break;
                    }
                }

                // If falling, check if hit is a valid landing spot

                if (isConstrainedToGround && !isOnWalkableGround)
                {
                    if (IsValidLandingSpot(updatedPosition, ref collisionResult))
                    {
                        _hasLanded = true;
                        landedVelocity = collisionResult.velocity;
                    }
                    else
                    {
                        // See if we can convert a normally invalid landing spot (based on the hit result) to a usable one.

                        if (ShouldCheckForValidLandingSpot(ref collisionResult))
                        {
                            FindGround(updatedPosition, out FindGroundResult groundResult);

                            collisionResult.isWalkable = groundResult.isWalkableGround;
                            if (collisionResult.isWalkable)
                            {
                                _foundGround = groundResult;

                                _hasLanded = true;
                                landedVelocity = collisionResult.velocity;
                            }
                        }
                    }

                    // If failed to find a valid landing spot but hit ground, update _foundGround with sweep hit result

                    if (!_hasLanded && collisionResult.hitLocation == HitLocation.Below)
                    {
                        float sweepDistance = collisionResult.hitResult.distance;
                        Vector3 surfaceNormal = collisionResult.surfaceNormal;

                        _foundGround.SetFromSweepResult(true, false, updatedPosition, sweepDistance,
                            ref collisionResult.hitResult, surfaceNormal);
                    }
                }
                
                //
                // Resolve collision (slide along hit surface)

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref _velocity, ref displacement,
                    ref collisionResult, ref prevNormal);

                //
                // Cache collision result

                AddCollisionResult(ref collisionResult);
            }

            //
            // Apply remaining displacement

            if (displacement.sqrMagnitude > _advanced.minMoveDistanceSqr)
                updatedPosition += displacement;

            //
            // If grounded, discard vertical movement BUT preserve its magnitude

            if (isGrounded)
            {
                _velocity = _velocity.projectedOnPlane(_characterUp).normalized * _velocity.magnitude;
                _velocity = ConstrainVectorToPlane(_velocity);
            }
        }

        /// <summary>
        /// Determines if can perch on other collider depending CollisionBehavior flags (if any).
        /// </summary>

        private bool CanPerchOn(Collider otherCollider)
        {
            // Validate input collider

            if (otherCollider == null)
                return false;

            // If collision behavior callback is assigned, use it

            if (collisionBehaviorCallback != null)
            {
                CollisionBehavior collisionBehavior = collisionBehaviorCallback.Invoke(otherCollider);

                if (CanPerchOn(collisionBehavior))
                    return true;

                if (CanNotPerchOn(collisionBehavior))
                    return false;
            }

            // Default case, managed by perchOffset

            return true;
        }

        /// <summary>
        /// Returns The distance from the edge of the capsule within which we don't allow the character to perch on the edge of a surface.
        /// </summary>
        
        private float GetPerchRadiusThreshold()
        {
	        // Don't allow negative values.
	        
            return Mathf.Max(0.0f, _radius - perchOffset);
        }

        /// <summary>
        /// Returns the radius within which we can stand on the edge of a surface without falling (if this is a walkable surface).
        /// </summary>

        private float GetValidPerchRadius(Collider otherCollider)
        {
            if (!CanPerchOn(otherCollider))
                return 0.0011f;
            
            return Mathf.Clamp(_perchOffset, 0.0011f, _radius);
        }

        /// <summary>
        /// Check if the result of a sweep test (passed in InHit) might be a valid location to perch, in which case we should use ComputePerchResult to validate the location.
        /// </summary>

        private bool ShouldComputePerchResult(Vector3 characterPosition, ref RaycastHit inHit)
        {
            // Don't try to perch if the edge radius is very small.
	        
            if (GetPerchRadiusThreshold() <= kSweepEdgeRejectDistance)
	        {
		        return false;
	        }

            float distFromCenterSq = (inHit.point - characterPosition).projectedOnPlane(_characterUp).sqrMagnitude;
            float standOnEdgeRadius = GetValidPerchRadius(inHit.collider);

            if (distFromCenterSq <= standOnEdgeRadius.square())
            {
                // Already within perch radius.

                return false;
            }

            return true;
        }

        /// <summary>
        /// Casts a capsule against specified colliders (by layerMask) in the Scene and returns detailed information on what was hit.
        /// </summary>

        private bool CapsuleCast(Vector3 point1, Vector3 point2, float castRadius, Vector3 castDirection,
            float castDistance, int castLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = false;

            int rawHitCount = Physics.CapsuleCastNonAlloc(point1, point2, castRadius, castDirection, _hits,
                castDistance, castLayerMask, triggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = _hits[hitIndex];
                return true;
            }

            return false;
        }

        /// <summary>
        /// Casts a box along a ray and returns detailed information on what was hit.
        /// </summary>

        private bool BoxCast(Vector3 center, Vector3 halfExtents, Quaternion orientation, Vector3 castDirection,
            float castDistance, int castLayerMask, out RaycastHit hitResult, out bool startPenetrating)
        {
            hitResult = default;
            startPenetrating = default;

            int rawHitCount = Physics.BoxCastNonAlloc(center, halfExtents, castDirection, _hits, orientation,
                castDistance, castLayerMask, triggerInteraction);

            if (rawHitCount == 0)
                return false;

            float closestDistance = Mathf.Infinity;

            int hitIndex = -1;
            for (int i = 0; i < rawHitCount; i++)
            {
                ref RaycastHit hit = ref _hits[i];
                if (ShouldFilter(hit.collider))
                    continue;

                if (hit.distance <= 0.0f)
                    startPenetrating = true;
                else if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    hitIndex = i;
                }
            }

            if (hitIndex != -1)
            {
                hitResult = _hits[hitIndex];
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Downwards (along character's up axis) sweep against the world and return the first blocking hit.
        /// </summary>
        
        private bool GroundSweepTest(Vector3 characterPosition, float capsuleRadius, float capsuleHalfHeight,
            float sweepDistance, out RaycastHit hitResult, out bool startPenetrating)
        {
            bool foundBlockingHit;

            if (!useFlatBaseForGroundChecks)
            {
                Vector3 characterCenter = characterPosition + _transformedCapsuleCenter;

                Vector3 point1 = characterCenter - _characterUp * (capsuleHalfHeight - capsuleRadius);
                Vector3 point2 = characterCenter + _characterUp * (capsuleHalfHeight - capsuleRadius);

                Vector3 sweepDirection = -1.0f * _characterUp;

                foundBlockingHit = CapsuleCast(point1, point2, capsuleRadius, sweepDirection, sweepDistance,
                    _collisionLayers, out hitResult, out startPenetrating);
            }
            else
            {
                // First test with the box rotated so the corners are along the major axes (ie rotated 45 degrees).

                Vector3 center = characterPosition + _transformedCapsuleCenter;
                Vector3 halfExtents = new Vector3(capsuleRadius * 0.707f, capsuleHalfHeight, capsuleRadius * 0.707f);
                
                Quaternion sweepOrientation = rotation * Quaternion.Euler(0f, -rotation.eulerAngles.y, 0f);
                Vector3 sweepDirection = -1.0f * _characterUp;

                LayerMask sweepLayerMask = _collisionLayers;

                foundBlockingHit = BoxCast(center, halfExtents, sweepOrientation * Quaternion.Euler(0.0f, 45.0f, 0.0f),
                    sweepDirection, sweepDistance, sweepLayerMask, out hitResult, out startPenetrating);

                if (!foundBlockingHit && !startPenetrating)
                {
                    // Test again with the same box, not rotated.

                    foundBlockingHit = BoxCast(center, halfExtents, sweepOrientation, sweepDirection, sweepDistance,
                        sweepLayerMask, out hitResult, out startPenetrating);
                }
            }
            
            return foundBlockingHit;
        }

        /// <summary>
        /// Compute distance to the ground from bottom sphere of capsule and store the result in collisionResult.
        /// This distance is the swept distance of the capsule to the first point impacted by the lower hemisphere,
        /// or distance from the bottom of the capsule in the case of a raycast.
        /// </summary>
        
        public void ComputeGroundDistance(Vector3 characterPosition, float sweepRadius, float sweepDistance,
            float castDistance, out FindGroundResult outGroundResult)
        {
            outGroundResult = default;

            // We require the sweep distance to be >= the raycast distance,
            // otherwise the HitResult can't be interpreted as the sweep result.

            if (sweepDistance < castDistance)
                return;

            float characterRadius = _radius;
            float characterHeight = _height;
            float characterHalfHeight = characterHeight * 0.5f;

            bool foundGround = default;
            bool startPenetrating = default;

            // Sweep test

            if (sweepDistance > 0.0f && sweepRadius > 0.0f)
            {
                // Use a shorter height to avoid sweeps giving weird results if we start on a surface.
                // This also allows us to adjust out of penetrations.

                const float kShrinkScale = 0.9f;
                float shrinkHeight = (characterHalfHeight - characterRadius) * (1.0f - kShrinkScale);

                float capsuleRadius = sweepRadius;
                float capsuleHalfHeight = characterHalfHeight - shrinkHeight;

                float actualSweepDistance = sweepDistance + shrinkHeight;

                foundGround = GroundSweepTest(characterPosition, capsuleRadius, capsuleHalfHeight, actualSweepDistance,
                    out RaycastHit hitResult, out startPenetrating);

                if (foundGround || startPenetrating)
                {
                    // Reject hits adjacent to us, we only care about hits on the bottom portion of our capsule.
                    // Check 2D distance to impact point, reject if within a tolerance from radius.

                    if (startPenetrating || !IsWithinEdgeTolerance(characterPosition, hitResult.point, capsuleRadius))
                    {
                        // Use a capsule with a slightly smaller radius and shorter height to avoid the adjacent object.
                        // Capsule must not be nearly zero or the trace will fall back to a line trace from the start point and have the wrong length.

                        const float kShrinkScaleOverlap = 0.1f;
                        shrinkHeight = (characterHalfHeight - characterRadius) * (1.0f - kShrinkScaleOverlap);

                        capsuleRadius = Mathf.Max(0.0011f, capsuleRadius - kSweepEdgeRejectDistance - kKindaSmallNumber);
                        capsuleHalfHeight = Mathf.Max(capsuleRadius, characterHalfHeight - shrinkHeight);

                        actualSweepDistance = sweepDistance + shrinkHeight;

                        foundGround = GroundSweepTest(characterPosition, capsuleRadius, capsuleHalfHeight,
                            actualSweepDistance, out hitResult, out startPenetrating);
                    }

                    if (foundGround && !startPenetrating)
                    {
                        // Reduce hit distance by shrinkHeight because we shrank the capsule for the trace.
                        // We allow negative distances here, because this allows us to pull out of penetrations.

                        float maxPenetrationAdjust = Mathf.Max(kMaxGroundDistance, characterRadius);
                        float sweepResult = Mathf.Max(-maxPenetrationAdjust, hitResult.distance - shrinkHeight);

                        Vector3 sweepDirection = -1.0f * _characterUp;
                        Vector3 hitPosition = characterPosition + sweepDirection * sweepResult;

                        Vector3 surfaceNormal = hitResult.normal;

                        bool isWalkable = false;
                        bool hitGround = sweepResult <= sweepDistance &&
                                         ComputeHitLocation(hitResult.normal) == HitLocation.Below;
                        
                        if (hitGround)
                        {
                            if (useFlatBaseForGroundChecks)
                                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
                            else
                            {
                                surfaceNormal = FindGeomOpposingNormal(sweepDirection * sweepDistance, ref hitResult);

                                isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
                            }
                        }

                        outGroundResult.SetFromSweepResult(hitGround, isWalkable, hitPosition, sweepResult,
                            ref hitResult, surfaceNormal);

                        if (outGroundResult.isWalkableGround)
                            return;
                    }
                }
            }

            // Since we require a longer sweep than raycast, we don't want to run the raycast if the sweep missed everything.
            // We do however want to try a raycast if the sweep was stuck in penetration.

            if (!foundGround && !startPenetrating)
                return;

            // Ray cast

            if (castDistance > 0.0f)
            {
                Vector3 rayOrigin = characterPosition + _transformedCapsuleCenter;
                Vector3 rayDirection = -1.0f * _characterUp;

                float shrinkHeight = characterHalfHeight;
                float rayLength = castDistance + shrinkHeight;

                foundGround = Raycast(rayOrigin, rayDirection, rayLength, _collisionLayers, out RaycastHit hitResult);

                if (foundGround && hitResult.distance > 0.0f)
                {
                    // Reduce hit distance by shrinkHeight because we started the ray higher than the base.
                    // We allow negative distances here, because this allows us to pull out of penetrations.

                    float MaxPenetrationAdjust = Mathf.Max(kMaxGroundDistance, characterRadius);
                    float castResult = Mathf.Max(-MaxPenetrationAdjust, hitResult.distance - shrinkHeight);
                    
                    if (castResult <= castDistance && IsWalkable(hitResult.collider, hitResult.normal))
                    {
                        outGroundResult.SetFromRaycastResult(true, true, outGroundResult.position,
                            outGroundResult.groundDistance, castResult, ref hitResult);

                        return;
                    }
                }
            }

            // No hits were acceptable.

            outGroundResult.isWalkable = false;
        }

        /// <summary>
        /// Compute the sweep result of the smaller capsule with radius specified by GetValidPerchRadius(),
        /// and return true if the sweep contacts a valid walkable normal within inMaxGroundDistance of impact point.
        /// This may be used to determine if the capsule can or cannot stay at the current location if perched on the edge of a small ledge or unwalkable surface. 
        /// </summary>

        private bool ComputePerchResult(Vector3 characterPosition, float testRadius, float inMaxGroundDistance,
            ref RaycastHit inHit, out FindGroundResult perchGroundResult)
        {
            perchGroundResult = default;

            if (inMaxGroundDistance <= 0.0f)
                return false;

            // Sweep further than actual requested distance, because a reduced capsule radius means we could miss some hits that the normal radius would contact.

            float inHitAboveBase = Mathf.Max(0.0f, Vector3.Dot(inHit.point - characterPosition, _characterUp));
            float perchCastDist = Mathf.Max(0.0f, inMaxGroundDistance - inHitAboveBase);
            float perchSweepDist = Mathf.Max(0.0f, inMaxGroundDistance);

            float actualSweepDist = perchSweepDist + _radius;
            ComputeGroundDistance(characterPosition, testRadius, actualSweepDist, perchCastDist, out perchGroundResult);

            if (!perchGroundResult.isWalkable)
                return false;
            else if (inHitAboveBase + perchGroundResult.groundDistance > inMaxGroundDistance)
            {
                // Hit something past max distance

                perchGroundResult.isWalkable = false;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sweeps a vertical cast to find the ground for the capsule at the given location.
        /// Will attempt to perch if ShouldComputePerchResult() returns true for the downward sweep result.
        /// No ground will be found if collision is disabled (eg: detectCollisions == false).
        /// </summary>

        public void FindGround(Vector3 characterPosition, out FindGroundResult outGroundResult)
        {
            // No collision, no ground...

            if (!_detectCollisions)
            {
                outGroundResult = default;
                return;
            }

            // Increase height check slightly if walking,
            // to prevent ground height adjustment from later invalidating the ground result.

            float heightCheckAdjust = isGrounded ? kMaxGroundDistance + kKindaSmallNumber : -kMaxGroundDistance;
            float sweepDistance = Mathf.Max(kMaxGroundDistance, stepOffset + heightCheckAdjust);

            // Sweep ground

            ComputeGroundDistance(characterPosition, _radius, sweepDistance, sweepDistance, out outGroundResult);

            // outGroundResult.hitResult is now the result of the vertical ground check.
            // See if we should try to "perch" at this location.

            if (outGroundResult.hitGround && !outGroundResult.isRaycastResult)
            {
                Vector3 positionOnGround = outGroundResult.position;

                if (ShouldComputePerchResult(positionOnGround, ref outGroundResult.hitResult))
                {
                    float maxPerchGroundDistance = sweepDistance;
                    if (isGrounded)
                        maxPerchGroundDistance += perchAdditionalHeight;

                    float validPerchRadius = GetValidPerchRadius(outGroundResult.collider);

                    if (ComputePerchResult(positionOnGround, validPerchRadius, maxPerchGroundDistance,
                        ref outGroundResult.hitResult, out FindGroundResult perchGroundResult))
                    {
                        // Don't allow the ground distance adjustment to push us up too high,
                        // or we will move beyond the perch distance and fall next time.

                        float moveUpDist = kAvgGroundDistance - outGroundResult.groundDistance;
                        if (moveUpDist + perchGroundResult.groundDistance >= maxPerchGroundDistance)
                        {
                            outGroundResult.groundDistance = kAvgGroundDistance;
                        }

                        // If the regular capsule is on an unwalkable surface but the perched one would allow us to stand,
                        // override the normal to be one that is walkable.

                        if (!outGroundResult.isWalkableGround)
                        {
                            // Ground distances are used as the distance of the regular capsule to the point of collision,
                            // to make sure AdjustGroundHeight() behaves correctly.

                            float groundDistance = outGroundResult.groundDistance;
                            float raycastDistance = Mathf.Max(kMinGroundDistance, groundDistance);

                            outGroundResult.SetFromRaycastResult(true, true, outGroundResult.position, groundDistance,
                                raycastDistance, ref perchGroundResult.hitResult);
                        }
                    }
                    else
                    {
                        // We had no ground (or an invalid one because it was unwalkable), and couldn't perch here,
                        // so invalidate ground (which will cause us to start falling).

                        outGroundResult.isWalkable = false;
                    }
                }
            }
        }

        /// <summary>
        /// Adjust distance from ground, trying to maintain a slight offset from the ground when walking (based on current GroundResult).
        /// Only if character isConstrainedToGround == true.
        /// </summary>

        private void AdjustGroundHeight()
        {
            // If we have a ground check that hasn't hit anything, don't adjust height.

            if (!_currentGround.isWalkableGround || !isConstrainedToGround)
                return;

            float lastGroundDistance = _currentGround.groundDistance;
            
            if (_currentGround.isRaycastResult)
            {
                if (lastGroundDistance < kMinGroundDistance && _currentGround.raycastDistance >= kMinGroundDistance)
                {
                    // This would cause us to scale unwalkable walls

                    return;
                }
                else
                {
                    // Falling back to a raycast means the sweep was unwalkable (or in penetration).
                    // Use the ray distance for the vertical adjustment.

                    lastGroundDistance = _currentGround.raycastDistance;
                }
            }

            // Move up or down to maintain ground height.

            if (lastGroundDistance < kMinGroundDistance || lastGroundDistance > kMaxGroundDistance)
            {
                float initialY = Vector3.Dot(updatedPosition, _characterUp);
                float moveDistance = kAvgGroundDistance - lastGroundDistance;

                Vector3 displacement = _characterUp * moveDistance;

                Vector3 sweepOrigin = updatedPosition;
                Vector3 sweepDirection = displacement.normalized;

                float sweepRadius = _radius;
                float sweepDistance = displacement.magnitude;

                int sweepLayerMask = _collisionLayers;

                bool hit = SweepTestEx(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                    out RaycastHit hitResult, out bool startPenetrating, out _, out _, true);

                if (!hit && !startPenetrating)
                {
                    // No collision, apply full displacement

                    updatedPosition += displacement;
                    _currentGround.groundDistance += moveDistance;
                }
                else if (moveDistance > 0.0f)
                {
                    // Moving up

                    updatedPosition += sweepDirection * hitResult.distance;

                    float currentY = Vector3.Dot(updatedPosition, _characterUp);
                    _currentGround.groundDistance += currentY - initialY;
                }
                else
                {
                    // Moving down

                    updatedPosition += sweepDirection * hitResult.distance;

                    float currentY = Vector3.Dot(updatedPosition, _characterUp);
                    _currentGround.groundDistance = currentY - initialY;
                }
            }

            // Adjust root transform position (accounting offset and skinWidth)

            if (_rootTransform)
            {
                _rootTransform.localPosition =
                    _rootTransformOffset - new Vector3(0.0f, kAvgGroundDistance, 0.0f);
            }
        }
        
        /// <summary>
        /// Determines if the character is able to step up on given collider.
        /// </summary>

        private bool CanStepUp(Collider otherCollider)
        {
            // Validate input collider

            if (otherCollider == null)
                return false;

            // If collision behavior callback assigned, use it

            if (collisionBehaviorCallback != null)
            {
                CollisionBehavior collisionBehavior = collisionBehaviorCallback.Invoke(otherCollider);

                if (CanStepOn(collisionBehavior))
                    return true;

                if (CanNotStepOn(collisionBehavior))
                    return false;
            }

            // Default case, managed by stepOffset

            return true;
        }

        /// <summary>
        /// Move up steps or slope.
        /// Does nothing and returns false if CanStepUp(collider) returns false, true if the step up was successful.
        /// </summary>

        private bool StepUp(ref CollisionResult inCollision, out CollisionResult stepResult)
        {
            stepResult = default;

            // Don't bother stepping up if top of capsule is hitting something.

            if (inCollision.hitLocation == HitLocation.Above)
                return false;

            // We need to enforce max step height off the actual point of impact with the ground.
            
            float characterInitialGroundPositionY = Vector3.Dot(inCollision.position, _characterUp);
            float groundPointY = characterInitialGroundPositionY;
            
            float actualGroundDistance = Mathf.Max(0.0f, _currentGround.GetDistanceToGround());
            characterInitialGroundPositionY -= actualGroundDistance;

            float stepTravelUpHeight = Mathf.Max(0.0f, stepOffset - actualGroundDistance);
            float stepTravelDownHeight = stepOffset + kMaxGroundDistance * 2.0f;

            bool hitVerticalFace =
                !IsWithinEdgeTolerance(inCollision.position, inCollision.point, _radius + kContactOffset);

            if (!_currentGround.isRaycastResult && !hitVerticalFace)
                groundPointY = Vector3.Dot(groundPoint, _characterUp);
            else
                groundPointY -= _currentGround.groundDistance;
            
            // Don't step up if the impact is below us, accounting for distance from ground.

            float initialImpactY = Vector3.Dot(inCollision.point, _characterUp);
            if (initialImpactY <= characterInitialGroundPositionY)
                return false;
            
            // Step up, treat as vertical wall

            Vector3 sweepOrigin = inCollision.position;
            Vector3 sweepDirection = _characterUp;

            float sweepRadius = _radius;
            float sweepDistance = stepTravelUpHeight;

            int sweepLayerMask = _collisionLayers;

            bool foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out RaycastHit hitResult, out bool startPenetrating);

            if (startPenetrating)
                return false;

            if (!foundBlockingHit)
                sweepOrigin += sweepDirection * sweepDistance;
            else
                sweepOrigin += sweepDirection * hitResult.distance;

            // Step forward (lateral displacement only)

            Vector3 displacement = inCollision.remainingDisplacement;
            Vector3 displacement2D = ConstrainVectorToPlane(Vector3.ProjectOnPlane(displacement, _characterUp));

            sweepDistance = displacement.magnitude;
            sweepDirection = displacement2D.normalized;            

            foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out hitResult, out startPenetrating);

            if (startPenetrating)
                return false;

            if (!foundBlockingHit)
                sweepOrigin += sweepDirection * sweepDistance;
            else
            {
                // Could not hurdle the 'barrier', return

                return false;
            }

            // Step down

            sweepDirection = -_characterUp;
            sweepDistance = stepTravelDownHeight;

            foundBlockingHit = SweepTest(sweepOrigin, sweepRadius, sweepDirection, sweepDistance, sweepLayerMask,
                out hitResult, out startPenetrating);

            if (!foundBlockingHit || startPenetrating)
                return false;

            // See if this step sequence would have allowed us to travel higher than our max step height allows.

            float deltaY = Vector3.Dot(hitResult.point, _characterUp) - groundPointY;
            if (deltaY > stepOffset)
                return false;

            // Is position on step clear ?

            Vector3 positionOnStep = sweepOrigin + sweepDirection * hitResult.distance;

            if (OverlapTest(positionOnStep, updatedRotation, _radius, _height, _collisionLayers, _overlaps, triggerInteraction) > 0)
                return false;
            
            // Reject unwalkable surface normals here.

            Vector3 surfaceNormal = FindGeomOpposingNormal(sweepDirection * sweepDistance, ref hitResult);

            bool isWalkable = IsWalkable(hitResult.collider, surfaceNormal);
            if (!isWalkable)
            {
                // Reject if normal opposes movement direction.

                bool normalTowardsMe = Vector3.Dot(displacement, surfaceNormal) < 0.0f;
                if (normalTowardsMe)
                    return false;

                // Also reject if we would end up being higher than our starting location by stepping down.

                if (Vector3.Dot(positionOnStep, _characterUp) > Vector3.Dot(inCollision.position, _characterUp))
                    return false;
            }

            // Reject moves where the downward sweep hit something very close to the edge of the capsule.
            // This maintains consistency with FindGround as well.

            if (!IsWithinEdgeTolerance(positionOnStep, hitResult.point, _radius + kContactOffset))
                return false;
            
            // Don't step up onto invalid surfaces if traveling higher.

            if (deltaY > 0.0f && !CanStepUp(hitResult.collider))
                return false;

            // Output new position on step.
            
            stepResult = new CollisionResult
            {
                position = positionOnStep
            };

            return true;
        }

        /// <summary>
        /// Temporarily disable ground constraint allowing the Character to freely leave the ground.
        /// Eg: LaunchCharacter, Jump, etc.
        /// </summary>

        public void PauseGroundConstraint(float unconstrainedTime = 0.1f)
        {
            _unconstrainedTimer = unconstrainedTime;
        }

        /// <summary>
        /// Updates current ground result.
        /// </summary>

        private void UpdateCurrentGround(ref FindGroundResult inGroundResult)
        {
            wasOnGround = isOnGround;

            wasOnWalkableGround = isOnWalkableGround;

            wasGrounded = isGrounded;

            _currentGround = inGroundResult;
        }

        /// <summary>
        /// Handle collisions of Character's bounding volume during a Move call.
        /// Unlike previous, this do not modifies / updates character's velocity.
        /// </summary>

        private int SlideAlongSurface(int iteration, Vector3 inputDisplacement, ref Vector3 displacement,
            ref CollisionResult inHit, ref Vector3 prevNormal)
        {
            inHit.normal = ComputeBlockingNormal(inHit.normal, inHit.isWalkable);

            if (inHit.isWalkable && isConstrainedToGround)
                displacement = ComputeSlideVector(displacement, inHit.normal, true);
            else
            {
                if (iteration == 0)
                {
                    displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    iteration++;
                }
                else if (iteration == 1)
                {
                    Vector3 crease = prevNormal.perpendicularTo(inHit.normal);

                    Vector3 oVel = inputDisplacement.projectedOnPlane(crease);

                    Vector3 nVel = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                            nVel = nVel.projectedOnPlane(crease);

                    if (oVel.dot(nVel) <= 0.0f || prevNormal.dot(inHit.normal) < 0.0f)
                    {
                        displacement = ConstrainVectorToPlane(displacement.projectedOn(crease));
                        ++iteration;
                    }
                    else
                    {
                        displacement = ComputeSlideVector(displacement, inHit.normal, inHit.isWalkable);
                    }
                }
                else
                {
                    displacement = Vector3.zero;
                }

                prevNormal = inHit.normal;
            }

            return iteration;
        }

        /// <summary>
        /// Perform collision constrained movement.
        /// This is exclusively used to move the character when standing on a moving platform as this will not update character's state.
        /// </summary>

        private void MoveAndSlide(Vector3 displacement)
        {
            //
            // Perform collision constrained movement (aka: collide and slide)

            Vector3 inputDisplacement = displacement;

            int iteration = default;
            Vector3 prevNormal = default;

            int maxSlideCount = _advanced.maxMovementIterations;
            while (maxSlideCount-- > 0 && displacement.sqrMagnitude > _advanced.minMoveDistanceSqr)
            {
                bool collided = MovementSweepTest(updatedPosition, default, displacement, out CollisionResult collisionResult);
                if (!collided)
                    break;

                // Apply displacement up to hit (near position) and update displacement with remaining displacement

                updatedPosition += collisionResult.displacementToHit;

                displacement = collisionResult.remainingDisplacement;

                //
                // Resolve collision (slide along hit surface)

                iteration = SlideAlongSurface(iteration, inputDisplacement, ref displacement, ref collisionResult, ref prevNormal);

                //
                // Cache collision result

                AddCollisionResult(ref collisionResult);
            }

            //
            // Apply remaining displacement

            if (displacement.sqrMagnitude > _advanced.minMoveDistanceSqr)
                updatedPosition += displacement;
        }

        /// <summary>
        /// Determines if the character is able to ride on (use it as moving platform) given collider.
        /// </summary>

        private bool CanRideOn(Collider otherCollider)
        {
            // Validate input collider

            if (otherCollider == null)
                return false;

            // If collision behavior callback assigned, use it

            if (collisionBehaviorCallback != null)
            {
                CollisionBehavior collisionBehavior = collisionBehaviorCallback.Invoke(otherCollider);

                if (CanRideOn(collisionBehavior) && otherCollider.attachedRigidbody)
                    return true;

                if (CanNotRideOn(collisionBehavior) && otherCollider.attachedRigidbody)
                    return false;
            }

            // Default, allow to ride on walkable rigidbodies (kinematic and dynamic)

            return otherCollider.attachedRigidbody;
        }

        /// <summary>
        /// Make collision detection ignore active platform collider(s).
        /// </summary>

        private void IgnoreCurrentPlatform(bool ignore)
        {
            IgnoreCollision(_movingPlatform.platform, ignore);
        }

        /// <summary>
        /// Allows you to explicitly attach this to a moving 'platform' so it no depends of ground state.
        /// </summary>

        public void SetPlatform(Rigidbody newPlatform)
        {
            _parentPlatform = newPlatform;
        }

        /// <summary>
        /// Update current active moving platform (if any).
        /// </summary>

        private void UpdateCurrentPlatform()
        {
            _movingPlatform.lastPlatform = _movingPlatform.platform;

            if (_parentPlatform)
                _movingPlatform.platform = _parentPlatform;
            else if (isGrounded && CanRideOn(groundCollider))
                _movingPlatform.platform = groundCollider.attachedRigidbody;
            else
                _movingPlatform.platform = null;

            if (_movingPlatform.platform)
            {
                Transform platformTransform = _movingPlatform.platform.transform;

                _movingPlatform.position = updatedPosition;
                _movingPlatform.localPosition = platformTransform.InverseTransformPoint(updatedPosition);

                _movingPlatform.rotation = updatedRotation;
                _movingPlatform.localRotation = Quaternion.Inverse(platformTransform.rotation) * updatedRotation;
            }
        }

        /// <summary>
        /// Update moving platform data and move /rotate character with it (if allowed).
        /// </summary>

        private void UpdatePlatformMovement(float deltaTime)
        {
            Vector3 lastPlatformVelocity = _movingPlatform.platformVelocity;

            if (!_movingPlatform.platform)
                _movingPlatform.platformVelocity = Vector3.zero;
            else
            {
                Transform platformTransform = _movingPlatform.platform.transform;

                Vector3 newPositionOnPlatform = platformTransform.TransformPoint(_movingPlatform.localPosition);
                Vector3 deltaPosition = newPositionOnPlatform - _movingPlatform.position;

                _movingPlatform.platformVelocity = deltaTime > 0.0f ? deltaPosition / deltaTime : Vector3.zero;

                if (impartPlatformRotation)
                {
                    Quaternion newRotationOnPlatform = platformTransform.rotation * _movingPlatform.localRotation;
                    Quaternion deltaRotation = newRotationOnPlatform * Quaternion.Inverse(_movingPlatform.rotation);

                    if (deltaRotation != Quaternion.identity)
                    {
                        Vector3 activePlatformAngularVelocity = deltaTime > 0.0f ? deltaRotation.eulerAngles * Mathf.Deg2Rad / deltaTime : Vector3.zero;
                        Quaternion yawRotation = Quaternion.Euler(Vector3.Project(activePlatformAngularVelocity, _characterUp) * (Mathf.Rad2Deg * deltaTime));

                        updatedRotation *= yawRotation;
                    }
                }
            }

            if (impartPlatformMovement && _movingPlatform.platformVelocity.sqrMagnitude > 0.0f)
            {
                if (fastPlatformMove)
                    updatedPosition += _movingPlatform.platformVelocity * deltaTime;
                else
                {
                    IgnoreCurrentPlatform(true);

                    MoveAndSlide(_movingPlatform.platformVelocity * deltaTime);

                    IgnoreCurrentPlatform(false);
                }
            }

            if (impartPlatformVelocity)
            {
                Vector3 impartedPlatformVelocity = Vector3.zero;

                if (_movingPlatform.lastPlatform && _movingPlatform.platform != _movingPlatform.lastPlatform)
                {
                    impartedPlatformVelocity -= _movingPlatform.platformVelocity;

                    impartedPlatformVelocity += lastPlatformVelocity;
                }

                if (_movingPlatform.lastPlatform == null && _movingPlatform.platform)
                {
                    impartedPlatformVelocity -= _movingPlatform.platformVelocity;
                }

                _velocity += impartedPlatformVelocity;
            }
        }

        /// <summary>
        /// Compute collision response impulses for character vs rigidbody or character vs character.
        /// </summary>

        private void ComputeDynamicCollisionResponse(ref CollisionResult inCollisionResult,
            out Vector3 characterImpulse, out Vector3 otherImpulse)
        {
            characterImpulse = default;
            otherImpulse = default;

            float massRatio = 0.0f;

            Rigidbody otherRigidbody = inCollisionResult.rigidbody;
            if (!otherRigidbody.isKinematic || otherRigidbody.TryGetComponent(out CharacterMovement _))
            {
                float mass = rigidbody.mass;
                massRatio = mass / (mass + inCollisionResult.rigidbody.mass);
            }

            Vector3 normal = inCollisionResult.normal;

            float velocityDotNormal = Vector3.Dot(inCollisionResult.velocity, normal);
            float otherVelocityDotNormal = Vector3.Dot(inCollisionResult.otherVelocity, normal);

            if (velocityDotNormal < 0.0f)
                characterImpulse += velocityDotNormal * normal;

            if (otherVelocityDotNormal > velocityDotNormal)
            {
                Vector3 relVel = (otherVelocityDotNormal - velocityDotNormal) * normal;

                characterImpulse += relVel * (1.0f - massRatio);
                otherImpulse -= relVel * massRatio;
            }
        }

        /// <summary>
        /// Compute and apply collision response impulses for dynamic collisions (eg: character vs rigidbodies or character vs other character).
        /// </summary>

        private void ResolveDynamicCollisions()
        {
            if (!enablePhysicsInteraction)
                return;

            for (int i = 0; i < _collisionCount; i++)
            {
                ref CollisionResult collisionResult = ref _collisionResults[i];
                if (collisionResult.isWalkable)
                    continue;

                Rigidbody otherRigidbody = collisionResult.rigidbody;
                if (otherRigidbody == null)
                    continue;

                ComputeDynamicCollisionResponse(ref collisionResult, out Vector3 characterImpulse, out Vector3 otherImpulse);

                collisionResponseCallback?.Invoke(ref collisionResult, ref characterImpulse, ref otherImpulse);

                if (otherRigidbody.TryGetComponent(out CharacterMovement otherCharacter))
                {
                    if (physicsInteractionAffectsCharacters)
                    {
                        velocity += characterImpulse;
                        otherCharacter.velocity += otherImpulse * pushForceScale;
                    }
                }
                else
                {
                    _velocity += characterImpulse;

                    if (!otherRigidbody.isKinematic)
                    {
                        otherRigidbody.AddForceAtPosition(otherImpulse * pushForceScale, collisionResult.point,
                            ForceMode.VelocityChange);
                    }
                }
            }

            if (isGrounded)
                _velocity = _velocity.projectedOnPlane(_characterUp).normalized * _velocity.magnitude;

            _velocity = ConstrainVectorToPlane(_velocity);
        }

        /// <summary>
        /// Update character current position.
        /// If updateGround is true, will find for ground and update character's current ground result.
        /// </summary>

        public void SetPosition(Vector3 newPosition, bool updateGround = false)
        {
            updatedPosition = newPosition;

            rigidbody.position = updatedPosition;
            transform.position = updatedPosition;

            if (!updateGround)
                return;
            
            FindGround(updatedPosition, out FindGroundResult groundResult);
            {
                UpdateCurrentGround(ref groundResult);
                
                AdjustGroundHeight();

                UpdateCurrentPlatform();
            }
        }

        /// <summary>
        /// Returns the character current position.
        /// </summary>

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        /// <summary>
        /// Returns the character' foot position accounting contact offset.
        /// </summary>

        public Vector3 GetFootPosition()
        {
            return transform.position - transform.up * kAvgGroundDistance;
        }

        /// <summary>
        /// Update character current rotation.
        /// </summary>

        public void SetRotation(Quaternion newRotation)
        {
            updatedRotation = newRotation;

            rigidbody.rotation = updatedRotation;
            transform.rotation = updatedRotation;
        }

        /// <summary>
        /// Returns the character current rotation.
        /// </summary>

        public Quaternion GetRotation()
        {
            return transform.rotation;
        }

        /// <summary>
        /// Sets the world space position and rotation of this character.
        /// If updateGround is true, will find for ground and update character's current ground result.
        /// </summary>

        public void SetPositionAndRotation(Vector3 newPosition, Quaternion newRotation, bool updateGround = false)
        {
            updatedPosition = newPosition;
            updatedRotation = newRotation;

            rigidbody.position = updatedPosition;
            rigidbody.rotation = updatedRotation;

            transform.SetPositionAndRotation(newPosition, newRotation);

            if (!updateGround)
                return;
            
            FindGround(updatedPosition, out FindGroundResult groundResult);
            {
                UpdateCurrentGround(ref groundResult);
                
                AdjustGroundHeight();

                UpdateCurrentPlatform();
            }
        }

        /// <summary>
        /// Orient the character's towards the given direction (in world space) using maxDegreesDelta as the rate of rotation change.
        /// </summary>
        /// <param name="worldDirection">The target direction in world space.</param>
        /// <param name="maxDegreesDelta">Change in rotation per second (Deg / s).</param>
        /// <param name="updateYawOnly">If True, the rotation will be performed on the Character's plane (defined by its up-axis).</param>

        public void RotateTowards(Vector3 worldDirection, float maxDegreesDelta, bool updateYawOnly = true)
        {
            Vector3 characterUp = transform.up;

            if (updateYawOnly)
                worldDirection = worldDirection.projectedOnPlane(characterUp);

            if (worldDirection == Vector3.zero)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection, characterUp);

            rotation = Quaternion.RotateTowards(rotation, targetRotation, maxDegreesDelta);
        }

        /// <summary>
        /// Update cached fields using during Move.
        /// </summary>

        private void UpdateCachedFields()
        {
            _hasLanded = false;
            _foundGround = default;

            updatedPosition = transform.position;
            updatedRotation = transform.rotation;

            _characterUp = updatedRotation * Vector3.up;

            _transformedCapsuleCenter = updatedRotation * _capsuleCenter;
            _transformedCapsuleTopCenter = updatedRotation * _capsuleTopCenter;
            _transformedCapsuleBottomCenter = updatedRotation * _capsuleBottomCenter;

            ResetCollisionFlags();
        }

        /// <summary>
        /// Clears any accumulated forces, including any pending launch velocity.
        /// </summary>

        public void ClearAccumulatedForces()
        {
            _pendingForces = Vector3.zero;
            _pendingImpulses = Vector3.zero;
            _pendingLaunchVelocity = Vector3.zero;
        }

        /// <summary>
        /// Adds a force to the Character.
        /// This forces will be accumulated and applied during Move method call.
        /// </summary>

        public void AddForce(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            switch (forceMode)
            {
                case ForceMode.Force:
                    {
                        _pendingForces += force / rigidbody.mass;
                        break;
                    }

                case ForceMode.Acceleration:
                    {
                        _pendingForces += force;
                        break;
                    }

                case ForceMode.Impulse:
                    {
                        _pendingImpulses += force / rigidbody.mass;
                        break;
                    }

                case ForceMode.VelocityChange:
                    {
                        _pendingImpulses += force;
                        break;
                    }
            }
        }

        /// <summary>
        /// Applies a force to a rigidbody that simulates explosion effects.
        /// The explosion is modeled as a sphere with a certain centre position and radius in world space;
        /// normally, anything outside the sphere is not affected by the explosion and the force decreases in proportion to distance from the centre.
        /// However, if a value of zero is passed for the radius then the full force will be applied regardless of how far the centre is from the rigidbody.
        /// </summary>

        public void AddExplosionForce(float forceMagnitude, Vector3 origin, float explosionRadius, ForceMode forceMode = ForceMode.Force)
        {
            // Do nothing if outside radius

            Vector3 delta = transform.TransformPoint(_capsuleCenter) - origin;

            if (delta.sqrMagnitude > forceMagnitude * forceMagnitude)
                return;

            if (explosionRadius > 0.0f)
                forceMagnitude *= 1.0f - delta.magnitude / explosionRadius;

            AddForce(delta.normalized * forceMagnitude, forceMode);
        }

        /// <summary>
        /// Set a pending launch velocity on the Character. This velocity will be processed next Move call.
        /// </summary>
        /// <param name="launchVelocity">The desired launch velocity.</param>
        /// <param name="overrideVerticalVelocity">If true replace the vertical component of the Character's velocity instead of adding to it.</param>
        /// <param name="overrideLateralVelocity">If true replace the XY part of the Character's velocity instead of adding to it.</param>

        public void LaunchCharacter(Vector3 launchVelocity, bool overrideVerticalVelocity = false, bool overrideLateralVelocity = false)
        {
            // Compute final velocity

            Vector3 finalVelocity = launchVelocity;

            // If not override, add lateral velocity to given launch velocity

            Vector3 characterUp = transform.up;

            if (!overrideLateralVelocity)
                finalVelocity += _velocity.projectedOnPlane(characterUp);

            // If not override, add vertical velocity to given launch velocity

            if (!overrideVerticalVelocity)
                finalVelocity += _velocity.projectedOn(characterUp);

            _pendingLaunchVelocity = finalVelocity;
        }

        /// <summary>
        /// Updates character's velocity, will apply and clear any pending forces and impulses.
        /// </summary>

        private void UpdateVelocity(Vector3 newVelocity, float deltaTime)
        {
            // Assign new velocity

            _velocity = newVelocity;
            
            // Add pending accumulated forces

            _velocity += _pendingForces * deltaTime;
            _velocity += _pendingImpulses;

            // Apply pending launch velocity

            if (_pendingLaunchVelocity.sqrMagnitude > 0.0f)
                _velocity = _pendingLaunchVelocity;
            
            // Clear accumulated forces

            ClearAccumulatedForces();

            // Apply plane constraint (if any)

            _velocity = ConstrainVectorToPlane(_velocity);
        }

        /// <summary>
        /// Moves the character along the given velocity vector.
        /// This performs collision constrained movement resolving any collisions / overlaps found during this movement.
        /// </summary>
        /// <param name="newVelocity">The updated velocity for current frame. It is typically a combination of vertical motion due to gravity and lateral motion when your character is moving.</param>
        /// <param name="deltaTime">The simulation deltaTime. If not assigned, it defaults to Time.deltaTime.</param>
        /// <returns>Return CollisionFlags. It indicates the direction of a collision: None, Sides, Above, and Below.</returns>
        
        public CollisionFlags Move(Vector3 newVelocity, float deltaTime = 0.0f)
        {
            if (deltaTime == 0.0f)
                deltaTime = Time.deltaTime;

            UpdateCachedFields();

            ClearCollisionResults();
            
            UpdateVelocity(newVelocity, deltaTime);

            UpdatePlatformMovement(deltaTime);

            PerformMovement(deltaTime);
            
            if (isGrounded || _hasLanded)
                FindGround(updatedPosition, out _foundGround);
            
            UpdateCurrentGround(ref _foundGround);
            {
                if (_unconstrainedTimer > 0.0f)
                {
                    _unconstrainedTimer -= deltaTime;
                    if (_unconstrainedTimer <= 0.0f)
                        _unconstrainedTimer = 0.0f;
                }
            }

            AdjustGroundHeight();

            UpdateCurrentPlatform();

            ResolveDynamicCollisions();
            
            SetPositionAndRotation(updatedPosition, updatedRotation);

            OnCollided();

            if (!wasOnWalkableGround && isOnGround)
                OnFoundGround();

            return collisionFlags;
        }

        /// <summary>
        /// Moves the character along its current velocity.
        /// This performs collision constrained movement resolving any collisions / overlaps found during this movement.
        /// </summary>
        /// <param name="deltaTime">The simulation deltaTime. If not assigned, it defaults to Time.deltaTime.</param>
        /// <returns>Return CollisionFlags. It indicates the direction of a collision: None, Sides, Above, and Below.</returns>

        public CollisionFlags Move(float deltaTime = 0.0f)
        {
            return Move(_velocity, deltaTime);
        }

        /// <summary>
        /// Update the character's velocity using a friction-based physical model and move the character along its updated velocity.
        /// This performs collision constrained movement resolving any collisions / overlaps found during this movement.
        /// </summary>
        /// <param name="desiredVelocity">Target velocity</param>
        /// <param name="maxSpeed">The maximum speed when grounded. Also determines maximum horizontal speed when falling (i.e. not-grounded).</param>
        /// <param name="acceleration">The rate of change of velocity when accelerating (i.e desiredVelocity != Vector3.zero).</param>
        /// <param name="deceleration">The rate at which the character slows down when braking (i.e. not accelerating or if character is exceeding max speed).
        /// This is a constant opposing force that directly lowers velocity by a constant value.</param>
        /// <param name="friction">Setting that affects movement control. Higher values allow faster changes in direction.</param>
        /// <param name="brakingFriction">Friction (drag) coefficient applied when braking (whenever desiredVelocity == Vector3.zero, or if character is exceeding max speed).</param>
        /// <param name="gravity">The current gravity force. Defaults to zero.</param>
        /// <param name="onlyHorizontal">Determines if the vertical velocity component should be ignored when falling (i.e. not-grounded) preserving gravity effects. Defaults to true.</param>
        /// <param name="deltaTime">The simulation deltaTime. Defaults to Time.deltaTime.</param>
        /// <returns>Return CollisionFlags. It indicates the direction of a collision: None, Sides, Above, and Below.</returns>

        public CollisionFlags SimpleMove(Vector3 desiredVelocity, float maxSpeed, float acceleration,
            float deceleration, float friction, float brakingFriction, Vector3 gravity = default,
            bool onlyHorizontal = true, float deltaTime = 0.0f)
        {
            if (deltaTime == 0.0f)
                deltaTime = Time.deltaTime;

            if (isGrounded)
            {
                // Calc new velocity

                velocity = CalcVelocity(velocity, desiredVelocity, maxSpeed, acceleration, deceleration, friction,
                    brakingFriction, deltaTime);
            }
            else
            {
                // Calc not grounded velocity

                Vector3 worldUp = -1.0f * gravity.normalized;
                Vector3 v = onlyHorizontal ? velocity.projectedOnPlane(worldUp) : velocity;

                if (onlyHorizontal)
                    desiredVelocity = desiredVelocity.projectedOnPlane(worldUp);

                // On not walkable ground ?

                if (isOnGround)
                {
                    // If moving into a 'wall', limit contribution.
                    // Allow movement parallel to the wall, but not into it because that may push us up.

                    Vector3 actualGroundNormal = groundNormal;
                    if (desiredVelocity.dot(actualGroundNormal) < 0.0f)
                    {
                        actualGroundNormal = actualGroundNormal.projectedOnPlane(worldUp).normalized;
                        desiredVelocity = desiredVelocity.projectedOnPlane(actualGroundNormal);
                    }
                }

                // Calc new velocity

                v = CalcVelocity(v, desiredVelocity, maxSpeed, acceleration, deceleration, friction, brakingFriction, deltaTime);

                // Update character's velocity

                if (onlyHorizontal)
                    velocity += Vector3.ProjectOnPlane(v - velocity, worldUp);
                else
                    velocity += v - velocity;

                // Apply gravity acceleration

                velocity += gravity * deltaTime;
            }

            // Perform the movement

            return Move(deltaTime);
        }

        /// <summary>
        /// Initialize CollisionLayers from GameObject's collision matrix.
        /// </summary>

        [ContextMenu("Init Collision Layers from Collision Matrix")]
        private void InitCollisionMask()
        {
            int layer = gameObject.layer;

            _collisionLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(layer, i))
                    _collisionLayers |= 1 << i;
            }
        }

        #endregion

        #region MONOBEHAVIOUR

        private void Reset()
        {
            SetDimensions(0.5f, 2.0f);
            SetPlaneConstraint(PlaneConstraint.None, Vector3.zero);

            _slopeLimit = 45.0f;
            _stepOffset = 0.45f;
            _perchOffset = 0.5f;
            _perchAdditionalHeight = 0.4f;
            
            _triggerInteraction = QueryTriggerInteraction.Ignore;

            _advanced.Reset();

            _isConstrainedToGround = true;

            _pushForceScale = 1.0f;
        }

        private void OnValidate()
        {
            SetDimensions(_radius, _height);
            SetPlaneConstraint(_planeConstraint, _constraintPlaneNormal);

            slopeLimit = _slopeLimit;
            stepOffset = _stepOffset;
            perchOffset = _perchOffset;
            perchAdditionalHeight = _perchAdditionalHeight;

            _advanced.OnValidate();
        }

        private void Awake()
        {
            CacheComponents();

            SetDimensions(_radius, _height);
            SetPlaneConstraint(_planeConstraint, _constraintPlaneNormal);
        }

        private void OnEnable()
        {
            updatedPosition = transform.position;
            updatedRotation = transform.rotation;
        }

#if UNITY_EDITOR

        public static void DrawDisc(Vector3 _pos, Quaternion _rot, float _radius, Color _color = default,
            bool solid = true)
        {
            if (_color != default)
                UnityEditor.Handles.color = _color;

            Matrix4x4 mtx = Matrix4x4.TRS(_pos, _rot, UnityEditor.Handles.matrix.lossyScale);

            using (new UnityEditor.Handles.DrawingScope(mtx))
            {
                if (solid)
                    UnityEditor.Handles.DrawSolidDisc(Vector3.zero, Vector3.up, _radius);
                else
                    UnityEditor.Handles.DrawWireDisc(Vector3.zero, Vector3.up, _radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw Foot position

            float skinRadius = _radius;
            Vector3 footPosition = GetFootPosition();

            Gizmos.color = new Color(0.569f, 0.957f, 0.545f, 0.5f);
            Gizmos.DrawLine(footPosition + Vector3.left * skinRadius, footPosition + Vector3.right * skinRadius);
            Gizmos.DrawLine(footPosition + Vector3.back * skinRadius, footPosition + Vector3.forward * skinRadius);

            // Draw perch offset radius

            if (perchOffset > 0.0f && perchOffset < radius)
            {
                DrawDisc(footPosition, rotation, _perchOffset, new Color(0.569f, 0.957f, 0.545f, 0.15f));
                DrawDisc(footPosition, rotation, _perchOffset, new Color(0.569f, 0.957f, 0.545f, 0.75f), false);
            }

            // Draw step Offset

            if (stepOffset > 0.0f)
            {
                DrawDisc(footPosition + transform.up * stepOffset, rotation, radius * 1.15f,
                    new Color(0.569f, 0.957f, 0.545f, 0.75f), false);
            }
        }

#endif

        #endregion
    }
}