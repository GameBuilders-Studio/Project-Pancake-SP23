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

    void Awake()
    {
        FindAllSpawnPoints();
        if (_forceSpawn)
        {
            ForceSpawnPlayers();
        }
        else
        {
            SpawnPlayers();
        }
    }

    /// <summary>
    /// Spawns players at each spawn point if there are enough input devices.
    /// </summary>
    public void SpawnPlayers()
    {
        for (int i = 0; i < InputManager.PlayerCount; i++)
        {
            _spawnPoints[i].SpawnPlayer(i, _playerPrefabs[i]);
        }
    }

    /// <summary>
    /// For debugging purposes. It will spawn players at each spawn point even if there are no input devices.
    /// </summary>
    [Button]
    public void ForceSpawnPlayers()
    {
        for (int i = 0; i < _spawnPoints.Count; i++)
        {
            _spawnPoints[i].SpawnPlayer(i, _playerPrefabs[i]);
        }
    }

    /// <summary>
    /// Fetches all spawn points in the scene
    /// </summary>
    [Button]
    private void FindAllSpawnPoints()
    {
        _spawnPoints = new List<PlayerSpawnPoint>(FindObjectsOfType<PlayerSpawnPoint>());
    }
}
