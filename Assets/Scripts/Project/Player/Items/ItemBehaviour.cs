using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBehaviour : MonoBehaviour
{
    [HideInInspector]
    public ItemBehaviourCollection Collection;
}

public abstract class Combinable : ItemBehaviour
{
    public abstract bool TryAddItem(ItemBehaviourCollection other);
}

public interface IInteractable
{
    public void OnInteractStart();
    public void OnInteractEnd();
    public bool Enabled {get;}
}