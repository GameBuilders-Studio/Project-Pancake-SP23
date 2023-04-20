using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(Station))]
public class StationController : InteractionBehaviour
{
    [SerializeField]
    [Required]
    public Station Station;

    public virtual bool ValidateItem(Carryable carryable) => true;

    public virtual void ItemPlaced(ref Carryable carryable) {}

    public virtual void ItemRemoved(ref Carryable carryable) {}
}
