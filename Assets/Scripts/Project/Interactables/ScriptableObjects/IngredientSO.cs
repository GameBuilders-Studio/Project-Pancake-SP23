using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/Ingredient Data")]
public class IngredientSO : ScriptableObject
{
    public Sprite icon;
    public GameObject prefab;
    public IngredientType type;
}

public enum IngredientType
{
    RedPlankton, GreenPlankton, BluePlankton, Rice, Seaweed, Pasta
}

[Serializable]
public class StateToPrefabDictionary : SerializableDictionary<IngredientStateData, GameObject> { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(StateToPrefabDictionary))]
public class StateToPrefabDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
#endif
