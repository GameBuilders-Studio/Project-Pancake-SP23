using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "New Order Data", menuName = "Order Data")]
public class OrderData : ScriptableObject
{
    public LevelRecipeListDictionary orders;
}

[System.Serializable]
public class RecipeListStorage : SerializableDictionary.Storage<List<RecipeData>> { }

[CustomPropertyDrawer(typeof(RecipeListStorage))]
public class ListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }

[System.Serializable]
public class LevelRecipeListDictionary : SerializableDictionary<int, List<RecipeData>, RecipeListStorage> { }

[CustomPropertyDrawer(typeof(LevelRecipeListDictionary))]
public class ListSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
