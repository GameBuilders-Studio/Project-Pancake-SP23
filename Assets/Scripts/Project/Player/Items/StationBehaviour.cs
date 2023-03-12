using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationBehaviour : MonoBehaviour
{
    public virtual bool ValidateItem(Carryable item) => true;

    public virtual void OnItemPlaced(ref Carryable item) {}

    public virtual void OnItemRemoved(ref Carryable item) {}
}
