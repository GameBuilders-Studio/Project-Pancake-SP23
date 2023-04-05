using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform pot;
    private Stove[] stoveInScene; //Make an array storing all the pots that are in the scene
    
    private Vector3 originalPos; //This is to store the original position 

    //IMPLEMENTATION:
    //Create an array of the stoves that are in the scene
    //When a pot falls down, we will go through the array of stoves and see if something is on top of it
    //If a stove is being occupied, we skip until the next one. If the stove is empty, we store the fallen pot 
    //on the empty stove 

    void Awake()
    {
        //stoveInScene = FindGameObjectsWithTag("Stove");
        stoveInScene = GameObject.FindObjectsOfType<Stove>();

        originalPos = pot.transform.position;
    }

    //When the player or pot hits the death trigger, this function will be used
    void OnTriggerEnter(Collider other)
    {
        //If the player hits the death trigger, 
        if(other.tag == "Player")
        {
            StartCoroutine(RespawnTime());

            //Drop the pot that the player is holding and respawn it to a stove top

        }

        //If the pot hits the death triggere
        if(other.tag == "Pot")
        {

            /*
            //use GameObject.FindGameObjectWithTag("Your_Tag_Here").transform.position; if this is equal to current position of pot that needs to respawns,
            move the pot to new position and check again is position is avaiable
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
        //pot.transform.position = originalPos;
    }
}