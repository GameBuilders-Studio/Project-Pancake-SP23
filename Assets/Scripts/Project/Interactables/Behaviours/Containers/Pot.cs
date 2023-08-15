
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : FoodContainer
{
    [SerializeField]
    private GameObject _soupVisual;

    private Renderer _soupRenderer;

    [SerializeField]
    private GameObject _cookingVisual;

    [SerializeField]
    private ParticleSystem _bubbleParticles;

    [SerializeField]
    private ParticleSystem _smokeParticles;

    [SerializeField]
    private Color _smokeColor;

    [SerializeField] private Transform foodAnchor;

    [SerializeField] private InGameProgress pgBar;
    [SerializeField] private GameObject warningSign;
    [SerializeField] private GameObject checkmark;
    public IngredientStateData targetCookState;

    private float _warningTime = 6f;
    private float _quickWarningTime = 2.5f;
    private float _slowFreq = 0.7f;
    private float _quickFreq = .1f;

    private void Awake()
    {
        _soupVisual.SetActive(false);
        _soupRenderer = _soupVisual.GetComponent<Renderer>();
    }

    protected override void OnIngredientsChanged(HashSet<(IngredientType, IngredientStateData)> ingredientTypes)
    {
        Debug.Log("Ingredients Changed!");
        SetOvercookedVisuals(false);
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
        SetOvercookedVisuals(true);
        for (int i = 0; i < foodAnchor.childCount; i++)
        {
            foodAnchor.GetChild(i).gameObject.SetActive(false);
        }
        StopFlashing();
    }

    public void EnableCookingVisual() {
        if(_cookingVisual.activeSelf == false){
            _cookingVisual.SetActive(true);
        }
    }

    public void DisableCookingVisual() {
        if(_cookingVisual.activeSelf == true) {
            _cookingVisual.SetActive(false);
        }
    }

    private void SetOvercookedVisuals(bool overcooked) {
        ParticleSystem.MainModule smokeMainModule = _smokeParticles.main;
        if(overcooked) {
            _soupRenderer.material.color = Color.black;
            _bubbleParticles.Stop();
            smokeMainModule.startColor = Color.black;
        } else {
            _soupRenderer.material.color = Color.white;
            _bubbleParticles.Play();
            smokeMainModule.startColor = _smokeColor;
        }
    }
}
