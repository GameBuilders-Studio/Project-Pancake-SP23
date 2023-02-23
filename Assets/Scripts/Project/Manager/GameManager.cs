using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public GameState state = GameState.Menu;
    public GameState State
    {
        get { return state; }
    }
    public static int stageNum;
    public List<string> sceneNameList = new List<string>();
    private static bool haveDone = false;

    void Start(){
        if(haveDone){
            return;
        }
        EventManager.AddListener("StartMenu", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[0]))); //When you first enter into the game
        EventManager.AddListener("StartTutorial", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[1]))); //When you press the start button
        EventManager.AddListener("RoundStart", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[2]))); //When you press the start button
        EventManager.AddListener("GameWon", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[3]))); //When you press the start button
        EventManager.AddListener("GameLost", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[4]))); //When you press the start button
        EventManager.AddListener("ChangeState", new UnityAction(()=>Debug.Log("ChangeState")));//when you change the states'
        EventManager.AddListener("Interm1", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[5])));
        EventManager.AddListener("Interm2", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[6])));
        EventManager.AddListener("WinInterim", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[7])));
        EventManager.AddListener("LoseInterim", new UnityAction(()=>SceneManager.LoadScene(Instance.sceneNameList[8])));
        haveDone = true;
        // ChangeState(GameState.Menu);  // TODO: uncomment in final build
    }

    public static void ChangeState(string str){
        switch(str){
            case "Menu":
    
                ChangeState(GameState.Menu);
                break;
            case "Tutorial":
                ChangeState(GameState.InGame);
                break;

        }
    }

    public static void ChangeState(GameState newState){
        switch(newState){
            case GameState.Menu:
    
               
                break;
            case GameState.InGame:
                break;

        }
    }

    private static void OnStateChanged()
    {
        EventManager.Invoke("ChangeState");
        return;
    }

}

public enum GameState
{
    Menu, //When you are in the menu
    InGame, //When you are in the game

}
