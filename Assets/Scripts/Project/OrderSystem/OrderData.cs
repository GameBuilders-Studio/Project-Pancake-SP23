using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

//<summary>
//This scriptable object is a serializable dictionary used to store the Orders sequence for each level
//</summary>

[CreateAssetMenu(fileName = "New Order Data", menuName = "Order Data")]
public class OrderData : ScriptableObject
{
    private Dictionary<int, List<Order>> _orders;
    public Dictionary<int, List<Order>> Orders
    {
        get { return _orders; }
    }

    [SerializeField] private List<GameObject> listOrders;

    public void Initialize()
    {
        _orders = new Dictionary<int, List<Order>>
        {
            [0] = listOrders.Select(x => x.GetComponent<Order>()).ToList(),
            [1] = listOrders.Select(x => x.GetComponent<Order>()).ToList()
        };
    }
}
