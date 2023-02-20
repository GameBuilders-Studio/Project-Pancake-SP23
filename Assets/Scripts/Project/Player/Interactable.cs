using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[RequireComponent(typeof(Selectable))]
[RequireComponent(typeof(Rigidbody))]
public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool _interacting = false;
    public UnityEvent interactBeginEvent;
    public UnityEvent interactCancelEvent;

    public void Interact()
    {
        if (!_interacting) { OnInteractBegin(); }
        _interacting = true;
    }

    public void CancelInteract()
    {
        if (_interacting) { OnInteractEnd(); }
        _interacting = false;
    }

    protected void OnInteractBegin()
    {
        //Debug.Log("interact begin!");
        interactBeginEvent?.Invoke();
    }

    protected void OnInteractEnd()
    {
        //Debug.Log("interact end");
        interactCancelEvent?.Invoke();
    }
}
