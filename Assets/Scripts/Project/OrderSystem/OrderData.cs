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
    [SerializeField] private List<Order> orders;
    public List<Order> Orders
    {
        get { return orders; }
    }
}
