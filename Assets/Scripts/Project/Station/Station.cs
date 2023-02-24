using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station<T> : Selectable
{
    [SerializeField]
    private Selectable _heldItem;

    [SerializeField]
    private Transform _itemHolderPivot;

    protected T _requiredComponent;

    public override bool IsCarryable
    {
        get => false;
    }

    public override bool IsInteractable
    {
        get => true;
    }

    void Update()
    {
        OnUpdate();
    }

    public override Selectable GetCarryableItem()
    {
        var item = _heldItem;

        _heldItem = null;
        _requiredComponent = default;

        return item;
    }

    public override bool TryPlaceItem(Selectable item)
    {
        if (_heldItem != null)
        {
            return _heldItem.TryPlaceItem(item);
        }
        else
        {
            if (item.TryGetComponent(out T component) && ValidateItem(item))
            {
                _requiredComponent = component;
                PlaceItem(item);
                return true; 
            }
            return false;
        }
    }

    protected virtual void OnUpdate()
    {
        // do something with _container automatically (cooking, etc)
    }

    protected virtual bool TryGetItemComponent(ref T component)
    {
        if (_requiredComponent != null)
        {
            component = _requiredComponent;
            return true;
        }
        else
        {
            component = default;
            return false;
        }
    }

    protected virtual bool ValidateItem(Selectable item)
    {
        return true;
    }

    private void PlaceItem(Selectable item)
    {
        _heldItem = item;

        CenterObject(item.gameObject);

        item.OnPlace();
    }

    private void CenterObject(GameObject go)
    {
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
