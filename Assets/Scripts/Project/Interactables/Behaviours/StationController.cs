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

    public virtual void PositionItem(ref Carryable item, Transform pivot)
    {
        var go = item.gameObject;
        go.transform.SetParent(pivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
