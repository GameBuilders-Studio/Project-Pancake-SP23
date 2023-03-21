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

    public PlayerInputActions Actions;

    public event UnityAction ActionsAssigned;

    private Vector2 _moveInput = Vector2.zero;
    public Vector2 MoveInput => _moveInput;

    void Update()
    {
        if (Actions == null)
        {
            if (_useAnyDevice)
            {
                Actions = InputManager.GetInputActionsAllDevices();
                OnActionsChanged();
            }
            else if (InputManager.GetInputActionsByIndex(PlayerIndex, out Actions))
            {
                Debug.Log($"Assigned input to player {PlayerIndex}");
                OnActionsChanged();
            }
        }
    }

    public void SetCallbacks(PlayerInputActions.IPlayerControlsActions instance)
    {
        if (Actions == null) { return; }
        Actions.PlayerControls.SetCallbacks(instance);
    }

    private void OnActionsChanged()
    {
        ActionsAssigned?.Invoke();
    }
}
