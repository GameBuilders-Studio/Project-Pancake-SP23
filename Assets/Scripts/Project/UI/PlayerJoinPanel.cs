using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
//using UnityEngine.InputSystem;

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
        _title.text = $"Player {_playerInputHandler.PlayerIndex}";
        _buttonPrompt.enabled = false;
    }

    private void OnPlayerLost()
    {
        _playerVisual.SetActive(false);
        _title.text = "???";
        _buttonPrompt.enabled = true;
    }

    // handle UI navigation here
    public void OnNavigate(InputAction.CallbackContext context)
    {

    }

    public void OnSubmit(InputAction.CallbackContext context)
    {

    }

    public void OnCancel(InputAction.CallbackContext context)
    {

    }
}
