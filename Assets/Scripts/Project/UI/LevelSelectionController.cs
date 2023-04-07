using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelSelectionController : MonoBehaviour, PlayerInputActions.IUIActions
{
    [SerializeField]
    private List<SceneReference> _scenes;
    [SerializeField]
    private GameObject _buttonPrefab;
    [SerializeField]
    private Transform _buttonParent;
    [SerializeField]
    private List<GameObject> _buttons;
    [SerializeField]
    private PlayerInputHandler _playerInputHandler;
    [SerializeField]
    private Color _selectedColor;
    [SerializeField]
    private int _selectIndex = 0;
    private bool _isSubmit = false;
    public int SelectIndex{
        get => _selectIndex;
        set
        {
            if (value < 0)
            {
                _selectIndex = _buttons.Count - 1;
            }
            else if (value > _buttons.Count - 1)
            {
                _selectIndex = 0;
            }
            else
            {
                _selectIndex = value;
            }
        }
    }
    private void Awake()
    {
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

    void OnEnable()
    {
        _playerInputHandler.InputActionsAssigned += OnPlayerJoin;
        _playerInputHandler.DeviceReassigned += OnPlayerJoin;
        _playerInputHandler.DeviceLost += OnPlayerLost;
    }

    void OnDisable()
    {
        _playerInputHandler.InputActionsAssigned -= OnPlayerJoin;
        _playerInputHandler.DeviceReassigned -= OnPlayerJoin;
        _playerInputHandler.DeviceLost -= OnPlayerLost;
    }

    private void OnPlayerJoin()
    {
        Debug.Log("Player joined");
        _playerInputHandler.SetCallbacksUI(this);
    }

    private void OnPlayerLost()
    {
        Debug.Log("Player lost");
    }

    void PlayerInputActions.IUIActions.OnNavigate(InputAction.CallbackContext context)
    {
        _buttons[SelectIndex].GetComponent<Image>().color = Color.white;
        var direction = context.ReadValue<Vector2>();
        if (direction.x > 0)
        {
            SelectIndex--;

        }
        else if (direction.x < 0)
        {
            SelectIndex++;
        }
        _buttons[SelectIndex].GetComponent<Image>().color = _selectedColor;
        //Debug.Log("direction: " + direction.x + " " + direction.y);

    }
    void PlayerInputActions.IUIActions.OnSubmit(InputAction.CallbackContext context)
    {
        if(_isSubmit) return;
        _buttons[SelectIndex].GetComponent<Button>().onClick.Invoke();
        _isSubmit = true;
    }
    void PlayerInputActions.IUIActions.OnCancel(InputAction.CallbackContext context)
    {
        _buttons[SelectIndex].GetComponent<Image>().color = Color.white;
        SelectIndex = 0;
    }
}
