using UnityEngine;

// This class exists solely to transport data from one level to another
// scripts have almost singleton-like access by finding by type

public class DataCapsule : MonoBehaviour
{
    public static DataCapsule instance {
        get
        {
            if (_instance == null)
            {
                var newCapsule = new GameObject();
                newCapsule.gameObject.name = "Capsule";
                _instance = newCapsule.AddComponent<DataCapsule>();
            }

            return _instance;

        }
        private set { _instance = value; }
    }

    private static DataCapsule _instance;

    private void Awake()
    {
        // someone else has already made another capsule
        // self destruct
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        // otherwise this is the instance
        _instance = this;
        DontDestroyOnLoad(this);
    }

    public string lastLevel;
    public int score;
}
