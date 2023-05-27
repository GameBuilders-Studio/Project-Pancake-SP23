using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField]
    private SceneReference _sceneToLoad;

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
        SceneManager.LoadScene(scenePath);
    }

}
