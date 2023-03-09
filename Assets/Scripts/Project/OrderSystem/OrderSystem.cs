using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Order
{
    public RecipeData recipe;
    public float timeRemaining;

    public Order(RecipeData recipe, float timeRemaining)
    {
        this.recipe = recipe;
        this.timeRemaining = timeRemaining;
    }

}

public class OrderSystem : MonoBehaviour
{
    [SerializeField]
    private OrderData _orderData;
    private List<Order> _currentOrders;
    private int _currentLevel = 0;
    private int _maxOrders = 6;
    private int _minOrders = 2;

    //todo: add a timer for each order
    //add a system to add orders according to specs listed on trello
    //add a system to remove order with highest priority
    //add a system to remove the order if the timer runs out

    public void Start()
    {
        _currentOrders = new List<Order>();
        for (int i = 0; i < 2; i++)
        {
            AddOrder(randomlyChooseNextOrder());
        }

    }
    public void Update()
    {
        if (_currentOrders.Count < _minOrders)
        {
            AddOrder(randomlyChooseNextOrder());
        }

        for (int i = _currentOrders.Count - 1; i >= 0; i--)
        {
            Order order = _currentOrders[i];
            if (order.timeRemaining > 0)
            {
                //Debug.Log("Order time remaining: " + order.timeRemaining);
                order.timeRemaining = Mathf.Max(order.timeRemaining - Time.deltaTime, 0);
            }
            else
            {
                Debug.Log("Order timed out");
                RemoveOrder(i);
            }
        }


    }

    public List<Order> CurrentOrders
    {
        get => _currentOrders;
    }
    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);
        Debug.Log("Order added" + order.recipe.name);
    }

    public void RemoveOrder(int index)
    {
        _currentOrders.RemoveAt(index);
    }
    public Order GetOrder(int index)
    {
        return _currentOrders[index];
    }
    public bool IsOrderValid(List<IngredientType> ingredients)
    {
        foreach (Order order in _currentOrders)
        {
            if (order.recipe.IsRecipeValid(ingredients))
            {
                Debug.Log("Order is valid");
                return true;
            }
        }
        return false;
    }

    private Order randomlyChooseNextOrder()
    {
        int randomIndex = Random.Range(0, _orderData.orders[_currentLevel].Count);
        float randomTimeInSeconds = Random.Range(5, 10);
        RecipeData recipe = _orderData.orders[_currentLevel][randomIndex];
        return new Order(recipe, randomTimeInSeconds);
    }

    private void printTime(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        Debug.Log(string.Format("{0:00}:{1:00}", minutes, seconds));
    }


}
