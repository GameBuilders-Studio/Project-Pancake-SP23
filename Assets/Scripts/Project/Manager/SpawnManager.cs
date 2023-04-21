using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private List<PlayerSpawnPoint> _spawnPoints;

    [SerializeField]
    private List<GameObject> _playerPrefabs;

    [SerializeField]
    private bool _forceSpawn = false;

    void Start()
    {
        if (_forceSpawn) 
        {
            ForceSpawnPlayers(); 
        }
        else
        {
            SpawnPlayers();
        }
    }

    public void SpawnPlayers()
    {
        for (int i = 0; i < InputManager.PlayerCount; i++)
        {
            _spawnPoints[i].SpawnPlayer(i, _playerPrefabs[i]);
        }
    }

    public void ForceSpawnPlayers()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i].SpawnPlayer(i, _playerPrefabs[i]);
        }
    }

    [Button]
    private void FindAllSpawnPoints()
    {
        _spawnPoints = new List<PlayerSpawnPoint>(FindObjectsOfType<PlayerSpawnPoint>());
    }
}
