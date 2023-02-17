/****************************
* written by Richard
* 2023.2.16
******************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


///<summary> 
/// Abstract Class 
///<para>abstract class for all kinds of containers</para> 
///</summary> 
public abstract class Ab_Container : MonoBehaviour     
{   
    protected List<Pickable> items;
    public abstract bool PlaceItemIntoHolder(); 
    public abstract void OnRemovingObjectFromHolder();
    public abstract List<Pickable> getPickableItemsList();
    public virtual bool IsEmpty() { return items == null || items.Count == 0;}
}

// object.transform.SetParent(gameObject.transform);
//     object.GetComponent<Rigidbody>().isKinematic = true;  //If isKinematic is enabled, Forces, collisions or joints will not affect the rigidbody anymore.
//     return true;
// }

