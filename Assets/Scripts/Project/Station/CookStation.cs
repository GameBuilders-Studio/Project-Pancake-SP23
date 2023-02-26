using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookStation : Station
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    protected override void OnUpdate()
    {
        if (PlacedItem is CookContainer cookContainer)
        {
            cookContainer.Cook();
        }
    }
}
