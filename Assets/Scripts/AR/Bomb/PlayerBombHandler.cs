// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerBombHandler : MonoBehaviour
// {
//     private GameState gameState;
//     public GameObject snowCloudPrefab;
//     private Transform enemyTransform;
//     public float heightOffset = 0f; // Adjustable height offset

//     // List to keep track of all spawned bombs
//     private List<GameObject> spawnedBombs = new List<GameObject>();

//     private void Start()
//     {
//         // Get reference to the GameState singleton
//         gameState = GameState.Instance;
//         // gameState.gameActionOccurred.AddListener(HandlePlayerBomb);
//     }

//     private void Update()
//     {
//         gameState.UpdateEnemyInBombCount();
//     }

//     public void SpawnBombOnEnemy()
//     {
//         enemyTransform = gameState.EnemyCoordinateTransform;
//         Debug.Log($"PlayerBombHandler: enemyTransform - {enemyTransform.position}");

//         if (enemyTransform == null)
//         {
//             Debug.LogError("PlayerBombHandler: Enemy transform not found.");
//             return;
//         }

//         if (snowCloudPrefab == null)
//         {
//             Debug.LogError("PlayerBombHandler: Snow Cloud Prefab is not assigned.");
//             return;
//         }

//         // --- Calculate Target World Position ---
//         // Use player's X and Z world position
//         float targetX = enemyTransform.position.x;
//         float targetZ = enemyTransform.position.z;

//         // Use WorldCoordinateTransform's Y world position + offset
//         float targetY = gameState.WorldCoordinateTransform.position.y + heightOffset;

//         // Combine into the final world position for instantiation
//         Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

//         // Vector3 spawnPosition = enemyTransform.position;
//         // spawnPosition.y += heightOffset;
//         GameObject snowCloud = Instantiate(snowCloudPrefab, spawnPosition, Quaternion.Euler(Vector3.up));

//         // if (gameState != null && gameState.WorldCoordinateTransform != null)
//         // {
//         //     snowCloud.transform.SetParent(gameState.WorldCoordinateTransform);
//         //     Debug.Log("PlayerBombHandler: Snow cloud parented to worldCoordinateTransform");
//         // }
//         // else
//         // {
//         //     Debug.LogWarning("PlayerBombHandler: worldCoordinateTransform not available, bomb left unparented");
//         // }

//         Debug.Log($"PlayerBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
//         Debug.Log($"PlayerBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");

//         // Add to our list of spawned bombs
//         spawnedBombs.Add(snowCloud);

//         // Register the bomb with GameState
//         int bombId = gameState.RegisterBomb(spawnPosition);

//         // Add a BombTracker component to the snow cloud if bomb positions need to be updated
//         BombTracker tracker = snowCloud.AddComponent<BombTracker>();
//         tracker.Initialize(bombId);

//         // Update the enemy in bomb count immediately
//         gameState.UpdateEnemyInBombCount();

//         Debug.Log($"PlayerBombHandler: Snow cloud created at position: {spawnPosition}, ID: {bombId}");
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    private GameState gameState;
    public GameObject snowCloudPrefab;
    public Transform enemyTargetTransform;
    // public Transform worldAnchorTransform;

    public float heightOffset = 0f; // Adjustable height offset

    // List to keep track of all spawned bombs
    private List<GameObject> spawnedBombs = new List<GameObject>();

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Optional but recommended: Add a check here to ensure the target was assigned
        if (enemyTargetTransform == null)
        {
            Debug.LogError("PlayerBombHandler: Enemy Target Transform is not assigned in the Inspector! Please assign the 'EnemyImageTarget' GameObject.", this.gameObject);
        }

        // gameState.gameActionOccurred.AddListener(HandlePlayerBomb); // Keep this commented if not used
    }

    private void Update()
    {
        // Ensure gameState is available before using it
        if (gameState != null)
        {
            gameState.UpdateEnemyInBombCount();
        }
        // else { Debug.LogWarning("PlayerBombHandler: GameState instance not available in Update."); } // Optional warning
    }

    public void SpawnBombOnEnemy()
    {
        // --- Use the assigned enemyTargetTransform ---
        // Check if the transform has been assigned in the Inspector
        if (enemyTargetTransform == null)
        {
            Debug.LogError("PlayerBombHandler: Cannot spawn bomb - Enemy Target Transform is not assigned in the Inspector.");
            return; // Exit the function if the target isn't set
        }

        Debug.Log($"PlayerBombHandler: Using target transform at - {enemyTargetTransform.position}");

        if (snowCloudPrefab == null)
        {
            Debug.LogError("PlayerBombHandler: Snow Cloud Prefab is not assigned.");
            return;
        }

        // --- Calculate Target World Position ---
        // Use the assigned enemyTargetTransform's X and Z world position
        float targetX = enemyTargetTransform.position.x;
        float targetZ = enemyTargetTransform.position.z;

        // Use WorldCoordinateTransform's Y world position + offset (from GameState)
        // Make sure GameState and its WorldCoordinateTransform are available
        float targetY;
        if (gameState != null && gameState.WorldCoordinateTransform != null)
        {
            targetY = gameState.WorldCoordinateTransform.position.y + heightOffset;
            // targetY = worldAnchorTransform.position.y + heightOffset;
        }
        else
        {
            Debug.LogWarning("PlayerBombHandler: GameState or WorldCoordinateTransform not available for Y calculation. Using target's Y + offset as fallback.");
            // Fallback: Use the target's Y plus offset if GameState/WorldCoord isn't ready
            targetY = enemyTargetTransform.position.y + heightOffset;
        }


        // Combine into the final world position for instantiation
        Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

        // Instantiate the snow cloud prefab
        // Using Quaternion.identity for default rotation, Quaternion.Euler(Vector3.up) might be specific if you need Y rotation
        GameObject snowCloud = Instantiate(snowCloudPrefab, spawnPosition, Quaternion.identity);

        if (gameState != null && gameState.WorldCoordinateTransform != null)
        {
            snowCloud.transform.SetParent(gameState.WorldCoordinateTransform);
            // snowCloud.transform.SetParent(worldAnchorTransform);
            Debug.Log("PlayerBombHandler: Snow cloud parented to worldCoordinateTransform");
        }
        else
        {
            Debug.LogWarning("PlayerBombHandler: worldCoordinateTransform not available, bomb left unparented");
        }

        Debug.Log($"PlayerBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"PlayerBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");

        // Add to our list of spawned bombs
        spawnedBombs.Add(snowCloud);

        // Register the bomb with GameState (check if gameState is valid first)
        if (gameState != null)
        {
            int bombId = gameState.RegisterBomb(spawnPosition);

            // Add a BombTracker component to the snow cloud
            BombTracker tracker = snowCloud.AddComponent<BombTracker>();
            tracker.Initialize(bombId);

            // Update the enemy in bomb count immediately
            gameState.UpdateEnemyInBombCount();

            Debug.Log($"PlayerBombHandler: Snow cloud created at position: {spawnPosition}, ID: {bombId}");
        }
        else
        {
             Debug.LogError("PlayerBombHandler: GameState instance not found. Cannot register bomb or update count.");
        }
    }
}
