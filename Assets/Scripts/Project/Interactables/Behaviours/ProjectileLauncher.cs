using UnityEngine;
using CustomAttributes;

public class ProjectileLauncher : InteractionBehaviour, IUsable
{
    [SerializeField]
    [Required]
    private GameObject _projectile;

    [SerializeField]
    [Required]
    private Transform _projectileSpawnPoint;

    [SerializeField]
    private bool _canRepeatFire = true;

    [SerializeField]
    [ShowIf("_canRepeatFire")]
    private float _repeatFireIntervalSeconds = 0.3f;

    private float _fireRepeatTimer = 0.0f;
    private bool _firing = false;

    bool IUsable.Enabled => true;

    void Update()
    {
        if (!_firing) { return; }

        if (_fireRepeatTimer > _repeatFireIntervalSeconds)
        {
            _fireRepeatTimer -= _repeatFireIntervalSeconds;
            LaunchProjectile();
        }

        if (!_canRepeatFire) { return; }

        _fireRepeatTimer += Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        if (_projectileSpawnPoint == null) { return; }

        Gizmos.DrawSphere(_projectileSpawnPoint.position, 0.1f * transform.localScale.x);

        var from = _projectileSpawnPoint.position;
        var to = _projectileSpawnPoint.position + _projectileSpawnPoint.forward;
        CustomGizmos.DrawArrow(from, to);
    }

    public void OnUseStart()
    {
        _firing = true;
        _fireRepeatTimer = _repeatFireIntervalSeconds;
    }

    public void OnUseEnd()
    {
        _firing = false;
    }

    protected virtual void LaunchProjectile()
    {
        Instantiate(_projectile, _projectileSpawnPoint.position, Quaternion.LookRotation(_projectileSpawnPoint.forward));
    }
}
