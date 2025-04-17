using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBombHandler : MonoBehaviour
{
    private GameState gameState;
    public GameObject snowCloudPrefab;
    public Transform enemyTargetTransform;

    public float heightOffset = 0f;

    private List<GameObject> spawnedBombs = new List<GameObject>();

    private void Start()
    {
        gameState = GameState.Instance;

        if (enemyTargetTransform == null)
        {
            Debug.LogError("PlayerBombHandler: Enemy Target Transform is not assigned in the Inspector! Please assign the 'EnemyImageTarget' GameObject.", this.gameObject);
        }
    }

    private void Update()
    {
        if (gameState != null)
        {
            gameState.UpdateEnemyInBombCount();
        }
    }

    public void SpawnBombOnEnemy()
    {
        if (enemyTargetTransform == null)
        {
            Debug.LogError("PlayerBombHandler: Cannot spawn bomb - Enemy Target Transform is not assigned in the Inspector.");
            return;
        }

        Debug.Log($"PlayerBombHandler: Using target transform at - {enemyTargetTransform.position}");

        if (snowCloudPrefab == null)
        {
            Debug.LogError("PlayerBombHandler: Snow Cloud Prefab is not assigned.");
            return;
        }

        float targetX = enemyTargetTransform.position.x;
        float targetZ = enemyTargetTransform.position.z;
        float targetY = gameState.WorldCoordinateTransform.position.y + heightOffset;

        Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

        GameObject snowCloud = Instantiate(snowCloudPrefab, spawnPosition, Quaternion.identity);
        snowCloud.transform.SetParent(gameState.WorldCoordinateTransform);

        Debug.Log($"PlayerBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"PlayerBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");

        spawnedBombs.Add(snowCloud);

        if (gameState != null)
        {
            int bombId = gameState.RegisterBomb(spawnPosition);
            BombTracker tracker = snowCloud.AddComponent<BombTracker>();
            tracker.Initialize(bombId);
            gameState.UpdateEnemyInBombCount();

            Debug.Log($"PlayerBombHandler: Snow cloud created at position: {spawnPosition}, ID: {bombId}");
        }
        else
        {
            Debug.LogError("PlayerBombHandler: GameState instance not found. Cannot register bomb or update count.");
        }
    }
}
