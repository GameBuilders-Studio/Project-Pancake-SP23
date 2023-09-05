using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pot : FoodContainer
{
    [Header("Ingredient State Data")]
    [SerializeField]
    private IngredientStateData _overCookedState;

    [SerializeField]
    private IngredientStateData _cookedState;

    [Header("Cooking Settings")]

    [Tooltip("Time it takes for a cooked ingredient to burn")]
    [SerializeField] 
    private float _overcookTime = 5f;

    [SerializeField]
    private float _cookTimePerIngredient;

    [Header("Visuals")]
    
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


    [Header("References")]
    [SerializeField] private Transform foodAnchor;
    [SerializeField] private GameObject _progressBarPrefab;
    private InGameProgress _progressBar;


    [Header("Flashing Icon Settings")]

    [Tooltip("When overcook time remaining is less than this, show slow warning sign")]
    [SerializeField] private float _warningTime = 6f;

    [Tooltip("When overcook time remaining is less than this, show quick warning sign")]
    [SerializeField] private float _quickWarningTime = 2.5f;
    [Tooltip("Frequency of flashing for slow warning sign")]
    [SerializeField] private float _slowFreq = 0.7f;
    [Tooltip("Frequency of flashing for quick warning sign")]
    [SerializeField] private float _quickFreq = .1f;

    Coroutine _flashingCoroutine;
    FlashingType _currentFlashingType = FlashingType.None;
    private enum FlashingType {
        None = 0,
        CheckMark = 1,
        WarningSlow = 2,
        WarningQuick = 3
    }

    private bool _overcooked = false;

    private bool _isOnStove = false;
    
    private float _overcookTimeRemaining;
    public float OvercookTimeRemaining
    {
        get { return _overcookTimeRemaining;}
        set
        {
            _overcookTimeRemaining = value;
            if (_overcookTimeRemaining <= 0f)
            {
                _overcookTimeRemaining = 0f;
                Overcooked();
            }
        }
    }
    private float _totalProgress = 0.0f;
    public float TotalProgress
    {
        get { return _totalProgress; }
        set
        {
            _totalProgress = value;
            if (_totalProgress >= 1f)
            {
                _totalProgress = 1f;
            }
            _progressBar.SetProgress(_totalProgress);
        }
    }

    private void Awake()
    {
        _soupVisual.SetActive(false);
        _soupRenderer = _soupVisual.GetComponent<Renderer>();
    }

    public override void Start() {
        base.Start();
        GameObject progressBarObject = Instantiate(_progressBarPrefab);
        _progressBar = progressBarObject.GetComponentInChildren<InGameProgress>();
        _progressBar.SetTarget(gameObject.transform); // Set the target of the tooltip to this object
        Transform transform = GameObject.Find("Canvas").transform;
        if (transform == null)
        {
            Debug.LogError("Canvas not found in FoodContainer.cs");
        } else
        {
            progressBarObject.transform.SetParent(transform, false); // Set the parent of the tooltip to the HUD canvas
        }
    }

    private void Update()
    {
        // The pot must be on the stove and not empty to cook
        if (_isOnStove && !IsEmpty)
        {
            EnableCookingVisual();
            Cook();
        } else {
            DisableCookingVisual();
        }
    }

    /// <summary>
    /// Sets whether the pot is on the stove or not
    /// </summary>
    /// <param name="isOnStove"></param>
    public void SetIsOnStove(bool isOnStove) {
        _isOnStove = isOnStove;
        if(!_isOnStove) {
            StopFlashingIcon();
        }
    }

    /// <summary>
    /// Called when the ingredients in the pot change. Mainly used to update visuals and _ingredientInfo
    /// </summary>
    /// <param name="ingredientTypes"></param> <summary>
    /// <param name="ingredientTypes"></param>
    protected override void OnIngredientsChanged(HashSet<(IngredientType, IngredientStateData)> ingredientTypes)
    {
        // Pot has only one ingredient, so if the ingredient changes, we can assume that the pot is empty or it is a new ingredient
        SetOvercookedVisuals(false);
        base.OnIngredientsChanged(ingredientTypes);
        bool showSoup = !IsEmpty;
        _soupVisual.SetActive(showSoup);
        _overcooked = false;
        OvercookTimeRemaining = _overcookTime; 
        TotalProgress = 0f;
    }

    /// <summary>
    /// Called when _overcookedTimeRemaining reaches 0, meaning the ingredients are overcooked
    /// </summary> 
    private void Overcooked()
    {
        _overcooked = true;
        SetOvercookedVisuals(true);
        for (int i = 0; i < foodAnchor.childCount; i++)
        {
            foodAnchor.GetChild(i).gameObject.SetActive(false);
        }
        StopFlashingIcon();
    }

    private void EnableCookingVisual() {
        if(_cookingVisual.activeSelf == false){
            _cookingVisual.SetActive(true);
        }
    }

    private void DisableCookingVisual() {
        if(_cookingVisual.activeSelf == true) {
            _cookingVisual.SetActive(false);
        }
    }

    private void Cook() {
        if(_overcooked) { return; } // The item will no longer cook until it is emptied and refilled

        TotalProgress = 0f;

        for (int i = 0; i < Count; i++)
        {
            var ingredient = _ingredients[i];
            if (ingredient.State != _cookedState && ingredient.State != _overCookedState) //only transition states if not already cooking or overcooked    
            {
                SetIngredientState(ingredient, _cookedState);
            }

            ingredient.AddProgress(Time.deltaTime / _cookTimePerIngredient);
            TotalProgress += ingredient.Progress / Count;
        }

        // If total progress is 1, and it is still cooking, then it is becoming overcooked 
        if (Mathf.Equals(TotalProgress, 1f))
        {
            ShowCheckmarkOrWarningSign();
            _overcookTimeRemaining -= Time.deltaTime;
            if(_overcookTimeRemaining <= 0f) {
                Overcooked();
            }
        }
    }

    /// <summary>
    /// Makes it start smoking and changes the color of the soup
    /// </summary>
    /// <param name="overcooked"></param>
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

    /// <summary>
    /// Called when the pot is done cooking, but the player has not taken the food out yet.
    /// It handles the logic for flashing the check mark or warning sign based on how close it is to being overcooked
    /// </summary>
    private void ShowCheckmarkOrWarningSign() {
        if (_overcookTimeRemaining > _warningTime)
        {
            if(_currentFlashingType != FlashingType.CheckMark) { SetFlashingCoroutine(FlashingType.CheckMark); }
        } else if (_overcookTimeRemaining <= _warningTime && _overcookTimeRemaining > _quickWarningTime) {
            if(_currentFlashingType != FlashingType.WarningSlow) { SetFlashingCoroutine(FlashingType.WarningSlow); }
        } else if (_overcookTimeRemaining <= _quickWarningTime && _overcookTimeRemaining > 0f) {
            if(_currentFlashingType != FlashingType.WarningQuick) { SetFlashingCoroutine(FlashingType.WarningQuick); }
        } else {
            if(_currentFlashingType != FlashingType.None) {SetFlashingCoroutine(FlashingType.None); }
        }
    }

    IEnumerator FlashingIconCoro(GameObject icon, float frequency) {
        while(true) {
            icon.SetActive(!icon.activeSelf);
            yield return new WaitForSeconds(frequency);
        }
    }

    private void StopFlashingIcon() {
        _progressBar.WarningSign.SetActive(false);
        _progressBar.Checkmark.SetActive(false);

        if(_currentFlashingType != FlashingType.None) {
            StopCoroutine(_flashingCoroutine);
            _currentFlashingType = FlashingType.None;
        }
    }

    /// <summary>
    /// Sets the flashing icon based on the flashing type
    /// </summary>
    /// <param name="flashingType"></param>
    private void SetFlashingCoroutine(FlashingType flashingType){
        // Make sure all icons are off, cause we only want one to be on at a time
        _progressBar.WarningSign.SetActive(false); 
        _progressBar.Checkmark.SetActive(false);

        // Don't do anything if it is already flashing the correct icon
        if(_currentFlashingType != flashingType) {
            // Stop the wrong coroutine if it is running
            if(_flashingCoroutine != null) { StopCoroutine(_flashingCoroutine); } 

            // Select the correct icon and frequency, then start the coroutine
            _currentFlashingType = flashingType;
            switch(flashingType) {
                case FlashingType.CheckMark:
                    _flashingCoroutine = StartCoroutine(FlashingIconCoro(_progressBar.Checkmark, _slowFreq));
                    break;
                case FlashingType.WarningSlow:
                    _flashingCoroutine = StartCoroutine(FlashingIconCoro(_progressBar.WarningSign, _slowFreq));
                    break;
                case FlashingType.WarningQuick:
                    _flashingCoroutine = StartCoroutine(FlashingIconCoro(_progressBar.WarningSign, _quickFreq));
                    break;
                case FlashingType.None:
                    break;
            }
        }
    }
}
