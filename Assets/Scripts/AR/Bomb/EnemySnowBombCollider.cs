using UnityEngine;
using System.Collections.Generic;

public class EnemySnowBombCollider : MonoBehaviour
{
    [Header("Detection Settings")]
    public string snowbombTag = "EnemySnowBomb";
    public float detectionRadius = 1.0f;

    [Header("Debug")]
    public bool showDebugMessages = true;

    // Private variables
    private GameState gameState;
    private Transform enemyTransform;
    private Dictionary<GameObject, bool> bombStatusTracker = new Dictionary<GameObject, bool>();
    private int currentEnemyInBombCount = 0;

    void Start()
    {
        gameState = GameState.Instance;
        if (gameState == null)
        {
            Debug.LogError("GameState instance not found!");
            return;
        }

        // We'll get the reference in Update instead of requiring assignment in the inspector
        Debug.Log("EnemySnowBombCollider initialized");
    }

    void Update()
    {
        // Get reference to enemy transform from GameState
        if (gameState == null)
            return;

        enemyTransform = gameState.EnemyCoordinateTransform;

        // Find all snowbombs in the scene
        GameObject[] snowbombs = GameObject.FindGameObjectsWithTag(snowbombTag);

        // Create a temporary list to track removed bombs
        List<GameObject> bombsToRemove = new List<GameObject>();
        foreach (var bomb in bombStatusTracker.Keys)
        {
            if (!System.Array.Exists(snowbombs, x => x == bomb))
            {
                bombsToRemove.Add(bomb);
            }
        }

        // Clean up any bombs that no longer exist
        foreach (var bomb in bombsToRemove)
        {
            if (bombStatusTracker[bomb]) // If enemy was in this bomb
            {
                currentEnemyInBombCount--;
            }
            bombStatusTracker.Remove(bomb);
            Debug.Log($"Removed tracking for destroyed bomb. Enemy in bomb count: {currentEnemyInBombCount}");
        }

        // Process each bomb
        foreach (GameObject bomb in snowbombs)
        {
            // Calculate distance between enemy and bomb
            float distance = Vector3.Distance(enemyTransform.position, bomb.transform.position);

            Debug.Log($"EnemySnowBombCollider - bomb.transform.position: {bomb.transform.position}");
            Debug.Log($"EnemySnowbombCollider - enemyTransform.position: {enemyTransform.position}");

            // If this is a new bomb we've never seen, add it to tracking
            if (!bombStatusTracker.ContainsKey(bomb))
            {
                bombStatusTracker.Add(bomb, false); // Initialize as "not in this bomb"
            }

            bool wasInBomb = bombStatusTracker[bomb];
            bool isInBomb = distance <= detectionRadius;

            // If status changed
            if (wasInBomb != isInBomb)
            {
                bombStatusTracker[bomb] = isInBomb;

                if (isInBomb)
                {
                    // Enemy entered bomb area
                    currentEnemyInBombCount++;
                    Debug.Log($"⚠️ ENEMY ENTERED snow bomb! Distance: {distance}m, Count: {currentEnemyInBombCount}");
                }
                else
                {
                    // Enemy exited bomb area
                    currentEnemyInBombCount--;
                    Debug.Log($"⚠️ ENEMY EXITED snow bomb! Distance: {distance}m, Count: {currentEnemyInBombCount}");
                }

                // Update game state
                gameState.EnemyInBombCount = currentEnemyInBombCount;
                Debug.Log($"Updated EnemyInBombCount: {currentEnemyInBombCount}");
            }

            // Log distance periodically to reduce spam
            if (showDebugMessages && Time.frameCount % 30 == 0)
            {
                Debug.Log($"Enemy distance to {bomb.name}: {distance}m (in bomb: {isInBomb})");
            }
        }
    }

    void OnDisable()
    {
        if (gameState != null && currentEnemyInBombCount > 0)
        {
            bombStatusTracker.Clear();
            currentEnemyInBombCount = 0;
            gameState.EnemyInBombCount = 0;

            Debug.Log("EnemySnowBombCollider disabled. Reset enemy bomb count to 0.");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (gameState != null && gameState.EnemyCoordinateTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gameState.EnemyCoordinateTransform.position, detectionRadius);
        }
    }

    public void ResetBombCounter()
    {
        bombStatusTracker.Clear();
        currentEnemyInBombCount = 0;

        if (gameState != null)
        {
            gameState.EnemyInBombCount = 0;
        }

        Debug.Log("Enemy bomb counter was manually reset to 0.");
    }
}
