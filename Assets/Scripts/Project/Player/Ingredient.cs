using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientType
{
    None,
    Cube,
    Sphere
}

[System.Serializable]
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
        if (progress > 1.0f || progress < 0.0f)
        {
            Debug.LogError("Progress must be between 0 and 1");
        }
        Progress = progress;
    }

    public void AddProgress(float progressDelta)
    {
        SetProgress(Progress + progressDelta);
    }

    public void ResetState(IngredientStateData state)
    {
        State = state;
        Progress = 0.0f;
    }
}
