/****************************
* Station.cs
* written by Richard
* 2023.2.16
******************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : Ab_Container
{   
    
    protected GameObject _item;  

    [Tooltip("should be container or food only")]
    [SerializeField]
    public GameObject Item
    {
        get => _item;
        set => _item = value;
    }

    private void Awake() {
        _isCarryable = false;
        _item = null;
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
        if (!this.IsEmpty()) {
            return false;
        }
        _item = gameObject;
        return true;
    }

    //TODO: implementation needed
    public override void OnPickup(GameObject gameObject) {
        _item = null;
        return;
    }

    public override bool IsEmpty() {
        return _item == null;
    }
}
