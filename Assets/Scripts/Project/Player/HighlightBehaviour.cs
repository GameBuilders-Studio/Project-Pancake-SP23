using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HighlightBehaviour : MonoBehaviour
{
    [SerializeField]
    private Renderer _renderer;

    [SerializeField]
    private Material _selectMaterial;

    protected Material _material;

    void Awake()
    {
        if (_renderer == null)
        {
            _renderer = GetComponent<Renderer>();
        }
        _material = _renderer.material;
    }

    public void EnableHighlight(bool enabled = true)
    {
        if (enabled)
        {
            _renderer.material = _selectMaterial;
        }
        else
        {
            _renderer.material = _material;
        }
    }
}
