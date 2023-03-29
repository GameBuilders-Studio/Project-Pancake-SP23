using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform pot;
    [SerializeField] private GameObject[] pots; //Make an array storing all the pots that are in the scene

    private Vector3 originalPos; //This is to store the original position 

    void Awake()
    {
        //Store all the pots with the tag "Pot" inside the array pots
        pots = GameObject.FindGameObjectsWithTag("Pot");

        //make an array that stores all the original positions of all the pots that are in the scene
        originalPos = pot.transform.position;
        Debug.Log(originalPos);
    }

    //When the player or pot hits the death trigger, this function will be used
    void OnTriggerEnter(Collider other)
    {
        //If the player hits the death trigger, 
        if(other.tag == "Player")
        {
            StartCoroutine(RespawnTime());
        }

        //If the pot hits the death trigger
        if(other.tag == "Pot")
        {
            //The pot needs to be let go from the player

            /*
            //if() if original position is being taken, check if space next to is open, if so then place object there
            IDEAS: use vector3 distance seeing is the distance between the pots is ~0, 
            this means that another pot is in another pots original postion is so, move the point to the next position using Vector3
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

