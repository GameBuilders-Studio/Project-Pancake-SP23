using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carryable : Selectable
{
    [Space(15f)]
    [SerializeField]
    private ThrowData _throwSettings;

    [SerializeField]
    private float _gravityScale = 2.5f;

    private bool _isFlying = false;

    private float _currentThrowTime = 0.0f;

    private Vector3 _throwDirection;
    private float _throwHeight;
    private Collider _ignoreCollider;

    public bool CanThrow
    {
        get => IsEverThrowable && _throwSettings != null;
    }

    public virtual bool IsEverThrowable 
    {
        get => true; 
    }

    protected override void OnAwake()
    {
        EnablePhysics();
    }
    
    void FixedUpdate()
    {
        var gravity = Physics.gravity * _gravityScale;
        _rigidbody.AddForce(gravity, ForceMode.Acceleration);

        if (!_isFlying) { return; }
        
        ThrowUpdate();
    }

    void OnCollisionStay(Collision collision)
    {
        var col = collision.contacts[0];
        if (_isFlying && col.otherCollider != _ignoreCollider)
        {
            CancelThrow();
            Debug.DrawRay(col.point, col.normal, Color.green, 3.0f);
        }
    }

    public void OnPickUp()
    {
        SetState(SelectableState.Disabled);
        DisablePhysics();
    }

    public void OnPlace()
    {
        SetState(SelectableState.Disabled);
        DisablePhysics();
    }

    public void OnDrop()
    {
        SetState(SelectableState.Default);
        EnablePhysics();
    }

    public void Throw(Vector3 direction, float footHeight, Collider ignoreCollider)
    {
        _isFlying = true;

        _throwDirection = direction;
        _throwHeight = transform.position.y - footHeight;
        _ignoreCollider = ignoreCollider;

        var throwTarget = transform.position + _throwDirection * _throwSettings.Distance;
        throwTarget.y = footHeight;
        Debug.DrawRay(throwTarget, Vector3.up, Color.red, 3.0f);

        transform.parent = null;
        
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        _rigidbody.MovePosition(transform.position);

        SetState(SelectableState.Disabled);

        _renderer.material.color = Color.red;
    }

    public void CancelThrow()
    {
        _isFlying = false;
        SetState(SelectableState.Default);
        EnablePhysics();
        _currentThrowTime = 0.0f;

        _renderer.material.color = Color.cyan;
    }
    
    void EnablePhysics()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void DisablePhysics()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
    }

    void ThrowUpdate()
    {
        float throwDuration = _throwSettings.ThrowDurationSeconds;
        AnimationCurve trajectory = _throwSettings.Trajectory;

        float oldProgress = _currentThrowTime / throwDuration;
        _currentThrowTime += Time.deltaTime;
        _currentThrowTime = Mathf.Clamp(_currentThrowTime, 0.0f, throwDuration);
        float newProgress = _currentThrowTime / throwDuration;
        
        float horizontalDelta = _throwSettings.Distance * (newProgress - oldProgress);
        float verticalDelta = _throwHeight * (trajectory.Evaluate(newProgress) - trajectory.Evaluate(oldProgress));
        Vector3 velocity = (_throwDirection * horizontalDelta) + (Vector3.up * verticalDelta);

        Debug.DrawRay(_rigidbody.position, Vector3.up * 0.5f, Color.blue, 3.0f);

        _rigidbody.MovePosition(transform.position + velocity);

        if (Mathf.Approximately(newProgress, 1.0f) && !Mathf.Approximately(newProgress, oldProgress))
        {
            CancelThrow();
        }
    }
}
