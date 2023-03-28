using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Transform respawnPoint;

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Time());
        if(other.tag == "Player")
        {
            //StartCoroutine(Time());
            player.transform.position = respawnPoint.transform.position; 
        }
    }

    IEnumerator Time()
    {
        yield return new WaitForSeconds(3); //wait 5 seconds to respawn the character
        Debug.Log("Done");
    }
}

