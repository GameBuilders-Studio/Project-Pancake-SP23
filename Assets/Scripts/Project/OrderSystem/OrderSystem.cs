using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class OrderSystem : MonoBehaviour
{
    [SerializeField] private OrderUI _orderUI; //UI that displays the orders
    [SerializeField, Range(0, 100)] private int _currentLevel; //Use current level to get the sequence of orders from _orderData
    [SerializeField] private OrderData _orderData; //Serializable dictionary that stores all orders for each level
    [SerializeField, Range(0f, 600f)] private float _orderSpawnTime = 15f; //Time in seconds between order spawns
    [SerializeField] private int _maxConcurrentOrders = 6; //Maximum number of orders that can be displayed at once
    [SerializeField] private int _minConcurrentOrders = 2; //Minimum number of orders that can be displayed at once
    [SerializeField] private float _stageStartDelay = 2.0f; //Delay before the first order spawns
    [SerializeField] private ScoreUI _scoreUI;
    [SerializeField] private SceneLoader _sceneLoader;
    [SerializeField] private int _nthOrderConsecutive = 0;
    [SerializeField] private int _lastSentOutIndex = -1;

    private Coroutine _orderSpawnCoroutine;
    private List<Order> _currentOrders = new();
    private Queue<Order> _orderQueue = new();

    // this is so that order system main file has absolute control over score generation
    // this is synced with score UI
    private int _score;

    public List<Order> CurrentOrders { get => _currentOrders; }

    private void Awake()
    {
        //Load original sequence of orders for the current level

    }

    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);
        EventManager.AddListener("OrderExpired", OnOrderExpired);
        EventManager.AddListener("TimerEnded", OnFinishLevel);
    }

    private void OnDisable()
    {
        StopOrderSpawn(); //Should we also stop the order timer coroutines?
    }

    private void Start()
    {
        foreach (Order order in _orderData.Orders)
        {
            _orderQueue.Enqueue(order);
        }
        StartCoroutine(StartLevelCoroutine());
    }
    public void AddOrder(Order order)
    {
        _orderQueue.Enqueue(order);
    }

    public Order RemoveOrder(int index)
    {
        if (index < 0 || index >= _currentOrders.Count)
        {
            Debug.LogError("Index out of range");
            return null;
        }

        Order temp = _currentOrders[index];
        _currentOrders.Remove(temp); //Remove from current orders list
        _orderUI.RemoveOrder(temp); //Remove from order UI

        //When orders drop below minimum, instantly add a new order
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

    /// <summary>
    /// Checks for a matching order with the the highest priority (lowest time remaining) from the orders list and removes it.
    /// Returns true if a matching order is found, false otherwise.
    /// </summary>
    public bool CheckOrderMatch(List<IngredientSO> ingredients)
    {
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
            temp.SetOrderComplete();
            temp.DespawnOrder();

            if (orderIndexToRemove >= _lastSentOutIndex)
            {
                _nthOrderConsecutive = Math.Min(4, _nthOrderConsecutive + 1);
            }
            else
            {
                _nthOrderConsecutive = 0;
            }

            _lastSentOutIndex = orderIndexToRemove;

            //EventManager.Invoke("IncrementingScore");

            _score += CalculateScore(
                minTimeRemaining,
                _currentOrders[orderIndexToRemove].GetTimeLimit(),
                Math.Max(_nthOrderConsecutive, 1),
                _currentOrders[orderIndexToRemove].RecipeData.baseScore);

            return true;
        }

        _lastSentOutIndex = -1;
        _nthOrderConsecutive = 0;

        return false;

    }

    private int CalculateScore(float orderTimeRemaining, float orderTimeLimit, int consecutiveMultiplier, int baseScore)
    {
        int tip = 3;

        if (orderTimeRemaining > orderTimeLimit / 3)
        {
            tip = 5;
        }

        if (orderTimeRemaining > orderTimeLimit / 3 * 2)
        {
            tip = 8;
        }

        return tip * consecutiveMultiplier + baseScore;
    }

    private void OnStartLevel()
    {
        while (_currentOrders.Count < _minConcurrentOrders - 1)
        {
            SpawnOrder();
        }
        if (_orderSpawnCoroutine == null)
        {
            //Start spawning orders if StartingLevel event is invoked (what happens if multiple calls?)
            _orderSpawnCoroutine = StartCoroutine(OrderSpawnCoroutine());
        }
        else
        {
            Debug.LogError("Order spawn coroutine already running");
        }

    }

    private IEnumerator OrderSpawnCoroutine()
    {
        while (true)
        {
            SpawnOrder();
            yield return new WaitForSeconds(_orderSpawnTime);
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
            if (_orderQueue.Count > 0)
            {
                //If there are still orders in the original sequence, spawn them first
                Order order = Instantiate(_orderQueue.Dequeue());
                _currentOrders.Add(order);
                _orderUI.AddOrder(order);

            }
            else
            {
                //If there are no more orders in the original sequence, spawn a random order
                int randomIndex = Random.Range(0, _orderData.Orders.Count);
                Order order = Instantiate(_orderData.Orders[randomIndex]);
                _currentOrders.Add(order);
                _orderUI.AddOrder(order);

            }
        }
    }

    private void OnOrderExpired()
    {
        for (int i = 0; i < _currentOrders.Count; i++)
        {
            //Remove all orders that have expired
            Order currentOrder = _currentOrders[i];
            if (currentOrder.TimeRemaining <= 0)
            {
                Order temp = RemoveOrder(i);
                temp.DespawnOrder();
            }
        }
    }

    IEnumerator StartLevelCoroutine()
    {
        yield return new WaitForSeconds(_stageStartDelay);
        EventManager.Invoke("StartingLevel");
    }

    private void OnFinishLevel()
    {
        Debug.Log("Level Finished");
        DataCapsule.Instance.lastLevel = SceneManager.GetActiveScene().name;
        DataCapsule.Instance.score = _score;
        DataCapsule.Instance.scoreBarMax = 300;
        _sceneLoader.LoadScene();
    }

    private void Update()
    {
        // :copium: - Revan
        _scoreUI.Score = _score;
    }

}





