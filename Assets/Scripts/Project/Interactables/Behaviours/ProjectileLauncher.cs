using UnityEngine;
using CustomAttributes;

public class ProjectileLauncher : InteractionBehaviour, IUsableWhileCarried
{
    [SerializeField]
    [Tooltip("The prefab projectile to launch.")]
    [Required]
    private GameObject _projectile;

    [SerializeField]
    [Tooltip("The transform to spawn the projectile at.")]
    [Required]
    private Transform _projectileSpawnPoint;

    [SerializeField]
    [Tooltip("Whether the launcher should fire automatically.")]
    private bool _canRepeatFire = true;

    [SerializeField]
    [Tooltip("The minimum time between firing each projectile.")]
    private float _fireIntervalSeconds = 0.1f;

    private float _fireRepeatTimer = 0.0f;
    private bool _firing = false;

    public bool Enabled => true;

    void Update()
    {
        _fireRepeatTimer -= Time.deltaTime;

        if (!_firing)
        {
            _fireRepeatTimer = Mathf.Max(_fireRepeatTimer, 0.0f);
            return;
        }

        if (_fireRepeatTimer < Mathf.Epsilon)
        {
            if (!_canRepeatFire) { _firing = false; }
            _fireRepeatTimer += _fireIntervalSeconds;
            LaunchProjectile();
        }
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
