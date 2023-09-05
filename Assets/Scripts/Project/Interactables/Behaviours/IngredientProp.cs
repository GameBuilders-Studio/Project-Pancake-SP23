using UnityEngine;
using CustomAttributes;
using UnityEditor;
using System;
using System.Linq;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : InteractionBehaviour
{
    [SerializeField]
    [ProgressBar("Progress", 1.0f, EColor.Green)]
    public float _progressIndicator = 0.0f;

    [SerializeField] private GameObject _progressBarPrefab;
    private InGameProgress _progressBar;

    [SerializeField]
    private Ingredient _ingredient;

    [SerializeField]
    [Tooltip("Maps ingredient state to the game object that holds the model for that state")]
    private StateToModelDictionary _stateToModel;
    public StateToModelDictionary StateToModel => _stateToModel;

    [SerializeField]
    public float Progress => _ingredient.Progress;
    public bool ProgressComplete => _ingredient.ProgressComplete;

    public IngredientStateData State => _ingredient.State;
    public Ingredient Ingredient => _ingredient;

    private void Start() {
        GameObject progressBarObject = Instantiate(_progressBarPrefab);
        _progressBar = progressBarObject.GetComponentInChildren<InGameProgress>();
        if(_progressBar == null)
        {
            Debug.LogError("InGameProgress component not found in FoodContainer.cs");
        }
        _progressBar.SetTarget(gameObject.transform); // Set the target of the tooltip to this object
        Transform transform = GameObject.Find("Canvas").transform;
        if (transform == null)
        {
            Debug.LogError("Canvas not found in FoodContainer.cs");
        } else
        {
            progressBarObject.transform.SetParent(transform, false); // Set the parent of the tooltip to the HUD canvas
        }
    }

    /// <summary>
    /// Makes sure the correct model is active when the state is changed in the inspector
    /// </summary>
    private void OnValidate()
    {
        if (State != null && _stateToModel.TryGetValue(State, out GameObject model))
        {
            DisableAllModels();
            if (model != null)
            {
                model.SetActive(true);
            }
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
        _ingredient.SetProgress(progress);
        _progressBar.SetProgress(progress);
        OnProgressUpdate(progress);
    }

    /// <summary>
    /// Resets the progress of the ingredient if the state is changed
    /// Also makes sure the correct model is active
    /// </summary>
    /// <param name="state"></param>
    public void SetState(IngredientStateData state)
    {
        _ingredient.SetState(state);
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
        foreach (var pair in _stateToModel.Where(pair => pair.Value != null))
        {
            if (pair.Value != null)
            {
                pair.Value.SetActive(false);
            }
        }
    }
}

[Serializable]
public class StateToModelDictionary : SerializableDictionary<IngredientStateData, GameObject> { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StateToModelDictionary))]
public class StateToModelDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
#endif
