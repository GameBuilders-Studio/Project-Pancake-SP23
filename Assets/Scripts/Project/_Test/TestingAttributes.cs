using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public enum MyFlags
{
	None = 0,
	Right = 1 << 0,
	Left = 1 << 1,
	Up = 1 << 2,
	Down = 1 << 3,
}

public class TestingAttributes : MonoBehaviour
{
    [MinMaxSlider(0f, 1f)]
    public Vector2 sliderValueOne;

    [MinMaxSlider(0, 1)]
    public Vector2 sliderValueTwo;

    [Button("Make fart noise")]
    private void MakeFartNoise() 
    {
        Debug.Log("brap");
    }

    [CurveRange(0f, 0f, 1f, 1f, EColor.Red)]
    public AnimationCurve curve;

    [EnumFlags]
	public MyFlags flags;
}