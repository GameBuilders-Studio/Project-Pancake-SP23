using UnityEngine;
using System.Collections.Generic;
using CustomAttributes;

[RequireComponent(typeof(Rigidbody))]
public class ConveyorFloor : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    [SerializeField]
    private bool _enabled = true;

    [SerializeField]
    [Required]
    private Rigidbody _rigidbody;

    private static readonly Dictionary<GameObject, ConveyorFloor> s_instances = new();

    void OnValidate()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.isKinematic = true;
    }

    void OnEnable()
    {
        s_instances.Add(gameObject, this);
    }

    void OnDisable()
    {
        s_instances.Remove(gameObject);
    }

    public static bool TryGetConveyor(GameObject go, out ConveyorFloor conveyorFloor)
    {
        return s_instances.TryGetValue(go, out conveyorFloor);
    }

    void FixedUpdate()
    {
        if (!_enabled) { return; }

        var currentPos = _rigidbody.position;

        _rigidbody.position += -transform.forward * _speed * Time.deltaTime;
        
        _rigidbody.MovePosition(currentPos);
    }

    public Vector3 GetMovementVector()
    {
        return transform.forward * _speed;
    }
}
