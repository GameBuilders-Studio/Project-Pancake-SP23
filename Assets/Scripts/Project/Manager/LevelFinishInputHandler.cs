using UnityEngine;

public class LevelFinishInputHandler : MonoBehaviour
{
    [SerializeField] private SceneLoader _sceneLoader;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Here");
            _sceneLoader.LoadScene();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _sceneLoader.LoadSceneString(DataCapsule.instance.lastLevel);
        }
    }
}
