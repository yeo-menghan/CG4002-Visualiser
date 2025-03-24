using UnityEngine;
using System.Collections.Generic;

public class PlayerSnowBombCollider : MonoBehaviour
{
    [Header("Detection Settings")]
    public string snowbombTag = "Snowbomb";
    public float detectionRadius = 1.0f;

    [Header("Debug")]
    public bool showDebugMessages = true;

    // Private variables
    private GameState gameState;
    private Dictionary<GameObject, bool> bombStatusTracker = new Dictionary<GameObject, bool>();
    private int currentInBombCount = 0;
    private Transform cameraTransform;

    void Awake()
    {
        cameraTransform = Camera.main.transform;
        Debug.Log("PlayerSnowBombCollider initialized on camera");
    }

    void Start()
    {
        gameState = GameState.Instance;
        if (gameState == null)
        {
            Debug.LogError("GameState instance not found!");
        }

        Debug.Log("PlayerSnowBombCollider Start method completed");
    }

    void Update()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Cannot find main camera!");
                return;
            }
        }

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
            if (bombStatusTracker[bomb]) // If player was in this bomb
            {
                currentInBombCount--;
            }
            bombStatusTracker.Remove(bomb);
            Debug.Log($"Removed tracking for destroyed bomb. In bomb count: {currentInBombCount}");
        }

        // Process each bomb
        foreach (GameObject bomb in snowbombs)
        {
            // Calculate distance
            float distance = Vector3.Distance(cameraTransform.position, bomb.transform.position);

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
                    // Player entered bomb area
                    currentInBombCount++;
                    Debug.Log($"⚠️ ENTERED snow bomb! Distance: {distance}m, Count: {currentInBombCount}");
                }
                else
                {
                    // Player exited bomb area
                    currentInBombCount--;
                    Debug.Log($"⚠️ EXITED snow bomb! Distance: {distance}m, Count: {currentInBombCount}");
                }

                // Update game state
                if (gameState != null)
                {
                    gameState.PlayerInBombCount = currentInBombCount;
                    Debug.Log($"Updated PlayerInBombCount: {currentInBombCount}");
                }
            }

            // Log distance periodically to reduce spam
            if (showDebugMessages && Time.frameCount % 30 == 0)
            {
                Debug.Log($"Distance to {bomb.name}: {distance}m (in bomb: {isInBomb})");
            }
        }
    }

    void OnDisable()
    {
        if (gameState != null && currentInBombCount > 0)
        {
            bombStatusTracker.Clear();
            currentInBombCount = 0;
            gameState.PlayerInBombCount = 0;

            Debug.Log("PlayerSnowBombCollider disabled. Reset player bomb count to 0.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 position = Application.isPlaying && cameraTransform != null ?
                          cameraTransform.position :
                          Camera.main != null ? Camera.main.transform.position : transform.position;

        Gizmos.DrawWireSphere(position, detectionRadius);
    }

    public void ResetBombCounter()
    {
        bombStatusTracker.Clear();
        currentInBombCount = 0;

        if (gameState != null)
        {
            gameState.PlayerInBombCount = 0;
        }

        Debug.Log("Player bomb counter was manually reset to 0.");
    }
}
