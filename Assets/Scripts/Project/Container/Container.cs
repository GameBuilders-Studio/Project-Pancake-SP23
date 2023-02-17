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
    private void Awake() {
        IsCarryable = true;
        items = new List<GameObject>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //TODO: implementation needed
    public override bool PlaceItemIntoHolder() {


        return false;
    }

    //TODO: implementation needed
    public override void OnRemovingObjectFromHolder() {

        return;
    }

}
