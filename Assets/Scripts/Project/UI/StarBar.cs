using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarBar : MonoBehaviour
{
    [SerializeField] private List<GameObject> stars;
    private List<Material> _mats;
    private float _currentFraction = 0.0f;
    private float _targetFraction = 0.0f;

    [SerializeField]
    private float _fillSpeed = 0.1f;


    private void Awake()
    {
        _mats = new List<Material>();
        foreach (var star in stars)
        {
            var renderer = star.GetComponent<RawImage>();
            renderer.material = Instantiate(renderer.material);
            _mats.Add(renderer.material);
        }
    }

    private void Update()
    {
        var fraction0 = _currentFraction < 0.333f
            ? _currentFraction * 3f : 1f;
        var fraction1 = _currentFraction < 0.333f ? 0 : (_currentFraction > 0.667f ? 1 : (_currentFraction - 0.333f) * 3);
        var fraction2 = _currentFraction > 0.667f ? (_currentFraction - 0.667f) * 3 : 0f;

        _mats[0].SetFloat("_Fraction", fraction0);
        _mats[1].SetFloat("_Fraction", fraction1);
        _mats[2].SetFloat("_Fraction", fraction2);
        // Debug.Log($"Fractions: {fraction0}, {fraction1}, {fraction2}");
    }

    private IEnumerator SlideProgressCoro(float targetFrac)
    {
        while (_currentFraction < targetFrac)
        {
            _currentFraction += _fillSpeed * Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Set the target fraction to the given value [0, 1f] 
    /// </summary>
    /// <param name="fraction"></param>
    public void SetTargetFraction(float fraction)
    {
        _targetFraction = fraction;
        StartCoroutine(SlideProgressCoro(MathF.Min(_targetFraction, 1f))); // The maximum progess is 100%
    }

    /// <summary>
    /// Makes the stars start filling up left to right from 0 to the target fraction
    /// </summary>
    public void PlayStarAnimation()
    {
        StartCoroutine(SlideProgressCoro(_targetFraction));
    }



}
