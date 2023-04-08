using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//PlayerInputActions.IInGameUIActionActions

public class PausedMenu : MonoBehaviour
{ 
    public GameObject pauseMenu;
    public static bool isPaused = false;

    //private PlayerInput playerInput;

    private PlayerInputActions player1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("1");
        pauseMenu.SetActive(false);
        //if (InputManager.GetInputActionsByIndex(0, out player1) != true)
        //{
        //    Debug.Log("2null");
        //}
        //else
        //{
        //    Debug.Log("00");
        //}

        //player1.InGameUIAction.SetCallbacks(this);

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

    public void OnPaused()
    {
        Debug.Log("2");
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
