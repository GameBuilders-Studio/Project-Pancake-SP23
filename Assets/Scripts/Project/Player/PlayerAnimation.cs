using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Tooltip("The speed at which the player will transition from idle to running animation.")]
    [SerializeField] float _runningSpeedThreshold = 0.1f;
    Animator _animator;
    Vector3 _previousPosition;
    bool _isRunning = false;

    private void Awake() {
        _animator = GetComponentInChildren<Animator>(); // Animator component is on child fbx object
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfRunning();
    }

    private void CheckIfRunning() {
        float speed = (transform.position - _previousPosition).magnitude / Time.deltaTime;
        _previousPosition = transform.position;
        Debug.Log("Speed: " + speed);

        if (speed > _runningSpeedThreshold != _isRunning) { // Check if running state has changed
            _isRunning = speed > _runningSpeedThreshold; 
            _animator.SetBool("IsRunning", _isRunning); // Update the state once per change
        }
    }
}
