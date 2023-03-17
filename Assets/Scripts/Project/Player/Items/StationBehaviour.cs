using UnityEngine;

[RequireComponent(typeof(Station))]
public class StationBehaviour : MonoBehaviour
{
    public virtual bool ValidateItem(Carryable item) => true;

    public virtual void ItemPlaced(ref Carryable item) {}

    public virtual void ItemRemoved(ref Carryable item) {}
}
