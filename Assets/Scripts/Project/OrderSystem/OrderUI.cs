using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class OrderUI : MonoBehaviour
{
    [SerializeField] float _spacing;
    [SerializeField] float _leftPadding;

    private RectTransform _rectTransform;
    private List<Order> _currentOrders = new();

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);
        order.transform.SetParent(transform);
        // Debug.Log("Added " + order.RecipeData.name + " to UI");

        UpdateOrderUI(); //Reorder UI
    }

    public void RemoveOrder(Order order)
    {
        int idx = _currentOrders.IndexOf(order);
        if (idx == -1)
        {
            Debug.LogError("Order not found in UI");
            return;
        }

        _currentOrders[idx].gameObject.SetActive(false); //Disable the order
        _currentOrders.RemoveAt(idx);
        // Debug.Log("Removed " + order.RecipeData.name + " from UI");

        UpdateOrderUI(); //Reorder UI
    }
    public void UpdateOrderUI()
    {
        //Update UI
        float parentWidth = _rectTransform.rect.width;
        float distFromLeftSideX = parentWidth / 2;

        for (int i = 0; i < _currentOrders.Count; i++)
        {

            Order order = _currentOrders[i];
            RectTransform childTransform = order.GetComponent<RectTransform>();

            float offset = childTransform.rect.width;

            float spacing = _spacing * i;
            childTransform.anchoredPosition = new Vector2(-distFromLeftSideX + (offset * i + offset / 2) + _leftPadding + spacing, 0);
        }
    }

}
