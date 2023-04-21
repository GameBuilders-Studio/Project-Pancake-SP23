using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
[RequireComponent(typeof(Collider))]
public class RespawnManager : MonoBehaviour
{
    [SerializeField]
    [Required]
    [Tooltip("The point where the player will respawn to")]
    private Transform respawnPoint;

    [SerializeField]
    [Required]
    [Tooltip("The point where the pot will respawn to if there are no stoves or counters available")]
    private Transform PotRespawnPoint;
    [SerializeField]
    [Required]
    private DishStack _dishStack;
    private List<Station> stovesInScene = new(); //Make an array storing all the stoves that are in the scene
    private List<Station> tablesInScene = new(); //Used to place when we have to put our extra pots and pans on table     

    void Awake()
    {
        //Helps initialize the amount of stoves that are in the scene
        Object[] stoves = FindObjectsOfType(typeof(Stove));
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
        Object[] counters = GameObject.FindObjectsOfType(typeof(Counter));
        foreach (Counter obj1 in counters)
        {
            Station station1 = obj1.GetComponent<Station>();
            tablesInScene.Add(station1);
        }
    }

    //When the player or pot hits the death trigger, this function will be used
    void OnTriggerEnter(Collider other)
    {
        //If the player hits the death trigger, 
        if (other.GetComponent<PlayerInteraction>() != null)
        {
            StartCoroutine(RespawnTime(other.gameObject));
            PlayerInteraction playerHands = other.GetComponent<PlayerInteraction>();
            //Drop the item that the player is holding and respawn it back to posititon
            playerHands.TryDropItem();
        }

        Pot pot = other.GetComponent<Pot>();
        //If the pot hits the death triggeer
        if (pot != null)
        {
            pot.ClearIngredients();
            Carryable carryable = other.GetComponent<Carryable>();
            if (carryable == null)
            {
                Debug.LogError("Objects with Pot tag should have Carryable component");
            }

            //We will iterate through each of the stoves in the scene
            foreach (Station stoves in stovesInScene)
            {
                //if a stove is not occupied, put the pot on that empty stove
                if (stoves.PlacedItem == null) //EMPTY
                {
                    // Check if station has a placed item using the public property 
                    stoves.PlaceItem(carryable);
                    return;
                }
            }
            //When there all the stoves are occupied and the object can't go back onto the stove
            openTables(carryable);
        }

        //If a plate touches the death trigger
        //Plate plate = other.GetComponent<Plate>(); <--ADD LATER
        if(other.tag == "Plate" /*plate != null <--ADD LATER*/)
        {
            Debug.Log("Plate touched death trigger");
            
                Debug.Log(_dishStack);
                //We must increase the count of the DishStackCounter
                _dishStack.Count++;

            //Delete the plate off the scene so the player doesn't see the plate fall down
        }
    }

    private void openTables(Carryable carryable)
    {
        foreach (Station tables in tablesInScene)
        {
            //if a stove is not occupied, put the pot on that empty stove
            if (tables.PlacedItem == null) //EMPTY
            {
                // Check if station has a placed item using the public property 
                tables.PlaceItem(carryable);
                return;
            }
        }
        carryable.transform.position = PotRespawnPoint.transform.position; //This will probably never happen
    }

    //Used as a timer so the Player must wait 5 seconds to respawn
    IEnumerator RespawnTime(GameObject player)
    {
        yield return new WaitForSeconds(5); //wait 5 seconds to respawn the character
        player.transform.position = respawnPoint.transform.position; //Set them to the respawnPoint
    }
}