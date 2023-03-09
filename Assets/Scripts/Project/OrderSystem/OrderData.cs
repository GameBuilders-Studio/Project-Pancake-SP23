using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//Use Order instead of recipedata

[CreateAssetMenu(fileName = "New Order Data", menuName = "Order Data")]
public class OrderData : ScriptableObject
{
    [SerializeField]
    private LevelOrderListDictionary _orders;

    public LevelOrderListDictionary Orders
    {
        get { return _orders; }
    }
}

[System.Serializable]
public class OrderListStorage : SerializableDictionary.Storage<List<Order>> { }

[CustomPropertyDrawer(typeof(OrderListStorage))]
public class ListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }

[System.Serializable]
public class LevelOrderListDictionary : SerializableDictionary<int, List<Order>, OrderListStorage> { }

[CustomPropertyDrawer(typeof(LevelOrderListDictionary))]
public class ListSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
