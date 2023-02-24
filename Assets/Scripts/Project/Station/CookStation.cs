using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookStation : Station<CookContainer>
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    private CookContainer _cookContainer;

    protected override void OnUpdate()
    {
        if (PlacedItemHasRequiredComponent(ref _cookContainer))
        {
            _cookContainer.Cook();
        }
    }
}
