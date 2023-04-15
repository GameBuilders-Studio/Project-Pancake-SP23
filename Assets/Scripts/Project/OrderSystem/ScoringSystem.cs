using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
NOTES:
-use GetOrder from ordersystem.cs
-If the most recent item submitted is the first element in the list, then we have submitted in order
*/

public class ScoringSystem : MonoBehaviour
{
    //Declare variables
    private List<Order> _currentOrdersChecks = new();
    private int multiplierCounter; // keeps track of what multiplier we should be at

    private OrderSystem submit; //checks if the item submitted is first

    //Variables for tip values
    private int greenTip = 8;
    private int yellowTip = 5;
    private int redTip = 3;

    public Order Scoring(){
        
    }
}
