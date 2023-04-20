using UnityEngine;
using CustomAttributes;
using UnityEditor;
using System;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : InteractionBehaviour
{
    [SerializeField]
    [ProgressBar("Progress", 1.0f, EColor.Green)]
    public float _progressIndicator = 0.0f;

    [SerializeField]
    private Ingredient _ingredientData;

    [SerializeField]
    [Tooltip("Maps ingredient state to the game object that holds the model for that state")]
    private StateToModelDictionary _stateToModel;

    public float Progress => _ingredientData.Progress;
    public bool ProgressComplete => _ingredientData.ProgressComplete;

    public IngredientStateData State => _ingredientData.State;
    public Ingredient Data => _ingredientData;

    /// <summary>
    /// Makes sure the correct model is active when the state is changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        if (State != null && _stateToModel.TryGetValue(State, out GameObject model))
        {
            DisableAllModels();
            _stateToModel[State].SetActive(true);
        }
    }

    public void AddProgress(float progressDelta)
    {
        SetProgress(Progress + progressDelta);
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        _progressIndicator = progress;
        _ingredientData.SetProgress(progress);
        OnProgressUpdate(progress);
    }

    /// <summary>
    /// Resets the progress of the ingredient if the state is changed
    /// Also makes sure the correct model is active 
    /// </summary>
    /// <param name="state"></param>
    public void SetState(IngredientStateData state)
    {
        _ingredientData.SetState(state);
        if (_stateToModel.TryGetValue(state, out GameObject model))
        {
            DisableAllModels();
            model.SetActive(true);
        }
    }

    /// <summary>
    /// Called when the progress of the ingredient is updated
    /// TODO: Override this method to handle visual behaviour of ingredient
    /// </summary>
    /// <param name="progress"></param>
    protected virtual void OnProgressUpdate(float progress)
    {
        // handle visual behaviour of ingredient
    }

    /// <summary>
    /// Hides all models
    /// </summary>
    private void DisableAllModels()
    {
        foreach (var pair in _stateToModel)
        {
            pair.Value.SetActive(false);
        }
    }
}

[Serializable]
public class StateToModelDictionary : SerializableDictionary<IngredientStateData, GameObject> { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StateToModelDictionary))]
public class StateToModelDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
#endif