using UnityEngine;
using UnityEditor;
using EasyCharacterMovement;
using CustomAttributes;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField]
    public PlayerCharacter ManagedPlayer;
    
    [SerializeField]
    [ReadOnly]
    private CapsuleCollider _playerCollider;

    [SerializeField]
    [ReadOnly]
    private Vector3 _floorPosition;

    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(_floorPosition + Vector3.up * 0.03f, Vector3.up, 0.5f, 2.0f);
    }

    void OnDrawGizmosSelected()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10.0f, -1, QueryTriggerInteraction.Ignore))
        {
            _floorPosition = hitInfo.point;
        }
        Gizmos.color = Color.green;
        CustomGizmos.DrawArrow(transform.position, _floorPosition);
    }

    public void SpawnPlayer(int playerIndex, GameObject playerPrefab)
    {
        if (ManagedPlayer == null)
        {
            // instantiate new player
            var player = Instantiate(playerPrefab, transform.position, Quaternion.identity);

            if (!player.TryGetComponent(out _playerCollider))
            {
                Debug.LogError("Player prefab has no capsule collider!");
            }

            var heightOffset =  _playerCollider.height * 0.5f * Vector3.up;
            player.transform.position = _floorPosition + heightOffset;

            if (!player.TryGetComponent(out PlayerCharacter ManagedPlayer))
            {
                Debug.LogError("Player prefab has no PlayerCharacter!");
            }

            if (!player.TryGetComponent(out PlayerInputHandler inputHandler))
            {
                Debug.LogError("Player prefab has no PlayerInputHandler!");
            }

            inputHandler.PlayerIndex = playerIndex;
        }
        else
        {
            // respawn player
            var heightOffset =  _playerCollider.height * 0.5f * Vector3.up;
            ManagedPlayer.transform.position = _floorPosition + heightOffset;
        }
    }
}
