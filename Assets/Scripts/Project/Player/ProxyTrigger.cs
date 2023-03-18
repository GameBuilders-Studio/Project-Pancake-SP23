using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class ProxyTrigger : MonoBehaviour
{
    [SerializeField]
    private Collider _triggerCollider;

    private List<Rigidbody> _ignoredRigidbodies = new();

    public Collider Collider => _triggerCollider;

    public UnityAction<Collider> Enter;
    public UnityAction<Collider> Exit;

    void Awake()
    {
        if (_triggerCollider == null)
        {
            _triggerCollider = GetComponent<Collider>();
        }
        _triggerCollider.isTrigger = true;
    }

    void OnValidate()
    {
        Awake();
    }

    void OnTriggerEnter(Collider other)
    {
        if (ShouldFilter(other.attachedRigidbody)) { return; }
        Enter?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (ShouldFilter(other.attachedRigidbody)) { return; }
        Exit?.Invoke(other);
    }

    public void IgnoreCollision(Rigidbody otherRigidbody, bool ignore = true)
    {
        if (ignore)
        {
            _ignoredRigidbodies.Add(otherRigidbody);
        }
        else
        {
            _ignoredRigidbodies.Remove(otherRigidbody);
        }
    }

    public void ClearIgnoredCollisions()
    {
        _ignoredRigidbodies.Clear();
    }

    /// <summary>
    /// Use in editor only!
    /// </summary>
    public static ProxyTrigger FindByName(GameObject parent, string objectName, bool emitWarning = true)
    {
        var triggers = parent.GetComponentsInChildren<ProxyTrigger>();
        foreach (var trigger in triggers)
        {
            if (trigger.gameObject.name != objectName) { continue; }
            return trigger;
        }

        if (emitWarning)
        {
            Debug.LogWarning($"ProxyTrigger with name {objectName} not found under {parent.name}", parent);
        }
        
        return null;
    }

    private bool ShouldFilter(Rigidbody rigidbody)
    {
        return _ignoredRigidbodies.Contains(rigidbody);
    }
}
