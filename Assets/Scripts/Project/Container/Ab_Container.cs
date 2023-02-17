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
    
    protected bool _isCarryable;
    public bool IsCarryable
    {
        get => _isCarryable;
        set => _isCarryable = value;
    }

    public abstract bool IsEmpty();
    public abstract bool PlaceItemIntoContaier(GameObject gameObject); 
    //should envoke by pickuper to delete reference
    public abstract void OnPickup(GameObject gameObject);
}

// object.transform.SetParent(gameObject.transform);
//     object.GetComponent<Rigidbody>().isKinematic = true;  //If isKinematic is enabled, Forces, collisions or joints will not affect the rigidbody anymore.
//     return true;
// }

