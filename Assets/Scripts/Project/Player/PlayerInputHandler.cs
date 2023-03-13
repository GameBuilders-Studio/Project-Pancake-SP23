using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Users;

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    public int PlayerIndex = 0;

    [Tooltip("Use any connected device available to control this player. For testing purposes only!")]
    [SerializeField]
    public bool UseAnyDevice = false;

    public PlayerInputActions Actions;

    public UnityAction OnActionsAssigned;

    void Start()
    {
        if (UseAnyDevice)
        {
            Actions = GetAllDevicesInputActions();
            OnActionsAssigned?.Invoke();
        }
    }

    void Update()
    {
        if (Actions == null)
        {
            if (GameInputManager.GetInputActions(PlayerIndex, out Actions))
            {
                Debug.Log($"Assigned input to player {PlayerIndex}");
                OnActionsAssigned?.Invoke();
            }
        }
    }

    private PlayerInputActions GetAllDevicesInputActions()
    {
        var user = new InputUser();
        user = GameInputManager.AddAllDevicesToUser(user);

        var actions = new PlayerInputActions();
        user.AssociateActionsWithUser(actions);
        actions.Enable();

        return actions;
    }
}
