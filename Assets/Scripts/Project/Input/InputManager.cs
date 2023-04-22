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

    private static bool s_isInitialized = false;

    private static List<InputUser> s_players = new();
    private static Dictionary<InputUser, PlayerInputActions> s_userToInputActions = new();

    public static event UnityAction<int, InputUser> PlayedJoined;
    public static event UnityAction<int, InputUser> DeviceLost;
    public static event UnityAction<int, InputUser> DeviceReassigned;

    public static int PlayerCount => s_players.Count;

    void Awake()
    {
        if (s_isInitialized) { return; }
        s_isInitialized = true;

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

    public static void EnablePairing()
    {
        InputUser.listenForUnpairedDeviceActivity = 1;
        Debug.Log("PAIRING ENABLED");
    }

    public static void DisablePairing()
    {
        InputUser.listenForUnpairedDeviceActivity = 0;
        Debug.Log("PAIRING DISABLED");

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
        if (playerIndex > s_players.Count - 1 || playerIndex < 0)
        {
            inputActions = null;
            return false;
        }
        var user = s_players[playerIndex];
        inputActions = s_userToInputActions[user];
        return true;
    }

    public static string GetControlSchemeByIndex(int playerIndex)
    {
        if (playerIndex > s_players.Count - 1 || playerIndex < 0)
        {
            return null;
        }
        var user = s_players[playerIndex];
        return user.controlScheme?.name;
    }

    public void ReassignDevice(InputUser user, InputDevice newDevice)
    {
        user = InputUser.PerformPairingWithDevice(newDevice, user, InputUserPairingOptions.UnpairCurrentDevicesFromUser);
        SetControlScheme(user);
        DeviceReassigned?.Invoke(UserIndex(user), user);
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
                DeviceLost?.Invoke(UserIndex(user), user);
                Log($"Lost device {device}");
                break;
            case InputUserChange.DeviceRegained:
                DeviceReassigned?.Invoke(UserIndex(user), user);
                Log("Device regained");
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

        SetControlScheme(user);

        s_players.Add(user);
        s_userToInputActions.Add(user, actions);

        PlayedJoined?.Invoke(UserIndex(user), user);
    }

    private void SetControlScheme(InputUser user)
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

    private static void RemovePlayer(InputUser user)
    {
        user.UnpairDevicesAndRemoveUser();
        s_players.Remove(user);
        s_userToInputActions.Remove(user);
    }

    private int UserIndex(InputUser user)
    {
        return s_players.FindIndex(x => x == user);
    }

    private void Log(string s)
    {
        if (!_logStatus) { return; }
        Debug.Log(s);
    }
}
