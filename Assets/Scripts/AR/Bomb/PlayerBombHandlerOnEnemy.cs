using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandlerOnEnemy : MonoBehaviour
{
    private GameState gameState;

    [Tooltip("Snow Cloud prefab to spawn when the bomb action is triggered.")]
    [SerializeField] private GameObject snowCloudPrefab;

    [Tooltip("Enemy GameObject to attach the bomb.")]
    [SerializeField] private GameObject enemyObject;

    [SerializeField] private float heightOffset = 0f; // Adjustable height offset

    // Add this to allow finding the object at runtime if needed
    [SerializeField] private string enemyTargetName = "EnemyImageTarget";

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;
        if (gameState == null)
        {
            Debug.LogError("PlayerBombHandler: GameState instance not found.");
            return;
        }

        // Try to find the enemy object if not already assigned
        if (enemyObject == null)
        {
            enemyObject = GameObject.Find(enemyTargetName);
            if (enemyObject == null)
            {
                Debug.LogWarning("PlayerBombHandler: EnemyObject not found, will attempt to find it at runtime.");
            }
        }

        gameState.gameActionOccurred.AddListener(HandlePlayerBomb);
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(HandlePlayerBomb);
        }
    }

    public void HandlePlayerBomb(string actionType)
    {
        // // Try to find enemy object again if it's null
        // if (enemyObject == null)
        // {
        //     enemyObject = GameObject.Find(enemyTargetName);
        //     if (enemyObject == null)
        //     {
        //         Debug.LogError("PlayerBombHandler: EnemyObject is null during HandlePlayerBomb and could not be found.");
        //         return;
        //     }
        // }

        // Check if the action is "bomb" and if the player is visible to the enemy
        if (actionType == "bomb" && gameState.EnemyActive)
        {
            Debug.Log($"PlayerBombHandler: Preparing to spawn bomb on enemy");
            SpawnBombOnEnemy();
        }
    }

    private void SpawnBombOnEnemy()
    {
        Debug.Log("PlayerBombHandler: Within SpawnBombOnEnemy");
        if (enemyObject == null)
        {
            Debug.LogError("PlayerBombHandler: EnemyObject is not assigned.");
            return;
        }

        if (snowCloudPrefab == null)
        {
            Debug.LogError("PlayerBombHandler: Snow Cloud Prefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = enemyObject.transform.position;
        spawnPosition.y += heightOffset;

        GameObject snowCloud = Instantiate(snowCloudPrefab, spawnPosition, Quaternion.identity);
        // snowCloud.transform.SetParent(null);

        Debug.Log($"PlayerBombHandler: Snow cloud created at position: {spawnPosition}");
    }
}
