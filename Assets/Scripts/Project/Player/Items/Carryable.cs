using System;
using UnityEngine;
using System.Collections.Generic;
using CustomAttributes;

[RequireComponent(typeof(Rigidbody), typeof(Selectable))]
public class Carryable : InteractionBehaviour, IHasCarryable
{
    [SerializeField]
    private ThrowData _throwSettings;

    [SerializeField]
    private float _gravityScale = 2.5f;

    [SerializeField]
    [ReadOnly]
    private Selectable _selectable;

    [HideInInspector] private Rigidbody _rigidbody;
    [HideInInspector] private Collider _collider;

    private bool _isFlying = false;

    private List<Collider> _ignoredColliders = new();

    private float _currentThrowTime = 0.0f;
    private float _throwHeight;
    private Vector3 _throwDirection;

    public Rigidbody Rigidbody => _rigidbody;

    public bool CanThrow => _throwSettings != null && _throwSettings.IsThrowable;

    public bool IsFlying => _isFlying;
    public bool PhysicsEnabled => !_rigidbody.isKinematic;

    void OnValidate()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _selectable = GetComponent<Selectable>();
        _collider = GetComponent<Collider>();
    }

    void Awake()
    {
        EnablePhysicsAndSelection();
    }
    
    void FixedUpdate()
    {
        var gravity = Physics.gravity * _gravityScale;
        Rigidbody.AddForce(gravity, ForceMode.Acceleration);

        if (!_isFlying) { return; }
        ThrowUpdate();
    }

    void OnCollisionStay()
    {
        if (!_isFlying) { return; }
        CancelThrow();
    }

    public void OnPickUp()
    {
        _isFlying = false;
        DisablePhysicsAndSelection();
    }

    public void OnPlace()
    {
        DisablePhysicsAndSelection();
    }

    public void OnDrop()
    {
        EnablePhysicsAndSelection();
    }

    public void OnThrow(Vector3 direction, float footHeight, Collider colliderToIgnore = null)
    {
        _isFlying = true;
        transform.parent = null;

        _throwDirection = direction;
        _throwHeight = transform.position.y - footHeight;

        var throwTarget = transform.position + _throwDirection * _throwSettings.Distance;
        throwTarget.y = footHeight;
        Debug.DrawRay(throwTarget, Vector3.up, Color.red, 3.0f);
        
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        if (colliderToIgnore)
        {
            IgnoreCollision(colliderToIgnore, ignore: true);
        }

        _selectable.SetSelectState(SelectState.Disabled);
    }

    public void CancelThrow()
    {
        _isFlying = false;
        _currentThrowTime = 0.0f;
        EnablePhysicsAndSelection();
        ClearIgnoredCollisions();
    }

    public void IgnoreCollision(Collider other, bool ignore = true)
    {
        Physics.IgnoreCollision(other, _collider, ignore);
        _ignoredColliders.Add(other);
    }

    private void ClearIgnoredCollisions()
    {
        foreach (Collider collider in _ignoredColliders)
        {
            Physics.IgnoreCollision(collider, _collider, ignore: false);
        }
        _ignoredColliders.Clear();
    }
    
    private void EnablePhysicsAndSelection()
    {
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _selectable.SetSelectState(SelectState.Default);
    }

    private void DisablePhysicsAndSelection()
    {
        Rigidbody.isKinematic = true;
        Rigidbody.interpolation = RigidbodyInterpolation.None;
        _selectable.SetSelectState(SelectState.Disabled);
    }

    private void ThrowUpdate()
    {
        float throwDuration = _throwSettings.ThrowDurationSeconds;
        AnimationCurve trajectory = _throwSettings.Trajectory;

        float previousProgress = _currentThrowTime / throwDuration;

        _currentThrowTime += Time.deltaTime;
        _currentThrowTime = Mathf.Clamp(_currentThrowTime, 0.0f, throwDuration);

        float currentProgress = _currentThrowTime / throwDuration;
        
        float horizontalDelta = _throwSettings.Distance * (currentProgress - previousProgress);
        float verticalDelta = _throwHeight * (trajectory.Evaluate(currentProgress) - trajectory.Evaluate(previousProgress));
        
        Vector3 velocity = (_throwDirection * horizontalDelta) + (Vector3.up * verticalDelta);

        Rigidbody.velocity = velocity / Time.deltaTime;
    
        if (Mathf.Approximately(currentProgress, 1.0f))
        {
            CancelThrow();
        }

        Debug.DrawRay(Rigidbody.position, Vector3.up * 0.5f, Color.blue, 3.0f);
    }

    public Carryable PopCarryable()
    {
        return this;
    }
}
