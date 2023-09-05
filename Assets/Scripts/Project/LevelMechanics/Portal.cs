using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
using System.Linq;

public class Portal : MonoBehaviour
{
    [SerializeField] Transform exitAtPoint;
    public Transform ExitAtPoint => exitAtPoint;

    [SerializeField] float exitVelocityMultiplier = 1f;

    [Required]
    [SerializeField] Portal exitPortal;
    
    [SerializeField] LayerMask teleportableLayers;

    // TODO: change to Dictionary<Collider, Portal> and remove collider when object exits the collider of the exit portal
    [SerializeField] float teleportCooldown = 0.5f;
    Dictionary<Collider, float> recentlyTeleportedToCooldownFinishedTimes = new Dictionary<Collider, float>();

    private float _cooldown = 0.5f;

    private void Update()
    {
        _cooldown -= Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Portal OnTriggerEnter");
        if (!ShouldTeleport(other)) { 
            Debug.Log("Portal: OnTriggerEnter: ShouldTeleport returned false");
            return; }
        float yOffset = other.ClosestPoint(transform.position).y - transform.position.y;
        Teleport(other, yOffset);
    }

    public void PutOnCooldown() {
        _cooldown = teleportCooldown;
    }

    private void Teleport(Collider other, float yOffset)
    {
        Debug.Log("Portal Teleport");
        Debug.Log("Collider: " + other.gameObject.name);
        // Changed to spawn at same relative height as entry portal
        float exitY = exitPortal.ExitAtPoint.position.y + yOffset;
        Vector3 exitPos = new Vector3(exitPortal.ExitAtPoint.position.x, exitY, exitPortal.ExitAtPoint.position.z);
        other.transform.position = exitPos;
        
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity *= exitVelocityMultiplier;
        }

        PutOnCooldown();
        exitPortal.PutOnCooldown();
    }

    private bool ShouldTeleport(Collider other)
    {
        // Return false if recently teleported
        // if (recentlyTeleportedToCooldownFinishedTimes.ContainsKey(other))
        // {
        //     if (Time.time < recentlyTeleportedToCooldownFinishedTimes[other])
        //     {
        //         Debug.Log("Portal: ShouldTeleport: recently teleported");
        //         return false;
        //     }
        //     else
        //     {
        //         Debug.Log("Portal: ShouldTeleport: recently teleported, but cooldown finished");
        //         recentlyTeleportedToCooldownFinishedTimes.Remove(other);
        //     }
        // }
        if(_cooldown > 0f)
        {
            return false;
        }
        
        // Return false if other layer not in teleportableLayers
        if (teleportableLayers != (teleportableLayers | (1 << other.gameObject.layer)))
        {
            Debug.Log("Portal: ShouldTeleport: other layer not in teleportableLayers");
            Debug.Log("GameObject name: " + other.gameObject.name);
            return false;
        }
        
        // return true;

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
