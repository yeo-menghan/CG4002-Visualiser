using UnityEngine;
using System.Collections;

public class MeleeActionManager : MonoBehaviour
{
    private GameState gameState;

    public GameObject boxingHitEffectPrefab;
    public GameObject boxingMissEffectPrefab;
    public GameObject fencingHitEffectPrefab;
    public GameObject fencingMissEffectPrefab;

    // Add distance from camera for spawning miss effects
    [SerializeField] private float missEffectDistance = 10f;
    // Add a transform to use as a spawn position for screen center effects
    [SerializeField] private Transform screenCenterTarget;

    void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(HandleMeleeAction);

        // Create screen center target if it doesn't exist
        if (screenCenterTarget == null)
        {
            GameObject targetObj = new GameObject("ScreenCenterTarget");
            screenCenterTarget = targetObj.transform;
            // Position it in front of the camera
            PositionScreenCenterTarget();
        }

        Debug.Log("MeleeActionManager: Ready");
    }

    void Update()
    {
        // Keep the screen center target positioned correctly
        // This ensures it stays in front of the camera even if camera moves
        if (screenCenterTarget != null)
        {
            PositionScreenCenterTarget();
        }
    }

    private void PositionScreenCenterTarget()
    {
        if (Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 cameraForward = Camera.main.transform.forward;
            screenCenterTarget.position = cameraPosition + cameraForward * missEffectDistance;
        }
    }

    void OnDestroy()
    {
        if(gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(HandleMeleeAction);
        }
    }

    public void HandleMeleeAction(string actionType)
    {
        Debug.Log($"MeleeActionManager: Handling action {actionType}, EnemyActive: {gameState.EnemyActive}");

        // Doesn't matter if enemy is active or not, just animate
        if (actionType == "boxing")
        {
            if (gameState.EnemyActive)
            {
                Debug.Log("Spawning boxing hit effect on enemy");
                SpawnPrefabOnEnemy(boxingHitEffectPrefab);
            }
            else
            {
                Debug.Log("Spawning boxing miss effect at screen center");
                SpawnPrefabAtScreenCenter(boxingMissEffectPrefab);
            }
        }
        else if (actionType == "fencing")
        {
            if (gameState.EnemyActive)
            {
                Debug.Log("Spawning fencing hit effect on enemy");
                SpawnPrefabOnEnemy(fencingHitEffectPrefab);
            }
            else
            {
                Debug.Log("Spawning fencing miss effect at screen center");
                SpawnPrefabAtScreenCenter(fencingMissEffectPrefab);
            }
        }
        else
        {
            Debug.LogWarning($"MeleeActionManager: Unhandled actionType '{actionType}' or no enemy active.");
        }
    }

    private GameObject SpawnPrefabOnEnemy(GameObject prefab)
    {
        if (prefab == null || gameState.EnemyCoordinateTransform == null)
        {
            Debug.LogWarning("SpawnPrefabOnEnemy: Prefab or enemy transform is null.");
            return null;
        }

        GameObject hitEffect = Instantiate(prefab, gameState.EnemyCoordinateTransform.position, Quaternion.identity);
        Debug.Log($"Spawned {prefab.name} at enemy position {gameState.EnemyCoordinateTransform.position}");
        Destroy(hitEffect, 2f);
        return hitEffect;
    }

    private GameObject SpawnPrefabAtScreenCenter(GameObject prefab, float lifetime = 2f)
    {
        if (prefab == null)
        {
            Debug.LogWarning("SpawnPrefabAtScreenCenter: Prefab is null.");
            return null;
        }

        // Use the screen center target transform for consistent positioning
        Vector3 spawnPosition = screenCenterTarget != null ?
            screenCenterTarget.position :
            Camera.main.transform.position + Camera.main.transform.forward * missEffectDistance;

        // Add a small random offset to ensure it's not exactly at the same position
        Vector3 randomOffset = new Vector3(
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f),
            Random.Range(-0.5f, 0.5f)
        );

        spawnPosition += randomOffset;

        // Instantiate the prefab at the calculated position
        GameObject effectInstance = Instantiate(prefab, spawnPosition, Quaternion.identity);
        Debug.Log($"Spawned {prefab.name} at screen center position {spawnPosition}");

        // Destroy the effect after the specified lifetime
        if (lifetime > 0f)
        {
            Destroy(effectInstance, lifetime);
        }

        return effectInstance;
    }
}
