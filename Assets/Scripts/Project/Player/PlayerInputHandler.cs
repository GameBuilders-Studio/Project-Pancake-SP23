using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    public int PlayerIndex = 0;

    [Tooltip("Use any connected device available to control this player. For testing purposes only!")]
    [SerializeField]
    private bool _useAnyDevice = false;

    private bool _isManaged = false;
    private PlayerInputActions _actions;
    private Vector2 _moveInput = Vector2.zero;

    public bool HasActions => _actions != null;
    public Vector2 MoveInput => _moveInput;
    
    public event UnityAction ActionsAssigned;

    void Update()
    {
        if (!HasActions)
        {
            if (InputManager.GetInputActionsByIndex(PlayerIndex, out _actions))
            {
                _isManaged = true;
                OnActionsChanged();
            }

            if (!_isManaged)
            {
                _useAnyDevice = true;
                Debug.LogWarning($"No actions assigned to Player {PlayerIndex}! Using any connected device");
            }

            if (_useAnyDevice)
            {
                _actions = InputManager.GetInputActionsAllDevices();
                OnActionsChanged();
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

    private void OnActionsChanged()
    {
        Debug.Log($"Assigned input to Player {PlayerIndex}");
        ActionsAssigned?.Invoke();
    }
}
