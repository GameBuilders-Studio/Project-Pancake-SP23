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
    private void Awake() {
        IsCarryable = false;
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
