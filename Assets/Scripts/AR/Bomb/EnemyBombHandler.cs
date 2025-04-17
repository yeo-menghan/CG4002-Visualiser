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
            Debug.LogError("EnemyBombHandler: Snow Cloud Prefab is not assigned.");
            return;
        }

        float targetX = playerTransform.position.x;
        float targetZ = playerTransform.position.z;
        float targetY = worldAnchorTransform.position.y + heightOffset;
        Vector3 spawnPosition = new Vector3(targetX, targetY, targetZ);

        InstantiateSnowCloud(spawnPosition);
    }

    private void InstantiateSnowCloud(Vector3 position)
    {
        GameObject snowCloud = Instantiate(snowCloudPrefab, position, Quaternion.identity);
        snowCloud.tag = "Player";
        if (gameState != null && gameState.WorldCoordinateTransform != null)
        {
            snowCloud.transform.SetParent(worldAnchorTransform);
            Debug.Log("EnemyBombHandler: Snow cloud parented to worldCoordinateTransform");
        }
        else
        {
            Debug.LogWarning("EnemyBombHandler: worldCoordinateTransform not available, bomb left unparented");
        }

        Debug.Log($"EnemyBombHandler: Prefab original scale - {snowCloudPrefab.transform.localScale}");
        Debug.Log($"EnemyBombHandler: Snow cloud instantiated scale - {snowCloud.transform.localScale}");
        Debug.Log("EnemyBombHandler: Snow cloud instantiated at position: " + position);
    }
}
