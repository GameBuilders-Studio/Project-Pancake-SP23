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
    private bool _isBeingCarried = false;

    private float _currentThrowTime = 0.0f;
    private float _throwHeight;
    private Vector3 _throwDirection;

    private const int MaxDepenetrationIterations = 3;

    public bool CanThrow
    {
        get => IsEverThrowable && _throwSettings != null;
    }

    public virtual bool IsEverThrowable 
    {
        get => true;
    }

    public bool IsCatchable
    {
        get => _isFlying && !_isBeingCarried;
    }

    protected override void OnAwake()
    {
        EnablePhysics();
    }
    
    void FixedUpdate()
    {
        var gravity = Physics.gravity * _gravityScale;
        Rigidbody.AddForce(gravity, ForceMode.Acceleration);

        if (!_isFlying) { return; }
        
        ThrowUpdate();
    }

    void OnCollisionStay(Collision collision)
    {
        var firstContact = collision.contacts[0];
        if (_isFlying)
        {
            CancelThrow();
            Debug.DrawRay(firstContact.point, firstContact.normal, Color.green, 3.0f);
        }
    }

    public void OnPickUp()
    {
        _isBeingCarried = true;
        _isFlying = false;
        SetState(SelectableState.Disabled);
        DisablePhysics();
    }

    public void OnPlace()
    {
        _isBeingCarried = false;
        SetState(SelectableState.Disabled);
        DisablePhysics();
    }

    public void OnDrop()
    {
        _isBeingCarried = false;
        SetState(SelectableState.Default);
        EnablePhysics();
    }

    public void OnThrow(Vector3 direction, float footHeight)
    {
        _isFlying = true;
        _isBeingCarried = false;
        transform.parent = null;

        _throwDirection = direction;
        _throwHeight = transform.position.y - footHeight;

        var throwTarget = transform.position + _throwDirection * _throwSettings.Distance;
        throwTarget.y = footHeight;
        Debug.DrawRay(throwTarget, Vector3.up, Color.red, 3.0f);
        
        Rigidbody.drag = 0.0f;
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

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
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void DisablePhysics()
    {
        Rigidbody.isKinematic = true;
        Rigidbody.interpolation = RigidbodyInterpolation.None;
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

        Debug.DrawRay(Rigidbody.position, Vector3.up * 0.5f, Color.blue, 3.0f);

        Rigidbody.velocity = velocity / Time.deltaTime;
    
        if (Mathf.Approximately(newProgress, 1.0f))
        {
            CancelThrow();
        }
    }
}
