using UnityEngine;
using UnityEngine.Events;

public class ProxyTrigger : MonoBehaviour
{
    [SerializeField]
    private Collider _triggerCollider;

    public UnityAction<Collider> OnEnter;
    public UnityAction<Collider> OnExit;

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
        OnEnter?.Invoke(other);
    }

    void OnTriggerExit(Collider other)
    {
        OnExit?.Invoke(other);
    }

    /// <summary>
    /// Use in editor only!
    /// </summary>
    public static ProxyTrigger FindByName(GameObject parent, string objectName)
    {
        var triggers = parent.GetComponentsInChildren<ProxyTrigger>();
        foreach (var trigger in triggers)
        {
            if (trigger.gameObject.name != objectName) { continue; }
            return trigger;
        }
        return null;
    }
}
