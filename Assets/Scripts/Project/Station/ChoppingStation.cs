using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingStation : Station<Ingredient>
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    private Ingredient _ingredient;

    public void Chop()
    {
        if (TryGetItemComponent(ref _ingredient))
        {
            _ingredient.AddProgress(Time.deltaTime / _totalChopTime);
        }
        else
        {
            Debug.Log("L");
        }
    }

    protected override void OnUpdate()
    {
        if (IsInteracting)
        {
            Debug.Log("chopping!");
            Chop();
        }
    }
}
