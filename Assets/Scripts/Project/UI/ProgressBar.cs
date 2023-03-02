using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
public class ProgressBar : MonoBehaviour
{
    [SerializeField] Color BarColor;
    [SerializeField] Color BarBackGroundColor;

    [SerializeField] Image bar;
    [SerializeField] Image barBackground;

    private float val;
    public float Val
    {
        get { return val; }

        set
        {
            value = Mathf.Clamp(value, 0, 1);
            val = value;
            UpdateValue(val);

        }
    }
    
    [SerializeField] TMP_Text text;

    private void Awake()
    {
        bar.color = BarColor;
        barBackground.color = BarBackGroundColor;

        UpdateValue(val);
    }

    void UpdateValue(float val)
    {
        // text.text = $"{Mathf.Min(losingFarmValue, Mathf.RoundToInt(val * losingFarmValue))}/{losingFarmValue} PLANTED";  //| Hard-coded
        bar.fillAmount = val;
    }
}
