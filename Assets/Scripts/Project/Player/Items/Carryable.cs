using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Carryable : Selectable
{
    [Space(15f)]
    [SerializeField]
    private ThrowData _throwSettings;

    [SerializeField]
    private float _gravityScale = 2.5f;

    protected Rigidbody _rigidbody;

    private bool _isFlying = false;
    private bool _isBeingCarried = false;

    private float _currentThrowTime = 0.0f;
    private float _throwHeight;
    private Vector3 _throwDirection;

    public Rigidbody Rigidbody => _rigidbody;

    public virtual bool IsEverThrowable => true;
    public bool CanThrow => IsEverThrowable && _throwSettings != null;
    
    public bool IsBeingCarried => _isBeingCarried;
    public bool IsFlying => _isFlying;
    public bool PhysicsEnabled => !_rigidbody.isKinematic;

    protected override void Awake()
    {
        base.Awake();
        _rigidbody = GetComponent<Rigidbody>();
        EnablePhysics();
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
        _isBeingCarried = true;
        _isFlying = false;
        SetSelectState(SelectState.Disabled);
        DisablePhysics();
    }

    public void OnPlace()
    {
        _isBeingCarried = false;
        SetSelectState(SelectState.Disabled);
        DisablePhysics();
    }

    public void OnDrop()
    {
        _isBeingCarried = false;
        SetSelectState(SelectState.Default);
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
        
        Rigidbody.isKinematic = false;
        Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

        SetSelectState(SelectState.Disabled);
    }

    public void CancelThrow()
    {
        _isFlying = false;
        _currentThrowTime = 0.0f;
        SetSelectState(SelectState.Default);
        EnablePhysics();
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
}
