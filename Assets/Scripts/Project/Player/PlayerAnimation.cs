using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Tooltip("The speed at which the player will transition from idle to running animation.")]
    [SerializeField] float _runningSpeedThreshold = 0.1f;
    Animator _animator;
    PlayerInteraction _playerInteraction;
    Vector3 _previousPosition;
    bool _isRunning = false;
    bool _isHolding = false;

    private void Awake() {
        _animator = GetComponentInChildren<Animator>(); // Animator component is on child fbx object
        _playerInteraction = GetComponent<PlayerInteraction>();
    }

    private void Start() {
        _playerInteraction.PickUpItemEvent += PlayPickUp;
        _playerInteraction.PlaceItemEvent += PlayDrop;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfRunning();
        CheckIfHolding();
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

    private void CheckIfHolding() {
        if(_playerInteraction.IsCarrying != _isHolding) { // Check if holding state has changed
            _isHolding = _playerInteraction.IsCarrying;
            _animator.SetBool("IsHolding", _isHolding); // Update the state once per change
        }
    }

    private void PlayPickUp() {
        _animator.SetTrigger("PickUp");
    }

    private void PlayDrop() {
        _animator.SetTrigger("Drop");
    }
}
