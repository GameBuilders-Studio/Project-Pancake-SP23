using UnityEngine;

// This class exists solely to transport data from one level to another
// scripts have almost singleton-like access by finding by type

public class DataCapsule : MonoBehaviour
{
    public static DataCapsule instance {
        get
        {
            if (s_instance == null)
            {
                var newCapsule = new GameObject();
                newCapsule.gameObject.name = "Capsule";
                s_instance = newCapsule.AddComponent<DataCapsule>();
            }

            return s_instance;

        }
        private set { s_instance = value; }
    }

    private static DataCapsule s_instance;

    private void Awake()
    {
        // someone else has already made another capsule
        // self destruct
        if (s_instance != null)
        {
            Destroy(gameObject);
        }
        // otherwise this is the instance
        s_instance = this;
        DontDestroyOnLoad(this);
    }

    public string lastLevel;
    // the score that is acquired by the player in the previous level
    public int score;
    // the score needed to get three stars `
    public int scoreBarMax;
}
