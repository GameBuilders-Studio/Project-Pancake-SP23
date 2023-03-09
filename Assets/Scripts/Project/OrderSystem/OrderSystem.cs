using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderSystem : MonoBehaviour
{
    [SerializeField] private Canvas _canvasReference;
    [SerializeField]
    private int _currentLevel;
    [SerializeField]
    private OrderData _orderData;
    [SerializeField]
    private float _defaultOrderSpawnTime = 15f;
    [SerializeField]
    private int _maxConcurrentOrders = 6;
    [SerializeField]
    private int _minConcurrentOrders = 2;

    private Coroutine _orderSpawnCoroutine;
    private WaitForSeconds _orderSpawnWait;

    //for displaying
    private int _currentOrderIndex = 0;

    private List<Order> _currentOrders = new List<Order>();
    private Queue<Order> _originalSequence = new Queue<Order>();
    public List<Order> CurrentOrders
    {
        get => _currentOrders;
    }
    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);
        Debug.Log("Order added " + order.RecipeData.name);
    }
    public void RemoveOrder(int index)
    {
        _currentOrders.RemoveAt(index);
    }
    public Order GetOrder(int index)
    {
        //todo: check if index is valid
        return _currentOrders[index];
    }
    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);
        EventManager.AddListener("OrderExpired", OnOrderExpired);
    }

    private void Awake()
    {
        _orderSpawnWait = new WaitForSeconds(_defaultOrderSpawnTime);
        foreach (Order order in _orderData.Orders[_currentLevel])
        {
            _originalSequence.Enqueue(order);
        }

    }

    private void OnStartLevel()
    {
        while (_currentOrders.Count < _minConcurrentOrders)
        {
            SpawnOrder();
        }
        _orderSpawnCoroutine = StartCoroutine(OrderSpawnCoroutine()); //Start spawning orders if StartingLevel event is invoked
    }

    private IEnumerator OrderSpawnCoroutine()
    {
        while (true)
        {
            SpawnOrder();
            Debug.Log("Spawning order");
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
        if (_currentOrderIndex >= _maxConcurrentOrders)
        {
            _currentOrderIndex = 0;
        }

        if (_currentOrders.Count < _maxConcurrentOrders)
        {
            if (_originalSequence.Count > 0)
            {
                Order order = Instantiate(_originalSequence.Dequeue());
                _currentOrders.Add(order);
                order.transform.SetParent(_canvasReference.transform);
                order.transform.Translate(new Vector3((_currentOrderIndex - _maxConcurrentOrders / 2) * 300, 200, 0)); //display test
            }
            else
            {

                int randomIndex = Random.Range(0, _orderData.Orders[_currentLevel].Count);
                Order order = Instantiate(_orderData.Orders[_currentLevel][randomIndex]);
                _currentOrders.Add(order);
                order.transform.SetParent(_canvasReference.transform);
                order.transform.Translate(new Vector3((_currentOrderIndex - _maxConcurrentOrders / 2) * 300, 200, 0));  //display test
            }
            _currentOrderIndex++;
        }
    }


    public void CheckOrderMatch(List<IngredientType> ingredients)
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
            _currentOrders.RemoveAt(orderIndexToRemove);
            EventManager.Invoke("IncrementingScore");
            //todo: when orders drop below 2 add a new order
            if (_currentOrders.Count < _minConcurrentOrders)
            {
                SpawnOrder();
            }
        }

    }

    private void OnOrderExpired()
    {
        for (int i = 0; i < _currentOrders.Count; i++)
        {
            if (_currentOrders[i].TimeRemaining <= 0)
            {
                Destroy(_currentOrders[i].gameObject);
                _currentOrders.RemoveAt(i);
                if (_currentOrders.Count < _minConcurrentOrders)
                {
                    SpawnOrder();
                }
            }
        }
    }



}


// //todo:
// //add a system to add orders according to specs listed on trello
// //add a system to remove the order if the timer runs out





