using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    private GameState gameState;
    public GameObject snowCloudPrefab;
    private Transform enemyTransform;
    public float heightOffset = 0f; // Adjustable height offset

    // List to keep track of all spawned bombs
    private List<GameObject> spawnedBombs = new List<GameObject>();

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;
        // gameState.gameActionOccurred.AddListener(HandlePlayerBomb);
    }

    private void Update()
    {
        // Periodically update the enemy in bomb count
        // (in case the enemy moves)
        if (gameState != null && gameState.EnemyActive)
        {
            gameState.UpdateEnemyInBombCount();
        }
    }

    // void OnDestroy()
    // {
    //     if(gameState != null)
    //     {
    //         gameState.gameActionOccurred.RemoveListener(HandlePlayerBomb);
    //     }
    // }

    // This method will be called from the Inspector to handle enemy game actions
    public void HandlePlayerBomb(string actionType)
    {
        // Check if the action is "bomb" and if the player is visible to the enemy
        Debug.Log($"PlayerBombHandler: gameState.EnemyActive - {gameState.EnemyActive}");
        if (actionType == "bomb" && gameState.EnemyActive)
        {
            SpawnBombOnEnemy();
        }
    }

    public void SpawnBombOnEnemy()
    {
        enemyTransform = gameState.EnemyCoordinateTransform;
        Debug.Log($"PlayerBombHandler: enemyTransform - {enemyTransform.position}");

        if (enemyTransform == null)
        {
            Debug.LogError("PlayerBombHandler: Enemy transform not found.");
            return;
        }

        if (snowCloudPrefab == null)
        {
            Debug.LogError("PlayerBombHandler: Snow Cloud Prefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = enemyTransform.position;
        spawnPosition.y += heightOffset;
        GameObject snowCloud = Instantiate(snowCloudPrefab, spawnPosition, Quaternion.Euler(Vector3.up));
        // snowCloud.transform.SetParent(null);

        Debug.Log($"PlayerBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"PlayerBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");

        // Add to our list of spawned bombs
        spawnedBombs.Add(snowCloud);

        // Register the bomb with GameState
        int bombId = gameState.RegisterBomb(spawnPosition);

        // Add a BombTracker component to the snow cloud if bomb positions need to be updated
        BombTracker tracker = snowCloud.AddComponent<BombTracker>();
        tracker.Initialize(bombId);

        // Update the enemy in bomb count immediately
        gameState.UpdateEnemyInBombCount();

        Debug.Log($"PlayerBombHandler: Snow cloud created at position: {spawnPosition}, ID: {bombId}");
    }

    // Optional: Method to clear all bombs (for level transitions or game reset)
    public void ClearAllBombs()
    {
        foreach (GameObject bomb in spawnedBombs)
        {
            if (bomb != null)
            {
                Destroy(bomb);
            }
        }

        spawnedBombs.Clear();

        // Clear bombs in GameState
        if (gameState != null)
        {
            gameState.ClearAllBombs();
        }
    }
}
