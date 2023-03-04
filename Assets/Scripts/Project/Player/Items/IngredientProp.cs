using UnityEngine;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : MonoBehaviour
{
    [SerializeField]
    private IngredientType _type;

    [SerializeField]
    private IngredientStateData _state;

    private Ingredient _ingredientData;

    public float Progress
    {
        get => _ingredientData.Progress;
        set => _ingredientData.Progress = value;
    }

    public bool ProgressComplete
    {
        get => _ingredientData.ProgressComplete;
    }

    public Ingredient Data
    {
        get => _ingredientData;
        set => _ingredientData = value;
    }

    void Awake()
    {
        _ingredientData = new(_type, _state);
        OnAwake();
    }

    public void AddProgress(float progressDelta)
    {
        SetProgress(Progress + progressDelta);
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        _ingredientData.SetProgress(progress);
        OnProgressUpdate(progress);
    }
    
    /// <summary>
    ///  Resets progress if state is changed
    /// </summary>
    public void SetIngredientState(IngredientStateData state)
    {
        if (state != _state)
        {
            _state = state;
            Data.ResetState(state);
        }
    }

    protected virtual void OnProgressUpdate(float progress)
    {
        // handle visual behaviour of ingredient
    }

    protected virtual void OnAwake() {}
}