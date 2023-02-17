/****************************
* Container.cs
* written by Richard
* 2023.2.16
******************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///<summary> 
///this is for pickable container like pan and pot 
///</summary> 
public class Container : Ab_Container
{   
    protected List<GameObject> _items;
    protected int _capacity;

    public List<GameObject> Items
    {
        get => _items;
        set => _items = value;
    }

    [Tooltip("how many fooddata can store, usually has maxiam of 4")]
    [SerializeField]
    public int Capacity
    {
        get => _capacity;
        set => _capacity = value;
    }

    private void Awake() {
        _capacity = 4;
        _isCarryable = true;
        _items = new List<GameObject>(_capacity);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override bool PlaceItemIntoContaier(GameObject gameObject) {
        //TODO: should destroy first and then store the fooddata into it.
        if (_items.Count >= _capacity)
            return false;
        _items.Add(gameObject);
        return true;
    }

    //TODO: implementation needed
    public override void OnPickup(GameObject gameObject) {

        return;
    }

    public override bool IsEmpty() { return _items == null || _items.Count == 0;}

}
