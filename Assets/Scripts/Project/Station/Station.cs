using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station<T> : Selectable where T : MonoBehaviour
{
    [SerializeField]
    private Selectable _placedItem;

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
        var item = _placedItem;

        _placedItem = null;
        _requiredComponent = null;

        return item;
    }

    public override bool TryPlaceItem(Selectable item)
    {
        if (_placedItem != null)
        {
            return _placedItem.TryPlaceItem(item);
        }
        else
        {
            if (!ValidateItem(item)) { return false; }

            item.TryGetComponent(out T component);
            _requiredComponent = component;

            PlaceItem(item);

            return true; 
        }
    }

    protected virtual void OnUpdate()
    {
        // do something with _container automatically (cooking, etc)
    }

    /// <summary>
    /// Returns true if an item placed on the station has the required component  
    /// </summary>
    protected virtual bool PlacedItemHasRequiredComponent(ref T component)
    {
        if (_requiredComponent != null)
        {
            component = _requiredComponent;
            return true;
        }
        else
        {
            component = null;
            return false;
        }
    }

    protected virtual bool ValidateItem(Selectable item)
    {
        return true;
    }

    private void PlaceItem(Selectable item)
    {
        _placedItem = item;

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
