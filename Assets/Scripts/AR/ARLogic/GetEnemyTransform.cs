using UnityEngine;

public class GetEnemyTransform : MonoBehaviour
{
    private GameState gameState;
    private DefaultObserverEventHandler observerEventHandler;

    void Start()
    {
        // Get the transform of this GameObject
        gameState = GameState.Instance;

        // Now you can access position, rotation, scale etc.
        Debug.Log("GetEnemyTransform: Starting Position: " + transform.position);
        Debug.Log("GetEnemyTransform: Starting Rotation: " + transform.rotation);
        Debug.Log("GetEnemyTransform: Starting Scale: " + transform.localScale);

        observerEventHandler = GetComponent<DefaultObserverEventHandler>();

        if (observerEventHandler == null)
        {
            Debug.LogError("ImageTargetDistanceTracker: DefaultObserverEventHandler not found on this GameObject!");
            return;
        }

        // Register callbacks for tracking found/lost
        observerEventHandler.OnTargetFound.AddListener(OnTargetFound);
        observerEventHandler.OnTargetLost.AddListener(OnTargetLost);

        Debug.Log($"ImageTargetDistanceTracker: Initialized on GameObject: {gameObject.name}");
    }

    void Update()
    {
        if (gameState.EnemyActive == true)
        {
            gameState.EnemyCoordinateTransform = transform;
        }
        else
        {
            gameState.EnemyCoordinateTransform = null;
        }

        Debug.Log("GetEnemyTransform: Position: " + gameState.EnemyCoordinateTransform.position);
        Debug.Log("GetEnemyTransform: Rotation: " + gameState.EnemyCoordinateTransform.rotation);
        Debug.Log("GetEnemyTransform: Scale: " + gameState.EnemyCoordinateTransform.localScale);
    }

    private void OnTargetFound()
    {
        gameState.EnemyActive = true;
        gameState.EnemyCoordinateTransform = transform;

        Debug.Log($"GetEnemyTransform: EnemyActive: " + gameState.EnemyActive);
        Debug.Log("GetEnemyTransform: Position: " + gameState.EnemyCoordinateTransform.position);
        Debug.Log("GetEnemyTransform: Rotation: " + gameState.EnemyCoordinateTransform.rotation);
        Debug.Log("GetEnemyTransform: Scale: " + gameState.EnemyCoordinateTransform.localScale);

    }

    private void OnTargetLost()
    {
        gameState.EnemyActive = false;
        gameState.EnemyCoordinateTransform = null;
        Debug.Log($"GetEnemyTransform: EnemyActive: " + gameState.EnemyActive);
        Debug.Log($"GetEnemyTransform: Image target {gameObject.name} lost.");
    }

    void OnDestroy()
    {
        if (observerEventHandler != null)
        {
            observerEventHandler.OnTargetFound.RemoveListener(OnTargetFound);
            observerEventHandler.OnTargetLost.RemoveListener(OnTargetLost);
        }
    }
}
