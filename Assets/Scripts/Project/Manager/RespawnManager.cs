using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            StartCoroutine(RespawnTime());
        }
    }

    IEnumerator RespawnTime()
    {
        yield return new WaitForSeconds(5); //wait 5 seconds to respawn the character
        Debug.Log("Done"); 
        player.transform.position = respawnPoint.transform.position; //Set them to the respawnPoint
    }
}

