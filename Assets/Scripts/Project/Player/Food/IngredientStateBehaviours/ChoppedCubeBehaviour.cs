using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppedCubeBehaviour : IngredientStateBehaviour
{
    private Renderer _renderer;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public override void OnProgressUpdate(float progress)
    {
        Debug.Log("progress updated on cube");
        _renderer.material.color = Vector4.Lerp(Color.white, Color.black, progress);
    }
}
