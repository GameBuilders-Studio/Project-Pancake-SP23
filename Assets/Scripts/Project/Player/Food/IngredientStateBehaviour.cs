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

    private float _progress;

    public IngredientStateData FoodState
    {
        get => _state;
        set => _state = value;
    }
    
    public void OnTransitionEnter() {}

    public void OnTransitionExit() {}

    public void UpdateProgress(float progress)
    {
        _progress = progress;
        OnProgressUpdate(progress);
    }

    public virtual void OnProgressUpdate(float progress)
    {
        // handle specific animation behaviour here!
    }
}
