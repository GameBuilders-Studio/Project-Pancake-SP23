using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingStation : MonoBehaviour
{
    [Tooltip("When choppingTimeCurrent reaches choppingTimeMax, the chopping is done")]
    [SerializeField]
    private float _choppingTimeCurrent = 0f;
    [SerializeField]
    private float _choppingTimeMax = 3f;
    [SerializeField]
    private bool _isChopping = false;
    
    public void StartChopping(){
        Debug.Log("Start Chopping");
        _isChopping = true;
        StartCoroutine("ChoppingTimer");
    }
    public void StopChopping(){
        Debug.Log("Stop Chopping");
        _isChopping = false;
        StopCoroutine("ChoppingTimer");
    }
    public void FinishChopping(){
        Debug.Log("Finish Chopping");
        _isChopping = false;;
        StopCoroutine("ChoppingTimer");
        _choppingTimeCurrent = 0f;
    }

    private IEnumerator ChoppingTimer(){
        while(_isChopping){
            yield return new WaitForSeconds(Time.deltaTime);
            _choppingTimeCurrent+=Time.deltaTime;
            if(_choppingTimeCurrent >= _choppingTimeMax){
                _isChopping = false;
                FinishChopping();
                yield break;
            }
        }
    }
    
    //Player can't move when chopping
    public void LockPlayer(){
        Debug.Log("Lock Player");

    }
}
