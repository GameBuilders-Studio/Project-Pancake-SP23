using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Handles ingredient state
/// </summary>
public class Ingredient : MonoBehaviour
{
    [SerializeField]
    private List<IngredientStateBehaviour> _allowedFoodStates;

    [SerializeField]
    private IngredientStateData _currentState;

    private Dictionary<IngredientStateData, IngredientStateBehaviour> _stateBehaviours;

    private float _progress = 0.0f;

    public float Progress
    {
        get => _progress;
        set => _progress = value;
    }

    void Awake()
    {
        // associate each IngredientStateData with an IngredientStateBehaviour
        foreach (IngredientStateBehaviour behaviour in _allowedFoodStates)
        {
            _stateBehaviours.Add(behaviour.FoodState, behaviour);
        }
    }

    public bool CanEnterState(IngredientStateData foodState)
    {
        return _stateBehaviours.ContainsKey(foodState);
    }

    public void SetState(IngredientStateData foodState)
    {
        if (!CanEnterState(foodState))
        {
            Debug.LogError("Food can not enter state " + foodState.name);
        }

        var currentStateBehaviour = _stateBehaviours[_currentState];
        if (currentStateBehaviour != null)
        {
            currentStateBehaviour.HideAndReset();
        }
        
        _currentState = foodState;

        var newStateBehaviour = _stateBehaviours[foodState];
        if (newStateBehaviour != null)
        {
            newStateBehaviour.Show();
        }
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _progress = progress;

        _stateBehaviours[_currentState].OnProgressUpdate(_progress);
    }
}