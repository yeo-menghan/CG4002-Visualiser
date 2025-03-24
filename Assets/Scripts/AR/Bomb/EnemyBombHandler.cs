using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBombHandler : MonoBehaviour
{
    private GameState gameState;
    public GameObject snowCloudPrefab;
    [SerializeField] private Transform playerTransform;

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

        // Reference the player object in the scene
        gameState.enemyGameActionOccurred.AddListener(HandleEnemyBomb);
    }

    void OnDestroy()
    {
        if(gameState != null)
        {
            gameState.enemyGameActionOccurred.RemoveListener(HandleEnemyBomb);
        }
    }

    // This method will be called from the Inspector to handle enemy game actions
    public void HandleEnemyBomb(string actionType)
    {
        // Check if the action is "bomb" and if the player is visible to the enemy
        if (actionType == "bomb" && gameState.EnemyActive)
        {
            // Spawn the bomb on the player
            SpawnBombOnPlayer();
        }
    }

    private void SpawnBombOnPlayer()
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

        // Get the player's current position
        Vector3 spawnPosition = playerTransform.position;

        // Instantiate the rain cloud at the calculated position
        InstantiateSnowCloud(spawnPosition);
    }

    private void InstantiateSnowCloud(Vector3 position)
    {
        // Instantiate the rain cloud at the specified position
        GameObject snowCloud = Instantiate(snowCloudPrefab, position, Quaternion.identity);

        // Detach the snow cloud from the player camera
        snowCloud.transform.SetParent(null);

        Debug.Log($"EnemyBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"EnemyBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");
        Debug.Log("EnemyBombHandler: Snow cloud instantiated at position: " + position);
        string parentHierarchy = GetParentHierarchy(snowCloud.transform);
        Debug.Log($"EnemyBombHandler: Snow cloud parent hierarchy: {parentHierarchy}");
    }

    // Helper method to get the full parent hierarchy as a string
    private string GetParentHierarchy(Transform transform)
    {
        if (transform.parent == null)
        {
            return "EnemyBombHandler:: No parent (root object)";
        }

        string hierarchy = "";
        Transform current = transform.parent;

        while (current != null)
        {
            hierarchy = current.name + " -> " + hierarchy;
            current = current.parent;
        }

        // Remove the trailing arrow from the last entry
        if (hierarchy.EndsWith(" -> "))
        {
            hierarchy = hierarchy.Substring(0, hierarchy.Length - 4);
        }

        return hierarchy;
    }
}
