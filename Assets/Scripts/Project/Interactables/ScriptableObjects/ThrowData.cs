using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/ThrowData")]
public class ThrowData : ScriptableObject
{
    [SerializeField]
    public bool IsThrowable = true;

    [Tooltip("Path of the thrown object, where 1 = initial throw height and 0 = ground height")]
    [SerializeField]
    public AnimationCurve Trajectory;

    [SerializeField]
    public float Distance;

    [SerializeField]
    public float ThrowDurationSeconds;
}

