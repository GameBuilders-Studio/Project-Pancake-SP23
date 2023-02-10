using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputSelect : MonoBehaviour
{
    public List<GameObject> playerList;

    // Start is called before the first frame update
    void Start()
    {
        playerList = new List<GameObject>(Prefabs.LoadAll<GameObject>("Player"));

        Instantiate(playerList[2], new Vector3(0,0,0), Quaternion.identity);
    }
}
