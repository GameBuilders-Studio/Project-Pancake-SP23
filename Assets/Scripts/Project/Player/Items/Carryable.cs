using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carryable : Selectable
{
    public bool IsCarryable { get; set; }

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
    
    void EnablePhysics()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.detectCollisions = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        SetState(SelectableState.Default);
    }

    void DisablePhysics()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
    }
}
