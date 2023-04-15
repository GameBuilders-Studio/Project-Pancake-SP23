using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class OrderUI : MonoBehaviour
{
    [SerializeField] float _spacing;
    [SerializeField] float _leftPadding;

    private RectTransform _rectTransform;
    private List<Order> _currentOrders = new();

    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);

        //Display order in UI
        order.transform.SetParent(transform, false);
        order.GetRectTransform().anchoredPosition = new Vector2(_rectTransform.rect.width, 0);

        UpdateOrderUI();
    }

    public void RemoveOrder(Order order)
    {
        int idx = _currentOrders.IndexOf(order);
        if (idx == -1)
        {
            Debug.LogError("Order not found in UI");
            return;
        }

        _currentOrders.RemoveAt(idx);
        SlideupOrder(order);

        UpdateOrderUI();
    }

    public void UpdateOrderUI()
    {
        //Shift all orders to the left if there are empty slots
        float parentWidth = _rectTransform.rect.width;
        float distFromLeftSideX = parentWidth / 2;

        for (int i = 0; i < _currentOrders.Count; i++)
        {

            Order order = _currentOrders[i];
            RectTransform childTransform = order.GetRectTransform();

            float offset = childTransform.rect.width;
            float spacing = _spacing * i;

            childTransform.DOAnchorPos(new Vector2(-distFromLeftSideX + (offset * i + offset / 2) + _leftPadding + spacing, 0), 1f);
        }
    }

    public void SlideupOrder(Order order)
    {
        RectTransform rectTransform = order.GetRectTransform();
        rectTransform.DOAnchorPosY(500, 2f); //Move order up, out of view
    }

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

}
