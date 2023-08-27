using System.Collections.Generic;
using UnityEngine;

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

    [Tooltip("The score required for the player to get 3 stars on this level")]
    [SerializeField] private int _scoreMax = 300; 
    public int ScoreMax
    {
        get { return _scoreMax; }
    }
}
