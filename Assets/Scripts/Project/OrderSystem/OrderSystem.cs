using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderSystem : MonoBehaviour
{
    [SerializeField] private OrderUI _orderUI; //UI that displays the orders
    [SerializeField] private int _currentLevel; //Use current level to get the sequence of orders from _orderData
    [SerializeField] private OrderData _orderData; //Serializable dictionary that stores all orders for each level
    [SerializeField] private float _orderSpawnTime = 15f; //Time between order spawns
    [SerializeField] private int _maxConcurrentOrders = 6; //Maximum number of orders that can be displayed at once
    [SerializeField] private int _minConcurrentOrders = 2; //Minimum number of orders that can be displayed at once

    private Coroutine _orderSpawnCoroutine;
    private WaitForSeconds _orderSpawnWait;

    private List<Order> _currentOrders = new();
    private Queue<Order> _originalSequence = new();
    public List<Order> CurrentOrders { get => _currentOrders; }
    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);
        Debug.Log("Order added " + order.RecipeData.name);
    }
    public Order RemoveOrder(int index)
    {
        if (index < 0 || index >= _currentOrders.Count)
        {
            Debug.LogError("Index out of range");
            return null;
        }

        Order temp = _currentOrders[index];
        _orderUI.RemoveOrder(temp); //Remove from order UI
        _currentOrders.Remove(temp); //Remove from current orders list
        Debug.Log("Order removed: " + temp.RecipeData.name);

        //when orders drop below minimum, instantly add a new order
        if (_currentOrders.Count < _minConcurrentOrders)
        {
            SpawnOrder();
        }

        return temp;
    }
    public Order GetOrder(int index)
    {
        return _currentOrders[index];
    }
    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);
        EventManager.AddListener("OrderExpired", OnOrderExpired);
    }

    private void Awake()
    {
        _orderSpawnWait = new WaitForSeconds(_orderSpawnTime);

        //Load original sequence of orders for the current level
        foreach (Order order in _orderData.Orders[_currentLevel])
        {
            _originalSequence.Enqueue(order);
        }

    }

    private void OnStartLevel()
    {
        while (_currentOrders.Count < _minConcurrentOrders - 1)
        {
            SpawnOrder();
        }
        _orderSpawnCoroutine = StartCoroutine(OrderSpawnCoroutine()); //Start spawning orders if StartingLevel event is invoked (what happens if multiple calls?)
    }

    private IEnumerator OrderSpawnCoroutine()
    {
        while (true)
        {

            SpawnOrder();
            yield return _orderSpawnWait;
        }
    }

    private void StopOrderSpawn()
    {
        if (_orderSpawnCoroutine != null)
        {
            StopCoroutine(OrderSpawnCoroutine());
            _orderSpawnCoroutine = null;
        }
    }

    private void ResumeOrderSpawn()
    {
        if (_orderSpawnCoroutine == null)
        {
            _orderSpawnCoroutine = StartCoroutine(OrderSpawnCoroutine());
        }
    }

    private void SpawnOrder()
    {
        if (_currentOrders.Count < _maxConcurrentOrders)
        {
            if (_originalSequence.Count > 0)             //If there are still orders in the original sequence, spawn them first
            {
                Order order = Instantiate(_originalSequence.Dequeue());
                Debug.Log("Spawning order: " + order.RecipeData.name);
                _currentOrders.Add(order);
                _orderUI.AddOrder(order);

            }
            else                                        //If there are no more orders in the original sequence, spawn a random order
            {
                int randomIndex = Random.Range(0, _orderData.Orders[_currentLevel].Count);
                Order order = Instantiate(_orderData.Orders[_currentLevel][randomIndex]);
                Debug.Log("Spawning order: " + order.RecipeData.name);
                _currentOrders.Add(order);
                _orderUI.AddOrder(order);

            }
        }
    }


    public bool CheckOrderMatch(List<IngredientType> ingredients)
    {
        //Check for a matching order with the the highest priority (lowest time remaining)
        int orderIndexToRemove = -1;
        float minTimeRemaining = float.MaxValue;

        for (int i = 0; i < _currentOrders.Count; i++)
        {
            if (_currentOrders[i].RecipeData.IsRecipeValid(ingredients))
            {
                if (_currentOrders[i].TimeRemaining < minTimeRemaining)
                {
                    minTimeRemaining = _currentOrders[i].TimeRemaining;
                    orderIndexToRemove = i;
                }
            }
        }

        if (orderIndexToRemove != -1)
        {
            Order temp = RemoveOrder(orderIndexToRemove);
            Destroy(temp.gameObject);
            EventManager.Invoke("IncrementingScore");
            return true;
        }

        return false;

    }

    private void OnOrderExpired()
    {
        for (int i = 0; i < _currentOrders.Count; i++) //Remove all orders that have expired
        {
            Order currentOrder = _currentOrders[i];
            if (currentOrder.TimeRemaining <= 0)
            {
                Order temp = RemoveOrder(i);
                Destroy(temp.gameObject);
            }
        }
    }



}


// //todo:
// //add a system to add orders according to specs listed on trello





