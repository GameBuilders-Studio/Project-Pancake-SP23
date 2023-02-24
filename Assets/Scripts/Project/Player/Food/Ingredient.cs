using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Handles ingredient state
/// </summary>
public class Ingredient : MonoBehaviour
{
    [SerializeField]
    private IngredientStateData _currentState;

    private IngredientStateBehaviour[] _allowedFoodStates;

    private Dictionary<IngredientStateData, IngredientStateBehaviour> _stateBehaviours;

    private float _progress = 0.0f;

    public float Progress
    {
        get => _progress;
        set => _progress = value;
    }

    public bool ProgressComplete
    {
        get => Mathf.Approximately(_progress, 1.0f);
    }

    void Start()
    {
        _allowedFoodStates = GetComponentsInChildren<IngredientStateBehaviour>();

        _stateBehaviours = new();

        // associate each IngredientStateData with an IngredientStateBehaviour
        foreach (IngredientStateBehaviour behaviour in _allowedFoodStates)
        {
            _stateBehaviours.Add(behaviour.FoodState, behaviour);
        }

        if (_currentState != null)
        {
            SetState(_currentState);
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
            Debug.LogError("Food cannot enter state " + foodState.name);
        }

        var currentStateBehaviour = _stateBehaviours[_currentState];
        if (currentStateBehaviour != null)
        {
            currentStateBehaviour.OnTransitionExit();
        }
        
        _currentState = foodState;

        var newStateBehaviour = _stateBehaviours[foodState];
        if (newStateBehaviour != null)
        {
            newStateBehaviour.OnTransitionEnter();
        }
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _progress = progress;

        _stateBehaviours[_currentState].UpdateProgress(_progress);
    }

    public void AddProgress(float progressDelta)
    {
        SetProgress(Progress + progressDelta);
    }
}