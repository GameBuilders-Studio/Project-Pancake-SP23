using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;
[RequireComponent(typeof(Collider))]
public class RespawnManager : MonoBehaviour
{
    [Header("Dependencies")]
    
    [SerializeField][Required]
    [Tooltip("The point where the player will respawn to")]
    private Transform _respawnPoint;

    [SerializeField] [Required]
    [Tooltip("The point where the pot will respawn to if there are no stoves or counters available")]
    private Transform _potRespawnPoint;

    [SerializeField] [Required]
    [Tooltip("The point where the clean plate will respawn to if there are no stoves or counters available")]
    private DishStack _cleanDishStack; 

    [SerializeField] [Required]
    [Tooltip("The point where the dirty plate will respawn to if there are no stoves or counters available")]
    private DishStack _dirtyDishStack;

    private List<Station> _stovesInScene = new(); //Make an array storing all the stoves that are in the scene
    private List<Station> _tablesInScene = new(); //Used to place when we have to put our extra pots and pans on table     

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
            _stovesInScene.Add(station);
        }

        //make a list of all the Tables in the scene
        Object[] counters = GameObject.FindObjectsOfType(typeof(Counter));
        foreach (Counter obj1 in counters)
        {
            Station station1 = obj1.GetComponent<Station>();
            _tablesInScene.Add(station1);
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
            //Drop the pot that the player is holding and respawn it to a stove top
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
            foreach (Station stoves in _stovesInScene)
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


        // Use tag instead of GetComponent since plate does not have its own script (uses FoodContainer instead which is shared with Pot)
        if (other.tag == "Plate" /*plate != null <--ADD LATER*/)
        {
            if (!_cleanDishStack.IsFull())
            {
                //Increase the count of DishStacks for the openDistacks 
                _cleanDishStack.Count++; 
                Destroy(other.gameObject);
                return;
            }
        }

        DirtyPlate dirtyPlate = other.GetComponent<DirtyPlate>();
        if(dirtyPlate != null)
        {
            if (!_dirtyDishStack.IsFull())
            {
                //Increase the count of DishStacks for the openDistacks 
                _dirtyDishStack.Count += dirtyPlate.Count;
                Destroy(other.gameObject);
                return;
            }
        }
    }

    private void openTables(Carryable carryable)
    {
        foreach (Station tables in _tablesInScene)
        {
            //if a stove is not occupied, put the pot on that empty stove
            if (tables.PlacedItem == null) //EMPTY
            {
                // Check if station has a placed item using the public property 
                tables.PlaceItem(carryable);
                return;
            }
        }
        carryable.transform.position = _potRespawnPoint.transform.position; //This will probably never happen
    }

    //Used as a timer so the Player must wait 5 seconds to respawn
    IEnumerator RespawnTime(GameObject player)
    {
        yield return new WaitForSeconds(5); //wait 5 seconds to respawn the character
        player.transform.position = _respawnPoint.transform.position; //Set them to the respawnPoint
    }
}   