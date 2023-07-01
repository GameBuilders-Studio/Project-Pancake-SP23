using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SetLevelTitleFromCapsule : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Text>().text = DataCapsule.instance.lastLevel;
    }

}
