using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class OrderProgressBar : MonoBehaviour
{
    public int maximum;
    public int current;
    public Image mask;

    public void SetFill(float fillAmount)
    {
        mask.fillAmount = fillAmount;
    }

}
