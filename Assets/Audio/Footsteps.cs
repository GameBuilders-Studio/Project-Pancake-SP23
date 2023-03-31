using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource AS;

    public AudioClip[] FootstepSounds;

    public float TimeBetweenSteps;

    //I do not know how to check if the associated player is walking
    //So this is my placeholder until then.
    bool isWalking = true;
    bool wasWalking = false;

    void Awake()
    {
        if(AS == null)
        {
            Debug.LogWarning($"The Footsteps script on object {gameObject.name} has no AudioSource");
        }
    }

    // Start is called before the first frame update
    void Update()
    {
        //If player just started walking, Invoke walking()
        if(isWalking && !wasWalking)
        {
            Walking();
        }

        wasWalking = isWalking;
    }

    void Walking()
    {
        //If player is still walking, make footstep sound after TimeBetweenSteps sec.
        if (isWalking)
        {
            Invoke("Footstep", TimeBetweenSteps);
        }
    }

    void Footstep()
    {
        if (AS != null)
        {
            //Play random footstep sound
            AS.clip = FootstepSounds[Random.Range(0, FootstepSounds.Length)];
            AS.Play();
            //Play another footstep sound if still walking
            Walking();
        }
    }
}
