using UnityEngine;
using CustomAttributes;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : InteractionBehaviour
{
    [ProgressBar("Progress", 1.0f, EColor.Green)]
    public float _progressIndicator = 0.0f;

    [SerializeField]
    private IngredientType _type;

    [SerializeField]
    [Required]
    private IngredientStateData _state;

    private Ingredient _ingredientData;

    public float Progress => _ingredientData.Progress;

    public bool ProgressComplete => _ingredientData.ProgressComplete;

    public Ingredient Data => _ingredientData;

    protected virtual void Awake()
    {
        _ingredientData = new(_type, _state);
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
    
    protected virtual void OnProgressUpdate(float progress)
    {
        // handle visual behaviour of ingredient
    }
}