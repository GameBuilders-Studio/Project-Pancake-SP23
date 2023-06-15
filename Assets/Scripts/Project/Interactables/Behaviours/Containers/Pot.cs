
using UnityEngine;

public class Pot : FoodContainer
{
    [SerializeField]
    private GameObject _soupVisual;

    [SerializeField] private InGameProgress pgBar;

    private void Awake()
    {
        _soupVisual.SetActive(false);
    }

    protected override void OnIngredientsChanged()
    {
        bool showSoup = !IsEmpty;
        _soupVisual.SetActive(showSoup);
    }

    public void SetProgress(float pg)
    {
        // simply forward this
        pgBar.SetProgress(pg);
    }

}
