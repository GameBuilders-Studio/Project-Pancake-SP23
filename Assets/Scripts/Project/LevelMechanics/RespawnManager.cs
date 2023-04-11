using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private Transform PotRespawnPoint; 
    [SerializeField] private PlayerInteraction playerHands; //used for the player to let go of the object if currently holding.
    private List<Station> stovesInScene = new List<Station>(); //Make an array storing all the stoves that are in the scene
    private List<Station> tablesInScene = new List<Station>(); //Used to place when we have to put our extra pots and pans on table
    //IMPLEMENTATION:
    //Create an array of the stoves that are in the scene - DONE
    //When a pot falls down, we will go through the array of stoves and see if something is on top of it
    //If a stove is being occupied, we skip until the next one. If the stove is empty, we store the fallen pot 
    //on the empty stove           

    void Awake()
    {
        //Helps initialize the amount of stoves that are in the scene
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

        //make a list of all the Tables in the scene
        Object[] tables = GameObject.FindObjectsOfType(typeof(Table));
        foreach(Table obj1 in tables)
        {
            Station station1 = obj1.GetComponent<Station>();
            tablesInScene.Add(station1);
        }

        Debug.Log("STOVES IN SCENE COUNT: " + stovesInScene.Count);
        Debug.Log("TABLES IN SCENE COUNT: " + tablesInScene.Count);
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
            playerHands.HandsEmpty();
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
            //When there all the stoves are occupied and the object can't go back onto the stove
            openTables(carryable);
        }
    }

     private void openTables(Carryable carryable){
        foreach (Station tables in tablesInScene)
            {
                //if a stove is not occupied, put the pot on that empty stove
                if (tables.PlacedItem == null) //EMPTY
                {
                    // Check if station has a placed item using the public property 
                    Debug.Log("Placed on table");
                    tables.PlaceItem(carryable);
                    return;
                }
            }
        carryable.transform.position = PotRespawnPoint.transform.position; //This will probably never happen
     }

    //Used as a timer so the Player must wait 5 seconds to respawn
    IEnumerator RespawnTime()
    {
        yield return new WaitForSeconds(5); //wait 5 seconds to respawn the character
        Debug.Log("Player Respawned");
        player.transform.position = respawnPoint.transform.position; //Set them to the respawnPoint
    }
}