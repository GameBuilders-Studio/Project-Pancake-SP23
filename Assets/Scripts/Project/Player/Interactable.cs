using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool _interacting = false;

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
        Debug.Log("interact begin!");
    }

    protected void OnInteractEnd()
    {
        Debug.Log("interact end");
    }
}
