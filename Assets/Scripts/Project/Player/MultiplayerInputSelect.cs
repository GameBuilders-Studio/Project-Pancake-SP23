using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class MultiplayerInputSelect : MonoBehaviour
{
    [SerializeField] private List <GameObject> _playerList;
    [SerializeField] private PlayerInputManager _playerInputManager; 

    public void OnPlayerJoined() 
    {
        if(_playerInputManager.playerCount > _playerList.Count) return; // Don't set new player prefab if there aren't any left

        _playerInputManager.playerPrefab = _playerList[_playerInputManager.playerCount]; 
    }
}
