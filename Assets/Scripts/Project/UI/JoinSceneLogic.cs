using System.Collections.Generic;
using CustomAttributes;
using UnityEngine;
using UnityEngine.Events;

public class JoinSceneLogic : MonoBehaviour
{
    [SerializeField] private List<PlayerJoinPanel> _playerJoinPanels = new List<PlayerJoinPanel>();
    [SerializeField, Required] private SceneLoader _sceneLoader = null;
    public void OnFinish()
    {
        // Check if there is at least one player 
        int playerCount = 0;
        foreach (PlayerJoinPanel panel in _playerJoinPanels)
        {
            if (panel.PlayerJoined)
            {
                playerCount++;
            }
        }

        if (playerCount == 0)
        {
            Debug.LogWarning("No players joined");
            return;
        }

        // Check if all players are ready
        foreach (PlayerJoinPanel panel in _playerJoinPanels)
        {
            if (panel.PlayerJoined && !panel.IsReady)
            {
                Debug.LogWarning("Not all players are ready");
                return;
            }
        }

        // Start the game
        _sceneLoader.LoadScene();
    }
}
