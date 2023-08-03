using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[CreateAssetMenu(menuName = "Props/Ingredient State")]
public class IngredientStateData : ScriptableObject
{
    [SerializeField]
    bool _hasProgress = false;
    public bool HasProgress => _hasProgress;
}
