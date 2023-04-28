using UnityEngine.InputSystem.Users;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    public int PlayerIndex = 0;

    [SerializeField]
    public string CurrentControlScheme = "";

    [Tooltip("Use any connected device available to control this player. For testing purposes only!")]
    [SerializeField]
    private bool _useAnyDevice = false;

    private bool _isManaged = false;
    private bool _loggedUnmanagedWarning = false;
    private PlayerInputActions _actions;
    private Vector2 _moveInput = Vector2.zero;

    public bool HasActions => _actions != null;
    public Vector2 MoveInput => _moveInput;
    
    public event UnityAction InputActionsAssigned;
    public event UnityAction DeviceLost;
    public event UnityAction DeviceReassigned;

    void OnEnable()
    {
        InputManager.DeviceLost += OnDeviceLost;
        InputManager.DeviceReassigned += OnDeviceReassigned;
    }

    void OnDisable()
    {
        InputManager.DeviceLost -= OnDeviceLost;
        InputManager.DeviceReassigned -= OnDeviceReassigned;
    }

    void Update()
    {
        if (!HasActions)
        {
            if (InputManager.GetInputActionsByIndex(PlayerIndex, out _actions))
            {
                _isManaged = true;
                OnActionsAssigned();
            }

            if (!_isManaged && !_loggedUnmanagedWarning)
            {
                Debug.Log($"No actions assigned to Player {PlayerIndex}! For testing, set \"Use Any Device\" to true.");
                _loggedUnmanagedWarning = true;
            }

            if (_useAnyDevice)
            {
                _actions = InputManager.GetInputActionsAllDevices();
                OnActionsAssigned();
            }
        }
    }

    public void SetCallbacks(PlayerInputActions.IPlayerControlsActions instance)
    {
        if (_actions == null)
        {
            Debug.LogWarning("No input actions associated with this PlayerInputHandler");
            return;
        }
        _actions.PlayerControls.SetCallbacks(instance);
    }

    public void SetCallbacksUI(PlayerInputActions.IUIActions instance)
    {
        if (_actions == null)
        {
            Debug.LogWarning("No input actions associated with this PlayerInputHandler");
            return;
        }
        _actions.UI.SetCallbacks(instance);
    }

    public void SetCallbacks(PlayerInputActions.IInGameUIActionActions instance)
    {
        if (_actions == null)
        {
            Debug.LogWarning("No input actions associated with this PlayerInputHandler");
            return;
        }
        _actions.InGameUIAction.SetCallbacks(instance);
    }

    private void OnActionsAssigned()
    {
        Debug.Log($"Assigned input to Player {PlayerIndex}");
        CurrentControlScheme = InputManager.GetControlSchemeByIndex(PlayerIndex);
        InputActionsAssigned?.Invoke();
    }

    private void OnDeviceLost(int playerIndex, InputUser user)
    {
        DeviceLost?.Invoke();
    }

    private void OnDeviceReassigned(int playerIndex, InputUser user)
    {
        DeviceReassigned?.Invoke();
    }
}
