using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform PotRespawnPoint;
    [SerializeField] private PlayerInteraction playerHands; //used for the player to let go of the object if currently holding.
    [SerializeField] private List<Station> stovesInScene = new List<Station>(); //Make an array storing all the stoves that are in the scene

    //IMPLEMENTATION:
    //Create an array of the stoves that are in the scene - DONE
    //When a pot falls down, we will go through the array of stoves and see if something is on top of it
    //If a stove is being occupied, we skip until the next one. If the stove is empty, we store the fallen pot 
    //on the empty stove           

    void Awake()
    {
        Object[] stoves = GameObject.FindObjectsOfType(typeof(Stove));
        foreach (Stove obj in stoves)
        {
            Station station = obj.GetComponent<Station>();
            if (station == null)
            {
                Debug.LogError("Every Stove should have Station component!");
            }
            stovesInScene.Add(station);
        }

        Debug.Log("STOVES IN SCENE COUNT: " + stovesInScene.Count);
    }

    //When the player or pot hits the death trigger, this function will be used
    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Object entered triggerbox");
        //If the player hits the death trigger, 
        if (other.tag == "Player")
        {
            StartCoroutine(RespawnTime());

            //Drop the pot that the player is holding and respawn it to a stove top
            Debug.Log("Error");
            //playerHands.HandsEmpty();
            Debug.Log("Check");
        }

        //If the pot hits the death triggeer
        if (other.tag == "Pot")
        {
            Debug.Log("Pot entered triggerbox");
            Carryable carryable = other.GetComponent<Carryable>();
            if (carryable == null)
            {
                Debug.LogError("Objects with Pot tag should have Carryable component");
            }
            Debug.Log(stovesInScene.Count);
            //We will iterate through each of the stoves in the scene
            foreach (Station stoves in stovesInScene)
            {
                //if a stove is not occupied, put the pot on that empty stove
                if (stoves.PlacedItem == null) //EMPTY
                {
                    // Check if station has a placed item using the public property 
                    Debug.Log("Pot moved to stove");
                    stoves.PlaceItem(carryable);
                    return;
                }
            }

            //Moves the pot to position if all stoves are taken
            carryable.transform.position = PotRespawnPoint.transform.position;
        }
    }

    //Used as a timer so the Player must wait 5 seconds to respawn
    IEnumerator RespawnTime()
    {
        yield return new WaitForSeconds(1); //wait 5 seconds to respawn the character
        Debug.Log("Player Respawned");
        player.transform.position = respawnPoint.transform.position; //Set them to the respawnPoint
    }
}