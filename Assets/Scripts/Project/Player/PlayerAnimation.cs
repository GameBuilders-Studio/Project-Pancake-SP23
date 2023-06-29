using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Tooltip("The speed at which the player will transition from idle to running animation.")]
    [SerializeField] float _runningSpeedThreshold = 0.1f;
    Animator _animator;
    Rigidbody _rigidbody;
    bool _isRunning = false;

    private void Awake() {
        _animator = GetComponentInChildren<Animator>(); // Animator component is on child fbx object
        _rigidbody = GetComponent<Rigidbody>(); 
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfRunning();
    }

    private void CheckIfRunning() {
        if (_rigidbody.velocity.magnitude > _runningSpeedThreshold != _isRunning) { // Check if running state has changed
            _isRunning = _rigidbody.velocity.magnitude > _runningSpeedThreshold; 
            _animator.SetBool("IsRunning", _isRunning); // Update the state once per change
        }
    }
}
