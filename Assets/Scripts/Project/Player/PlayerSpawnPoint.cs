using UnityEngine;
using UnityEditor;
using EasyCharacterMovement;
using CustomAttributes;

[ExecuteInEditMode]
public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField]
    [Tooltip("This ratio is used to calculate the offset based on the player's collider height. This is used to spawn the player at the correct height. ")]
    private float _heightOffsetRatio = 0.5f;
    [SerializeField, ReadOnly]
    public PlayerCharacter ManagedPlayer;
    [SerializeField, ReadOnly]
    private CapsuleCollider _playerCollider;
    [SerializeField, ReadOnly]
    private Vector3 _floorPosition;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Handles.DrawWireDisc(_floorPosition + Vector3.up * 0.03f, Vector3.up, 0.5f, 2.0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        CustomGizmos.DrawArrow(transform.position, _floorPosition);
    }
#endif

    void OnValidate()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 10.0f, -1, QueryTriggerInteraction.Ignore))
        {
            _floorPosition = hitInfo.point;
        }
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

            var heightOffset = _playerCollider.height * _heightOffsetRatio * Vector3.up;
            player.transform.position = _floorPosition + heightOffset;

            if (!player.TryGetComponent(out ManagedPlayer))
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
            var heightOffset = _playerCollider.height * _heightOffsetRatio * Vector3.up;
            ManagedPlayer.transform.position = _floorPosition + heightOffset;
        }
    }
}
