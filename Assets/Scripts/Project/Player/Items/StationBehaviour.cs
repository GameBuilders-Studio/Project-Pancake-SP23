using UnityEngine;

[RequireComponent(typeof(Station))]
public class StationBehaviour : ItemBehaviour
{
    public virtual bool ValidateItem(Carryable carryable) => true;
    public virtual void ItemPlaced(ref Carryable carryable) {}
    public virtual void ItemRemoved(ref Carryable carryable) {}
}
