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

    private string _readyUpText = "";
    private bool _playerJoined = false;
    public bool PlayerJoined => _playerJoined;
    private bool _isReady = false;
    public bool IsReady => _isReady;


    void Awake()
    {
        OnPlayerLost(_playerInputHandler);
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

    private void OnPlayerJoin(PlayerInputHandler inputHandler)
    {
        Debug.Log($"Player {_playerInputHandler.PlayerIndex} joined!");
        _playerJoined = true;
        _playerVisual.SetActive(true);
        _playerInputHandler.SetCallbacksUI(this);
        _title.text = $"Player {_playerInputHandler.PlayerIndex}";
        _readyUpText = _playerInputHandler.CurrentControlScheme == "Gamepad" ? _readyUpTextGamepad : _readyUpTextKeyboard;
        _buttonPrompt.text = _readyUpText;
    }

    private void OnPlayerLost(PlayerInputHandler inputHandler)
    {
        _playerJoined = false;
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
        ReadyUp();
    }

    public void OnCancel(InputAction.CallbackContext context)
    {
        Unready();
    }

    private void ReadyUp()
    {
        Debug.Log($"Player {_playerInputHandler.PlayerIndex} is ready!");
        if (!_isReady)
        {
            _isReady = true;
            _buttonPrompt.text = "Ready!";
        }
    }

    private void Unready()
    {
        Debug.Log($"Player {_playerInputHandler.PlayerIndex} is not ready!");
        if (_isReady)
        {
            _isReady = false;
            _buttonPrompt.text = _readyUpText;
        }
    }
}