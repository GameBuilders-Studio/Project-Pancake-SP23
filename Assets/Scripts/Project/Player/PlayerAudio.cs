using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public class PlayerAudio : MonoBehaviour
{
    [SerializeField]
    [Required]
    private AudioSource _audioSource;

    [SerializeField]
    [Required]
    private PlayerInteraction _playerInteraction;

    void OnValidate()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerInteraction = GetComponent<PlayerInteraction>();
    }

    void OnEnable()
    {
        _playerInteraction.DashEvent += OnDash;
        _playerInteraction.PickUpItemEvent += OnPickUp;
        _playerInteraction.PlaceItemEvent += OnPlace;
    }

    void OnDisable()
    {
        _playerInteraction.DashEvent -= OnDash;
        _playerInteraction.PickUpItemEvent -= OnPickUp;
        _playerInteraction.PlaceItemEvent -= OnPlace;
    }

    private void OnDash()
    {
        // _audioSource.PlayOneShot(myclip);
    }

    private void OnPickUp()
    { }

    private void OnPlace()
    { }
}
