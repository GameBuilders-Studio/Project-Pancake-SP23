using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeIngredient : IngredientProp
{
    [SerializeField]
    private Color _startColor;

    [SerializeField]
    private Color _endColor;

    private Renderer _renderer;

    protected override void Awake()
    {
        base.Awake();
        _renderer = GetComponent<Renderer>();
    }

    protected override void OnProgressUpdate(float progress)
    {
        _renderer.material.color = Vector4.Lerp(_startColor, _endColor, progress);
    }
}
