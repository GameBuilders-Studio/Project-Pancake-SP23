using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientType
{
    None,
    Cube,
    Sphere
}

public class Ingredient
{
    public IngredientType Type;

    public IngredientStateData State;

    public float Progress = 0.0f;

    public bool ProgressComplete
    {
        get => Mathf.Approximately(Progress, 1.0f);
    }

    public Ingredient(IngredientType type, IngredientStateData state)
    {
        Type = type;
        State = state;
    }

    public void SetProgress(float progress)
    {
        progress = Mathf.Clamp01(progress);
        Progress = progress;
    }

    public void ResetState(IngredientStateData state)
    {
        State = state;
        Progress = 0.0f;
    }
}
