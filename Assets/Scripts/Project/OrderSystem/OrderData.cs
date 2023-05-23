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

    [SerializeField] private List<Order> listOrders;

    public void Initialize()
    {
        _orders = new Dictionary<int, List<Order>>
        {
            // this is to preserve compatibility with rest of the code
            // in practice I intend to set Current Level to always be 0. - Revan
            [0] = listOrders
        };
    }
}
