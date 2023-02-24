using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Handles ingredient object behaviour
/// </summary>
public class IngredientProp : MonoBehaviour
{
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

    void Awake()
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
        if (Data.ProgressComplete)
        {
            Debug.Log("done!");
        }
    }
}