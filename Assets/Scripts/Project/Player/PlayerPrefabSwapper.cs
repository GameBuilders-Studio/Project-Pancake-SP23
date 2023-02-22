using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 

public class PlayerPrefabSwapper: MonoBehaviour
{
    [SerializeField] private List <GameObject> _playerList;
    [SerializeField] private PlayerInputManager _playerInputManager; 

    //if(_playerInputManager.playerCount >= _playerList.Count)
    public void OnPlayerJoined() 
    {
        if(_playerList.Count > _playerInputManager.playerCount)
        {
            _playerInputManager.playerPrefab = _playerList[_playerInputManager.playerCount];
        }
        
        if(_playerInputManager.playerCount > _playerList.Count + 1) //This should never happen, _playerList is out of players
        { 
            Debug.LogError("_playerList ran out of players, size of _playerList: " + _playerList.Count + ". Size of playerCount: " + _playerInputManager.playerCount); //This should never come
            return; // Don't set new player prefab if there aren't any left
        }
    }
}
