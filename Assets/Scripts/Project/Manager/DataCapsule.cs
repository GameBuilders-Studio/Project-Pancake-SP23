using UnityEngine;
using GameBuilders.Singleton;

// This class exists solely to transport data from one level to another
// scripts have almost singleton-like access by finding by type

public class DataCapsule : Singleton<DataCapsule>
{
    public string lastLevel;
    // the score that is acquired by the player in the previous level
    public int baseScore;
    // the score needed to get three stars `
    public int scoreBarMax;
    // Score from finishing order early
    public int bonusScore; 
    // Score deducted from missing orders
    public int scoreDeduction;
    // Total score for the level
    public int totalScore;
}
