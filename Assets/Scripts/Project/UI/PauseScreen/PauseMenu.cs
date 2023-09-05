using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using CustomAttributes;

//PlayerInputActions.IInGameUIActionActions

public class PauseMenu : MonoBehaviour, PlayerInputActions.IInGameUIActionActions
{
    public GameObject pauseMenu;
    private bool isPaused = false;
    private PlayerInputHandler[] _playerInputHandlers;

    private void Start() {
        _playerInputHandlers = FindObjectsOfType<PlayerInputHandler>();
        pauseMenu.SetActive(false);

        foreach(var playerInputHandler in _playerInputHandlers)
        {
            playerInputHandler.InputActionsAssigned += OnPlayerJoin;
            playerInputHandler.DeviceReassigned += OnPlayerJoin;
            playerInputHandler.DeviceLost += OnPlayerLost;
        }
    }

    void OnDisable()
    {
        foreach (var playerInputHandler in _playerInputHandlers)
        {
            playerInputHandler.InputActionsAssigned -= OnPlayerJoin;
            playerInputHandler.DeviceReassigned -= OnPlayerJoin;
            playerInputHandler.DeviceLost -= OnPlayerLost;
        }
    }

    private void OnPlayerJoin(PlayerInputHandler inputHandler)
    {
        inputHandler.SetCallbacks(this);
    }

    private void OnPlayerLost(PlayerInputHandler inputHandler)
    {
        inputHandler.SetCallbacks(null as PlayerInputActions.IInGameUIActionActions);
    }

    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OnClickResumeBtn()
    {
        ResumeGame();
    }

    public void OnClickMainMenuBtn()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
        isPaused = false;
    }

    public void OnClickQuitBtn()
    {
        Application.Quit();
    }

    public void OnClickRestartBtn()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        // Debug.Log("Pause button pressed");
        if (context.performed)
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }
}
