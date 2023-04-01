using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectionController : MonoBehaviour
{
    [SerializeField] 
    private List<SceneReference> _scenes;
    [SerializeField] 
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonParent;
    [SerializeField]
    private List<GameObject> _buttons;
    private void Awake() {
        int index = 1;
        foreach (var scene in _scenes)
        {
            var button = Instantiate(_buttonPrefab, _buttonParent);
            button.GetComponentInChildren<Text>().text = index.ToString();
            button.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene(scene));
            _buttons.Add(button);
            index++;
        }
    }
}
