using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Selectable))]
public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool _interacting = false;

    public UnityEvent interactBeginEvent;

    public UnityEvent interactCancelEvent;

    public void Interact()
    {
        if (!_interacting) { OnInteractStart(); }
        _interacting = true;
    }

    public void CancelInteract()
    {
        if (_interacting) { OnInteractEnd(); }
        _interacting = false;
    }

    protected void OnInteractStart()
    {
        interactBeginEvent?.Invoke();
    }

    protected void OnInteractEnd()
    {
        interactCancelEvent?.Invoke();
    }
}