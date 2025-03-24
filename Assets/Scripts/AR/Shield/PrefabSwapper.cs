using UnityEngine;

public class PrefabSwapper : MonoBehaviour
{
    // Reference to GameState
    private GameState gameState;

    // References to your prefabs
    public GameObject playerPrefab;
    public GameObject shieldPrefab;

    // Reference to the currently active object
    private GameObject currentObject;

    // Track which prefab is currently active
    private bool isShieldActive = false;

    void Start()
    {
        // Get reference to GameState
        gameState = GameState.Instance;

        // Initialize with player prefab by default
        SpawnPrefab(playerPrefab);
        isShieldActive = false;
    }

    void Update()
    {
        // Check if shield should be active
        bool shouldShieldBeActive = gameState.EnemyCurrentShield > 0;

        // Only swap prefabs if there's a change in the shield state
        if (shouldShieldBeActive != isShieldActive)
        {
            if (shouldShieldBeActive)
            {
                // Switch to shield prefab
                SpawnPrefab(shieldPrefab);
                isShieldActive = true;
            }
            else
            {
                // Switch back to player prefab
                SpawnPrefab(playerPrefab);
                isShieldActive = false;
            }
        }
    }

    private void SpawnPrefab(GameObject prefab)
    {
        // Destroy the current object if it exists
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        // Instantiate the new prefab as a child of this transform
        currentObject = Instantiate(prefab, transform.position, transform.rotation);
        currentObject.transform.parent = transform;
    }
}
