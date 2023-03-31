using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform exitAtPoint;
    public Transform ExitAtPoint => exitAtPoint;

    [Required]
    [SerializeField] Portal exitPortal;
    
    [SerializeField] float exitSpeedMultiplier = 1f;

    [SerializeField] LayerMask teleportableLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (!ShouldTeleport(other)) { return; }
        Vector3 exitPos = new Vector3(exitPortal.ExitAtPoint.position.x, other.transform.position.y, exitPortal.ExitAtPoint.position.z);
        other.transform.position = exitPos;
    }

    private bool ShouldTeleport(Collider other)
    {
        // Return false if other layer not in teleportableLayers
        if (teleportableLayers != (teleportableLayers | (1 << other.gameObject.layer)))
        {
            return false;
        }

        // Return true iff parent is null
        Debug.Log("has parent");
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
