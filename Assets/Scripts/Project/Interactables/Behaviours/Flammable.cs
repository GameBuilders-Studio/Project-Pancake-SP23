using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class Flammable : InteractionBehaviour
{
    [SerializeField]
    [Required]
    private FlammableData _settings;

    [SerializeField]
    [Required, Tooltip("The transform to spawn the fire prefab at.")]
    private Transform _firePivot;

    [SerializeField]
    private bool _isBurning = false;

    [SerializeField]
    [ReadOnly]
    private float _spreadTimer = 0.0f;

    [SerializeField]
    [ReadOnly, ProgressBar("Fire Health", "_maxFireHealth", EColor.Orange)]
    private float _fireHealth = 0.0f;

    [Header("Dependencies")]
    [SerializeField]
    [ReadOnly, Required]
    private Collider _collider;

    public static readonly Dictionary<GameObject, Flammable> Instances = new();

    private float _maxFireHealth = 1.0f;
    private GameObject _fireEffect;
    private Collider[] _overlapResults = new Collider[32];

    public bool IsBurning => _isBurning;

    void OnValidate()
    {
        _collider = GetComponent<Collider>();
    }

    void OnEnable()
    {
        Instances.Add(gameObject, this);
    }

    void OnDisable()
    {
        Instances.Remove(gameObject);
        if (_isBurning) { Destroy(_fireEffect); }
    }

    void Start()
    {
        _maxFireHealth = _settings.FireInitialHealth;
        if (_isBurning) { Ignite(); }
    }

    void Update()
    {
        if (!_isBurning) { return; }

        _spreadTimer -= Time.deltaTime;

        if (_spreadTimer <= 0.0f)
        {
            IgniteNeighbors();
            _spreadTimer = _settings.SpreadIntervalSeconds;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_settings == null) { return; }
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _settings.SpreadRadius);
    }

    public bool TryIgnite()
    {
        if (_isBurning) 
        {
            Ignite();
            return true; 
        }
        return false;
    }

    public void IgniteNeighbors()
    {
        // TODO: use layermask to filter non-flammable results
        int neighborCount = Physics.OverlapSphereNonAlloc(transform.position, _settings.SpreadRadius, _overlapResults);

        for (int i = 0; i < neighborCount; i++)
        {
            var collider = _overlapResults[i];

            if (collider == _collider) { continue; }

            if (Instances.TryGetValue(collider.gameObject, out Flammable flammable))
            {
                flammable.TryIgnite();
            }
        }
    }

    public void Extinguish(float fireDamage)
    {
        if (!_isBurning) { return; }

        _fireHealth -= fireDamage;

        if (_fireHealth < 0.0f)
        {
            _fireHealth = 0.0f;
            _spreadTimer = 0.0f;
            _isBurning = false;
            Destroy(_fireEffect);
        }
    }

    private void Ignite()
    {
        _isBurning = true;
        _spreadTimer = _settings.SpreadIntervalSeconds;
        _fireHealth = _settings.FireInitialHealth;
        _fireEffect = Instantiate(_settings.FirePrefab, _firePivot.position, Quaternion.identity);
    }
}
