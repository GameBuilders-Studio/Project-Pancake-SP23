using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    private bool _logStatus = false;

    [SerializeField]
    private bool _enablePairingOnStart = false;

    private static List<InputUser> s_players = new();
    private static Dictionary<InputUser, PlayerInputActions> s_userToInputActions = new();

    public static event UnityAction<InputUser> OnPlayerJoin;
    public static event UnityAction<InputUser> OnDeviceLost;
    public static event UnityAction<InputUser> OnDeviceRegained;

    public static int PlayerCount => s_players.Count;

    void Awake()
    {
        InputUser.onUnpairedDeviceUsed += OnUnpairedDeviceUsed;
        InputUser.onChange += OnInputUserChange;
    }

    void Start()
    {
        if (_enablePairingOnStart)
        {
            EnablePairing();
        }
    }

    public void EnablePairing()
    {
        InputUser.listenForUnpairedDeviceActivity = 1;
        Log("PAIRING ENABLED");
    }

    public void DisablePairing()
    {
        InputUser.listenForUnpairedDeviceActivity = 0;
        Log("PAIRING DISABLED");

        // remove unpaired users
        foreach (var user in s_players)
        {
            if (user.pairedDevices.Count == 0)
            {
                RemovePlayer(user);
            }
        }
    }

    public static bool GetInputActionsByIndex(int playerIndex, out PlayerInputActions inputActions)
    {
        if (playerIndex > s_players.Count - 1)
        {
            inputActions = null;
            return false;
        }
        var user = s_players[playerIndex];
        inputActions = s_userToInputActions[user];
        return true;
    }

    public void ReassignDevice(InputUser user, InputDevice newDevice)
    {
        user = InputUser.PerformPairingWithDevice(newDevice, user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);
        ConfigureControlScheme(user);
    }

    /// <summary>
    /// Adds all devices to a user and returns its associated PlayerInputActions. For testing purposes only! 
    /// </summary>
    public static PlayerInputActions GetInputActionsAllDevices()
    {
        var user = new InputUser();
        for (int i = 0; i < InputSystem.devices.Count; i++)
        {
            user = InputUser.PerformPairingWithDevice(InputSystem.devices[i], user);
        }

        var actions = new PlayerInputActions();
        user.AssociateActionsWithUser(actions);
        actions.Enable();

        return actions;
    }

    private void OnUnpairedDeviceUsed(InputControl control, InputEventPtr eventPtr)
    {
        // ignore any control that isn't a button
        if (control is not ButtonControl) { return; }

        // ignore unsupported devices
        if (control.device is not Keyboard && control.device is not Gamepad) { return; }

        // register device and add player
        AddPlayer(control.device);
    }

    private void OnInputUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        switch (change)
        {
            case InputUserChange.DeviceLost:
                OnDeviceLost?.Invoke(user);
                Log($"Lost device {device}");
                break;
            case InputUserChange.DeviceRegained:
                OnDeviceRegained?.Invoke(user);
                Log("device regained");
                break;
            default:
                break;
        }
    }

    private void AddPlayer(InputDevice device)
    {
        Log($"Added player with device {device.name}");

        var actions = new PlayerInputActions();
        var user = InputUser.PerformPairingWithDevice(device);

        user.AssociateActionsWithUser(actions);
        actions.Enable();

        ConfigureControlScheme(user);

        s_players.Add(user);
        s_userToInputActions.Add(user, actions);

        OnPlayerJoin?.Invoke(user);
    }

    private void ConfigureControlScheme(InputUser user)
    {
        var device = user.pairedDevices[0];
        if (device is Gamepad)
        {
            user.ActivateControlScheme("Gamepad");
        }
        else if (device is Keyboard)
        {
            user.ActivateControlScheme("Keyboard");
        }
    }

    private void RemovePlayer(InputUser user)
    {
        user.UnpairDevicesAndRemoveUser();
        s_players.Remove(user);
        s_userToInputActions.Remove(user);
    }

    private void Log(string s)
    {
        if (!_logStatus) { return; }
        Debug.Log(s);
    }
}