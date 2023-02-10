using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProxyTriggerVolume : MonoBehaviour
{
    [SerializeField]
    private Collider _triggerCollider;

    public delegate void OnProxyEnter(Collider other);
    public OnProxyEnter OnEnter;

    public delegate void OnProxyExit(Collider other);
    public OnProxyExit OnExit;

    void Awake()
    {
        if (_triggerCollider == null)
            _triggerCollider = GetComponent<Collider>();
            
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
