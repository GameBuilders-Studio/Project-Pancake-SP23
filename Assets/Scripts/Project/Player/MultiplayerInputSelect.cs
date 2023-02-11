using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerInputSelect : MonoBehaviour
{
    public List <GameObject> playerList;
    
    // Start is called before the first frame update
    void Start()
    {
        playerList = new List<GameObject>(Resources.LoadAll<GameObject>("Player"));

        int playerTotal = 0;

        //if the player presses a key, we instaniate, and move position in List
        if(playerTotal < playerList.Count && (Input.GetKeyDown(KeyCode.Space) /* || get gamepad button is pressed*/))
        { 
            Instantiate(playerList[playerTotal], new Vector3(0,0,0), Quaternion.identity);
            playerTotal++; //if we select the player 1 and want player 2, we will move to the next character  
        }
    }
}
