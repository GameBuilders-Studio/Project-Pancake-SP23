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

    void OnTriggerEnter(Collider other)
    {
        OnEnter(other);
    }

    void OnTriggerExit(Collider other)
    {
        OnExit(other);
    }
}
