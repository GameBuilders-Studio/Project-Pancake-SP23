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
        Container container = this.getContainer();
        if (!this.IsEmpty() && container == null) {             // non-container occupied
            return false;
        }
        else if (this.IsEmpty()) {                              // put directly if empty
            _item = gameObject;
            return true;
        }
        return  container.PlaceItemIntoContaier(gameObject);    // put into container 
 
    }

    /* either food or container, pick food from container processeded by container on it */
    public override void OnPickup(GameObject gameObject) { 
        _item = null;
        return;
    }

    public override bool IsEmpty() {
        return _item == null;
    }

    public Container getContainer() {
        return _item.GetComponent<Container>();
    }

}
