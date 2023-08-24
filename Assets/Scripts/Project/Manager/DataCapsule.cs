using UnityEngine;
using GameBuilders.Singleton;

// This class exists solely to transport data from one level to another
// scripts have almost singleton-like access by finding by type

public class DataCapsule : Singleton<DataCapsule>
{
    public string lastLevel;
    // the score that is acquired by the player in the previous level
    public int score;
    // the score needed to get three stars `
    public int scoreBarMax;
}
