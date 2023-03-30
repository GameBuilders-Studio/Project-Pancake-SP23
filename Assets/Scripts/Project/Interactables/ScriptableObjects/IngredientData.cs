using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    public string Name;
    public Sprite Sprite;
    public GameObject prafab;
}
