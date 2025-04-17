using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBombHandler : MonoBehaviour
{
    private GameState gameState;
    public GameObject snowCloudPrefab;
    [SerializeField] private Transform playerTransform;
    public Transform worldAnchorTransform;

    public float heightOffset = 0f;

    void Awake()
    {
        if (playerTransform == null)
        {
            playerTransform = Camera.main.transform;
            Debug.Log("EnemyBombHandler: Reference transform not assigned, using main camera as default.");
        }
    }

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;
    }

    public void SpawnBombOnPlayer()
    {
        if (playerTransform == null)
        {
            Debug.LogError("EnemyBombHandler: Player transform not found.");
            return;
        }

        if (snowCloudPrefab == null)
        {
            Debug.LogError("EnemyBombHandler: Rain Cloud Prefab is not assigned.");
            return;
        }

        // --- Calculate Target World Position ---
        // Use player's X and Z world position
        float targetX = playerTransform.position.x;
        float targetZ = playerTransform.position.z;

        // Use WorldCoordinateTransform's Y world position + offset
        // float targetY = gameState.WorldCoordinateTransform.position.y + heightOffset;
        float targetY = worldAnchorTransform.position.y + heightOffset;

        // Combine into the final world position for instantiation
        Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

        // Vector3 spawnPosition = playerTransform.position;
        // spawnPosition.y += heightOffset;

        // Instantiate the rain cloud at the calculated position
        InstantiateSnowCloud(spawnPosition);
    }

    private void InstantiateSnowCloud(Vector3 position)
    {
        // Instantiate the rain cloud at the specified position
        GameObject snowCloud = Instantiate(snowCloudPrefab, position, Quaternion.identity);

        // Add the "Player" tag to the instantiated bomb
        snowCloud.tag = "Player";

        // Anchor the snow cloud to the worldCoordinateTransform
        if (gameState != null && gameState.WorldCoordinateTransform != null)
        {
            // snowCloud.transform.SetParent(gameState.WorldCoordinateTransform);
            snowCloud.transform.SetParent(worldAnchorTransform);
            Debug.Log("EnemyBombHandler: Snow cloud parented to worldCoordinateTransform");
        }
        else
        {
            Debug.LogWarning("EnemyBombHandler: worldCoordinateTransform not available, bomb left unparented");
        }

        // spawnedBombs.Add(snowCloud);
        // int bombId = gameState.RegisterPlayerBomb(position);

        // PlayerBombTracker tracker = snowCloud.AddComponent<PlayerBombTracker>();
        // tracker.Initialize(bombId);

        // gameState.UpdatePlayerInBombCount();

        Debug.Log($"EnemyBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"EnemyBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");
        Debug.Log("EnemyBombHandler: Snow cloud instantiated at position: " + position);
    }
}
