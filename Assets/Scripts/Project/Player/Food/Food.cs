using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField]
    private List<FoodStateBehaviour> _allowedFoodStates;

    [SerializeField]
    private FoodStateData _currentState;

    private Dictionary<FoodStateData, FoodStateBehaviour> _stateToAnimator;

    private float _stateProgress = 0.0f;

    void Awake()
    {
        foreach (FoodStateBehaviour animator in _allowedFoodStates)
        {
            _stateToAnimator.Add(animator.FoodState, animator);
        }
    }

    public void SetState(FoodStateData foodState)
    {
        if (_currentState == null) { return; }

        if (_stateToAnimator[foodState] != null)
        {
            _stateToAnimator[foodState].HideAndReset();
        }
        
        _currentState = foodState;

        if (_stateToAnimator[foodState] != null)
        {
            _stateToAnimator[foodState].Show();
        }
    }

    public void SetStateProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);

        _stateProgress = progress;

        _stateToAnimator[_currentState].OnProgressUpdate(_stateProgress);
    }
}