using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("GameObject/UI/Progress Bar")]
    public static void AddLinearProgressBar()
    {
        GameObject obj = Instantiate(Resources.Load<GameObject>("Progress Bar"));
        Debug.Log(obj);
        obj.transform.SetParent(Selection.activeGameObject.transform, false);
    }
#endif
    [SerializeField] private Image _border;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fill;
    [SerializeField] private Gradient _gradient;


    public void SetProgress(float value)
    {
        if (value > _slider.maxValue || value < _slider.minValue)
        {
            Debug.LogWarning("Progress Bar: Value is out of range");
            return;
        }

        _slider.value = value;
        _fill.color = _gradient.Evaluate(_slider.normalizedValue);
    }

    public float GetProgress()
    {
        return _slider.value;
    }

    public void SetMaxValue(float maxValue)
    {
        _slider.maxValue = maxValue;
        _slider.value = maxValue;

        _fill.color = _gradient.Evaluate(1f);
    }

}
