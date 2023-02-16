using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Handles the visual behaviour of ingredients
/// </summary>
public class IngredientStateBehaviour : MonoBehaviour
{
    [SerializeField]
    private IngredientStateData _state;

    [SerializeField]
    private Renderer _renderer;

    private float _progress;

    public IngredientStateData FoodState
    {
        get => _state;
        set => _state = value;
    }

    void Awake()
    {
        if (_renderer == null) 
        { 
            _renderer = GetComponent<Renderer>(); 
        }
        _progress = 0.0f;
    }

    public void Show()
    {
        _renderer.enabled = true;
    }

    public void HideAndReset()
    {
        _renderer.enabled = false;
        _progress = 0.0f;
    }

    public virtual void OnProgressUpdate(float progress)
    {
        _progress = progress;
        // handle specific animation behaviour here!
    }
}
