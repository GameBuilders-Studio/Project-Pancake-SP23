using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

[System.Serializable]
public class Ingredient
{
    public IngredientSO Data;
    public IngredientStateData State;

    [ProgressBar("Progress", 1.0f, EColor.Green)]
    public float Progress = 0.0f;

    public bool ProgressComplete => Mathf.Approximately(Progress, 1.0f);

    public Ingredient(IngredientSO data, IngredientStateData state)
    {
        Data = data;
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
        SetProgress(Mathf.Clamp01(Progress + progressDelta));
    }

    /// <summary>
    ///  Resets progress if state is changed
    /// </summary>
    public void SetState(IngredientStateData state)
    {
        if (state != State)
        {
            State = state;
            ResetState(state);
        }
    }

    public void ResetState(IngredientStateData state)
    {
        State = state;
        Progress = 0.0f;
    }
}
