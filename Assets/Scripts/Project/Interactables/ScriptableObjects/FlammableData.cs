using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/Flammable Data")]
public class FlammableData : ScriptableObject
{
    [SerializeField]
    public GameObject FirePrefab;

    [SerializeField]
    public float SpreadIntervalSeconds = 3.0f;

    [SerializeField]
    public float SpreadRadius = 0.6f;

    [SerializeField]
    public float FireInitialHealth = 100.0f;
}
