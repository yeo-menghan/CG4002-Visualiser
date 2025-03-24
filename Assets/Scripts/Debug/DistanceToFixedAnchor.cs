using UnityEngine;

public class DistanceToFixedAnchor : MonoBehaviour
{
    [Tooltip("Reference to the fixed anchor GameObject")]
    public GameObject fixedAnchor;

    [Tooltip("Show distance in debug logs")]
    public bool showDebugLogs = true;

    [Tooltip("Update frequency in seconds (0 for every frame)")]
    public float updateInterval = 0.5f;

    private float timeSinceLastUpdate = 0f;

    void Start()
    {
        // Find the fixed anchor if not assigned in inspector
        if (fixedAnchor == null)
        {
            fixedAnchor = GameObject.Find("fixed anchor");
            if (fixedAnchor == null)
            {
                Debug.LogError("DistanceToFixedAnchor: Could not find 'fixed anchor' GameObject!");
            }
            else
            {
                Debug.Log("DistanceToFixedAnchor: Found fixed anchor GameObject automatically.");
            }
        }
    }

    void Update()
    {
        timeSinceLastUpdate += Time.deltaTime;

        // Only calculate distance at specified intervals
        if (timeSinceLastUpdate >= updateInterval)
        {
            CalculateDistance();
            timeSinceLastUpdate = 0f;
        }
    }

    void CalculateDistance()
    {
        if (fixedAnchor == null)
        {
            Debug.LogWarning("DistanceToFixedAnchor: Fixed anchor reference is missing!");
            return;
        }

        // Calculate distance between AR camera and fixed anchor
        float distance = Vector3.Distance(transform.position, fixedAnchor.transform.position);

        if (showDebugLogs)
        {
            Debug.Log($"DistanceToFixedAnchor: Distance to fixed anchor: {distance} units");
            Debug.Log($"DistanceToFixedAnchor: AR Camera position: {transform.position}, Fixed Anchor position: {fixedAnchor.transform.position}");
        }
    }

    // Public method to get the current distance (can be called from other scripts)
    public float GetDistanceToFixedAnchor()
    {
        if (fixedAnchor == null) return -1f; // Return negative value to indicate error

        return Vector3.Distance(transform.position, fixedAnchor.transform.position);
    }
}
