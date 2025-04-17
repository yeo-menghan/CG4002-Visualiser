using UnityEngine;

public class GetWorldAnchorTransform : MonoBehaviour
{
    private GameState gameState;
    private DefaultObserverEventHandler observerEventHandler;

    void Start()
    {
        gameState = GameState.Instance;
        Debug.Log("GetWorldAnchorTransform: Starting Position: " + transform.position);
        Debug.Log("GetWorldAnchorTransform: Starting Rotation: " + transform.rotation);
        Debug.Log("GetWorldAnchorTransform: Starting Scale: " + transform.localScale);

        observerEventHandler = GetComponent<DefaultObserverEventHandler>();
        if (observerEventHandler == null)
        {
            Debug.LogError("GetWorldAnchorTransform: DefaultObserverEventHandler not found on this GameObject!");
            return;
        }

        observerEventHandler.OnTargetFound.AddListener(OnTargetFound);
        observerEventHandler.OnTargetLost.AddListener(OnTargetLost);
        Debug.Log($"GetWorldAnchorTransform: Initialized on GameObject: {gameObject.name}");
    }

    void Update()
    {
        gameState.WorldCoordinateTransform = transform;
        Debug.Log("GetWorldAnchorTransform: Position: " + gameState.WorldCoordinateTransform.position);
        Debug.Log("GetWorldAnchorTransform: Rotation: " + gameState.WorldCoordinateTransform.rotation);
        Debug.Log("GetWorldAnchorTransform: Scale: " + gameState.WorldCoordinateTransform.localScale);
    }

    private void OnTargetFound()
    {
        gameState.WorldCoordinateTransform = transform;
        Debug.Log("GetWorldAnchorTransform: Position: " + gameState.WorldCoordinateTransform.position);
        Debug.Log("GetWorldAnchorTransform: Rotation: " + gameState.WorldCoordinateTransform.rotation);
        Debug.Log("GetWorldAnchorTransform: Scale: " + gameState.WorldCoordinateTransform.localScale);
    }

    private void OnTargetLost()
    {
        gameState.WorldCoordinateTransform = null;
        Debug.Log($"GetWorldAnchorTransform: Image target {gameObject.name} lost.");
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
