using System.Data.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private SceneReference _sceneToLoad;

    [SerializeField]
    private string _lastSceneName; 

    public void SetSceneToLoad(SceneReference sc)
    {
        _sceneToLoad = sc;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(_sceneToLoad);
    }

    public void LoadSceneString(string scenePath)
    {
        Scene scene = SceneManager.GetSceneByName(scenePath);
        if (scene.IsValid())
        {
            SceneManager.LoadScene(scenePath);
        }
        else
        {
            Debug.LogError("Scene " + scenePath + " not found!");
        }
        SceneManager.LoadScene(scenePath);
    }

    public void LoadLastLevel()
    {
        // Check if scene name is set in the DataCapsule 
        if (DataCapsule.Instance.lastLevel == null)
        {
            Debug.LogError("No last level found");
            return;
        } else {
            LoadSceneString(DataCapsule.Instance.lastLevel);
        }
    }
}
