using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerJoinPanel : MonoBehaviour, PlayerInputActions.IUIActions
{
    [SerializeField]
    private PlayerInputHandler _playerInputHandler;

    [SerializeField]
    private GameObject _playerVisual;

    [Space(15f)]
    [SerializeField]
    private TextMeshProUGUI _title;

    [SerializeField]
    private TextMeshProUGUI _buttonPrompt;

    [Space(15f)]
    [SerializeField]
    private string _joinText;

    [SerializeField]
    private string _readyUpTextGamepad;

    [SerializeField]
    private string _readyUpTextKeyboard;

    void Awake()
    {
        OnPlayerLost();
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
        _playerVisual.SetActive(true);
        _playerInputHandler.SetCallbacks(this);

        _title.text = $"Player {_playerInputHandler.PlayerIndex}";

        if (_playerInputHandler.CurrentControlScheme == "Gamepad")
        {
            _buttonPrompt.text = _readyUpTextGamepad;
        }
        else
        {
            _buttonPrompt.text = _readyUpTextKeyboard; 
        }
    }

    private void OnPlayerLost()
    {
        _playerVisual.SetActive(false);

        _title.text = "";
        _buttonPrompt.text = _joinText;
    }

    // handle UI navigation here
    public void OnNavigate(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {

    }

    public void OnCancel(InputAction.CallbackContext context)
    {

    }
}
