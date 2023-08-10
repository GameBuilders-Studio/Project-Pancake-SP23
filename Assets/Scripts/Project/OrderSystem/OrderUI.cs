using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(RectTransform))]
public class OrderUI : MonoBehaviour
{
    [SerializeField] private float _spacing = 5.0f;
    [SerializeField] private float _leftPadding = 0.0f;
    [SerializeField] private float _slideUpTime = 2.0f;
    [SerializeField] private float _slideUpYPosition = 500.0f;
    [SerializeField] private float _slideLeftTime = 1.0f;


    private RectTransform _rectTransform;
    private List<Order> _currentOrders = new();

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }
    public void AddOrder(Order order)
    {
        _currentOrders.Add(order);

        //Display order in UI
        order.transform.SetParent(transform, false);
        // Debug.Log("order" + order);
        // Debug.Log("_rectTransform" + _rectTransform);
        order.RectTransform.anchoredPosition = new Vector2(_rectTransform.rect.width, 0);

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
        DOTween.Kill(order.RectTransform); //Stop any tweens on the order
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
            RectTransform childTransform = order.RectTransform;

            float offset = childTransform.rect.width;
            float spacing = _spacing * i;

            childTransform.DOAnchorPos(new Vector2(-distFromLeftSideX + (offset * i + offset / 2) + _leftPadding + spacing, 0), _slideLeftTime);
        }
    }

    public void SlideupOrder(Order order)
    {
        RectTransform rectTransform = order.RectTransform;
        rectTransform.DOAnchorPosY(_slideUpYPosition, _slideUpTime); //Move order up, out of view
    }

}
