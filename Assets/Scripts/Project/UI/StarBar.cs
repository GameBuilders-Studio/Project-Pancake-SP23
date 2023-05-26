using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StarBar : MonoBehaviour
{
    [SerializeField] private List<GameObject> stars;
    private List<Material> _mats;
    private float _fullFraction = 0.0f;
    private float _targetFraction = 0.0f;


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

    private void Start()
    {
        StartCoroutine(SlideProgressCoro(1.0f));
    }

    private void Update()
    {
        var fraction0 = _fullFraction < 0.333f
            ? _fullFraction * 3f : 1f;
        var fraction1 = _fullFraction < 0.333f ? 0 : (_fullFraction > 0.667f ? 1 : (_fullFraction - 0.333f) * 3);
        var fraction2 = _fullFraction > 0.667f ? (_fullFraction - 0.667f) * 3 : 0f;

        _mats[0].SetFloat("_Fraction", fraction0);
        _mats[1].SetFloat("_Fraction", fraction1);
        _mats[2].SetFloat("_Fraction", fraction2);
    }

    private IEnumerator SlideProgressCoro(float targetFrac)
    {
        while (_fullFraction < targetFrac)
        {
            _fullFraction += 0.2f * Time.deltaTime;
            yield return null;
        }
    }



}
