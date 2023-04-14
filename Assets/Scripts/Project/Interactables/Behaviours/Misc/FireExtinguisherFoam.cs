using UnityEngine;
using CustomAttributes;

public class FireExtinguisherFoam : MonoBehaviour
{
    [SerializeField]
    private float _fireDamage = 10.0f;

    [SerializeField]
    private float _travelDistance = 3.0f;

    [SerializeField]
    private float _travelDuration = 0.6f;

    [SerializeField]
    [CurveRange(0f, 0f, 1f, 1f)]
    private AnimationCurve _speedCurve;

    [Header("Dependencies")]
    [SerializeField]
    [ReadOnly, Required]
    private Rigidbody _rigidbody;

    [SerializeField]
    [ReadOnly, Required]
    private Collider _collider;

    private float _travelTime = 0.0f;
    private float _travelProgress = 0.0f;

    void OnValidate()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        _collider.isTrigger = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    void FixedUpdate()
    {
        // TODO: object pooling
        if (_travelTime > _travelDuration)
        {
            Destroy(gameObject);
        }

        _travelTime += Time.deltaTime;

        float previousTravelProgress = _travelProgress;
        _travelProgress = _speedCurve.Evaluate(_travelTime / _travelDuration);

        var velocity = transform.forward * (_travelProgress - previousTravelProgress) * _travelDistance;

        _rigidbody.MovePosition(_rigidbody.position + velocity);
    }

    void OnTriggerEnter(Collider other)
    {
        if (Flammable.TryGetFlammable(other.gameObject, out Flammable flammable))
        {
            flammable.DamageFire(_fireDamage);
        }
    }
}
