using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//PlayerInputActions.IInGameUIActionActions

public class PausedMenu : MonoBehaviour, PlayerInputActions.IInGameUIActionActions
{
    public GameObject pauseMenu;
    public static bool isPaused = false;

    [SerializeField]
    private PlayerInputHandler _playerInputHandler;

    private PlayerInputActions player1;

    // Start is called before the first frame update
    void Start()
    {
        pauseMenu.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (isPaused)
        //    {
        //        ResumeGame();
        //    }
        //    else
        //    {
        //        PauseGame();
        //    }
        //}
    }

    void OnEnable()
    {
        _playerInputHandler.InputActionsAssigned += OnPlayerJoin;
        _playerInputHandler.DeviceReassigned += OnPlayerJoin;
        _playerInputHandler.DeviceLost += OnPlayerLost;
    }

    void OnDisable()
    {
        _playerInputHandler.InputActionsAssigned -= OnPlayerJoin;
        _playerInputHandler.DeviceReassigned -= OnPlayerJoin;
        _playerInputHandler.DeviceLost -= OnPlayerLost;
    }

    private void OnPlayerJoin()
    {
        _playerInputHandler.SetCallbacks(this);
    }

    private void OnPlayerLost()
    {
        _playerInputHandler.SetCallbacks(null as PlayerInputActions.IInGameUIActionActions);
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

    public void OnPause(InputAction.CallbackContext context)
    {
        Debug.Log("3");
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
