using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : FoodContainer
{
    [SerializeField]
    private GameObject _soupVisual;

    private void Awake()
    {
        _soupVisual.SetActive(false);
    }

    protected override void OnIngredientsChanged()
    {
        bool showSoup = !IsEmpty;
        _soupVisual.SetActive(showSoup);
    }

}
