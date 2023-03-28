using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//<summary>
//This scriptable object is a serializable dictionary used to store the Orders sequence for each level
//</summary>

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

[System.Serializable]
public class LevelOrderListDictionary : SerializableDictionary<int, List<Order>, OrderListStorage> { }

#if UNITY_EDITOR

[CustomPropertyDrawer(typeof(LevelOrderListDictionary))]
public class ListSerializableDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

[CustomPropertyDrawer(typeof(OrderListStorage))]
public class ListStoragePropertyDrawer : SerializableDictionaryStoragePropertyDrawer { }

#endif
