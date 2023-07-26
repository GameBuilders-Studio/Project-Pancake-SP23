
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : FoodContainer
{
    [SerializeField]
    private GameObject _soupVisual;

    private Renderer _soupRenderer;

    [SerializeField] private Transform foodAnchor;

    [SerializeField] private InGameProgress pgBar;
    [SerializeField] private GameObject warningSign;
    [SerializeField] private GameObject checkmark;

    private float _warningTime = 6f;
    private float _quickWarningTime = 2.5f;
    private float _slowFreq = 0.7f;
    private float _quickFreq = .1f;

    private void Awake()
    {
        _soupVisual.SetActive(false);
        _soupRenderer = _soupVisual.GetComponent<Renderer>();
    }

    protected override void OnIngredientsChanged(HashSet<IngredientType> ingredientTypes)
    {
        base.OnIngredientsChanged(ingredientTypes);
        bool showSoup = !IsEmpty;
        _soupVisual.SetActive(showSoup);
    }

    public void SetProgress(float pg)
    {
        // simply forward this
        pgBar.SetProgress(pg);
    }

    // bool is either the checkmark or the warning sign
    public void StartFlashing(float timeRemaining)
    {
        StartCoroutine(FlashingCoro(timeRemaining));
    }

    private IEnumerator FlashingCoro(float timeRemaining)
    {
        if (timeRemaining > _warningTime)
        {
            float waitTime = Mathf.Repeat(timeRemaining, _slowFreq);
            yield return new WaitForSeconds(waitTime);
            checkmark.SetActive(true);
            while (timeRemaining > _warningTime)
            {
                yield return new WaitForSeconds(_slowFreq);
                timeRemaining -= _slowFreq;
                checkmark.SetActive(!checkmark.activeSelf);
            }
        }
        checkmark.SetActive(false);

        if (timeRemaining > _quickWarningTime)
        {
            float slowWarningWaitTime = Mathf.Repeat(timeRemaining, _slowFreq);
            yield return new WaitForSeconds(slowWarningWaitTime);
            warningSign.SetActive(true);
            while (timeRemaining > _quickWarningTime)
            {
                yield return new WaitForSeconds(_slowFreq);
                timeRemaining -= _slowFreq;
                warningSign.SetActive(!warningSign.activeSelf);
            }
        }

        while (timeRemaining > 0f)
        {
            yield return new WaitForSeconds(_quickFreq);
            timeRemaining -= _quickFreq;
            warningSign.SetActive(!warningSign.activeSelf);
        }

        warningSign.SetActive(false);
    }

    // this stops the flashing no matter what
    public void StopFlashing()
    {
        StopAllCoroutines();
        warningSign.SetActive(false);
        checkmark.SetActive(false);
    }

    public void Overcooked()
    {
        _soupRenderer.material.color = Color.black;
        for (int i = 0; i < foodAnchor.childCount; i++)
        {
            foodAnchor.GetChild(i).gameObject.SetActive(false);
        }
        StopFlashing();
    }
}
