using UnityEngine;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : Carryable
{
    [Space(15f)]
    [SerializeField]
    private IngredientType _type;

    [SerializeField]
    private IngredientStateData _initialState;

    private Ingredient _ingredientData;

    public float Progress
    {
        get => _ingredientData.Progress;
        set => _ingredientData.Progress = value;
    }

    public Ingredient Data
    {
        get => _ingredientData;
        set => _ingredientData = value;
    }

    protected override void OnAwake()
    {
        _ingredientData = new(_type, _initialState);
    }

    public void AddProgress(float progressDelta)
    {
        SetProgress(Progress + progressDelta);
    }

    public void SetProgress(float progress)
    {
        _ingredientData.SetProgress(progress);
        OnProgressUpdate(progress);
    }

    protected virtual void OnProgressUpdate(float progress)
    {
        // handle visual behaviour of ingredient
    }
}