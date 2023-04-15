using UnityEngine;
using CustomAttributes;


public class DirtyDish : InteractionBehaviour
{
    [SerializeField]
    private int _count = 0;
    public int Count
    {
        get => _count;
        set { 
            _count = value;
            // handle visual behaviour of dirty dish
        }
    }
}