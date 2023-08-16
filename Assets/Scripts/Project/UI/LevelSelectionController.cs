using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using CustomAttributes;
using TMPro;

public class LevelSelectionController : MonoBehaviour, PlayerInputActions.IUIActions
{
    [SerializeField]
    private List<SceneReference> _scenes;
    [SerializeField]
    [Required]
    private GameObject _buttonPrefab;
    [SerializeField]
    [Required]
    private Transform _buttonParent;
    [SerializeField]
    private List<GameObject> _buttons;
    [SerializeField]
    [Required]
    private PlayerInputHandler _playerInputHandler;
    [SerializeField]
    private Color _selectedColor;
    [SerializeField]
    [Required]
    private GameObject _exitButton;
    [SerializeField]
    private int _selectedIndex = 0;
    private bool _isSubmit = false;
    private bool _isCancel = false;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            if (value < 0)
            {
                _selectedIndex = _buttons.Count - 1;
            }
            else if (value > _buttons.Count - 1)
            {
                _selectedIndex = 0;
            }
            else
            {
                _selectedIndex = value;
            }
        }
    }
    private void Start()
    {
        int index = 1;
        _isSubmit = false;
        _isCancel = false;
        foreach (var scene in _scenes)
        {
            var button = Instantiate(_buttonPrefab, _buttonParent);
            button.GetComponentInChildren<TextMeshProUGUI>().text = index.ToString();
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
        _playerInputHandler.SetCallbacksUI(null as PlayerInputActions.IUIActions);
        _playerInputHandler.InputActionsAssigned -= OnPlayerJoin;
        _playerInputHandler.DeviceReassigned -= OnPlayerJoin;
        _playerInputHandler.DeviceLost -= OnPlayerLost;
    }

    private void OnPlayerJoin()
    {
        //Debug.Log("Player joined");
        _playerInputHandler.SetCallbacksUI(this);
    }

    private void OnPlayerLost()
    {
        _playerInputHandler.SetCallbacksUI(null as PlayerInputActions.IUIActions);
        //Debug.Log("Player lost");
    }

    void PlayerInputActions.IUIActions.OnNavigate(InputAction.CallbackContext context)
    {
        _buttons[SelectedIndex].GetComponent<Image>().color = Color.white;
        var direction = context.ReadValue<Vector2>();
        if (direction.x > 0)
        {
            SelectedIndex--;

        }
        else if (direction.x < 0)
        {
            SelectedIndex++;
        }
        _buttons[SelectedIndex].GetComponent<Image>().color = _selectedColor;
        //Debug.Log("direction: " + direction.x + " " + direction.y);

    }
    void PlayerInputActions.IUIActions.OnSubmit(InputAction.CallbackContext context)
    {
        //Prevent multiple inputs
        if (_isSubmit) return;
        _buttons[SelectedIndex].GetComponent<Button>().onClick.Invoke();
        _isSubmit = true;
    }
    void PlayerInputActions.IUIActions.OnCancel(InputAction.CallbackContext context)
    {
        if(_isCancel) return;
        _buttons[SelectedIndex].GetComponent<Image>().color = Color.white;
        SelectedIndex = 0;
        _exitButton.GetComponent<Button>().onClick.Invoke();
        _isCancel = true;

    }
}
