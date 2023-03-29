using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform pot;

    private Vector3 originalPos; //This is to store the original position 

    void Awake()
    {
        //if(gameObject.tag == "Pot"){
            originalPos = pot.transform.position;
            Debug.Log(originalPos);
        //}
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            //player.transform.position = respawnPoint.transform.position;
            StartCoroutine(RespawnTime());
        }

        if(other.tag == "Pot")
        {
            //LET GO FROM PLAYER
            /*
            //if() if original position is being taken, check if space next to is open, if so then place object there
            IDEAS: use vector3 distance seeing is the distance between the pots is ~0, is so, move the point to the next position
            {

            }
            */
            pot.transform.position = originalPos;
        }
    }

    //Used as a timer so the Player must wait 5 seconds to respawn
    IEnumerator RespawnTime() 
    {
        yield return new WaitForSeconds(1); //wait 5 seconds to respawn the character
        Debug.Log("Player Respawned"); 
        player.transform.position = respawnPoint.transform.position; //Set them to the respawnPoint
        pot.transform.position = originalPos;
    }
}

