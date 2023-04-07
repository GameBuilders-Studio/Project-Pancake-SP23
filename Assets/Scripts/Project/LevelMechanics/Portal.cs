using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
using System.Linq;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform exitAtPoint;
    public Transform ExitAtPoint => exitAtPoint;

    [SerializeField] bool exitAtSameY = true;

    [Required]
    [SerializeField] Portal exitPortal;
    
    [SerializeField] LayerMask teleportableLayers;

    [SerializeField] float teleportCooldown = 0.5f;
    Dictionary<Collider, float> recentlyTeleportedToCooldownFinishedTimes = new Dictionary<Collider, float>();

    void Update()
    {
        recentlyTeleportedToCooldownFinishedTimes = recentlyTeleportedToCooldownFinishedTimes
            .Where(kvp => kvp.Value > Time.time)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ShouldTeleport(other)) { return; }
        Teleport(other);
    }

    private void Teleport(Collider other)
    {
        float exitY = exitAtSameY ? other.transform.position.y : exitPortal.ExitAtPoint.position.y;
        Vector3 exitPos = new Vector3(exitPortal.ExitAtPoint.position.x, exitY, exitPortal.ExitAtPoint.position.z);
        other.transform.position = exitPos;
        recentlyTeleportedToCooldownFinishedTimes.Add(other, Time.time + teleportCooldown);
    }

    private bool ShouldTeleport(Collider other)
    {
        // Return false if recently teleported
        if (recentlyTeleportedToCooldownFinishedTimes.ContainsKey(other))
        {
            return false;
        }
        
        // Return false if other layer not in teleportableLayers
        if (teleportableLayers != (teleportableLayers | (1 << other.gameObject.layer)))
        {
            return false;
        }

        // Return true iff parent is null
        return other.transform.parent == null; 
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (exitPortal != null)
        {
            Gizmos.DrawLine(transform.position, exitPortal.transform.position);
        }
    }
}
