using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ProjectileTrailRenderer : MonoBehaviour
{
    [Header("Trail Settings")]
    [Tooltip("Maximum number of points to display in the trail.")]
    public int maxTrailPoints = 15;
    [Tooltip("Time interval (in seconds) between recording positions.")]
    public float recordInterval = 0.05f;
    private LineRenderer lineRenderer;
    private List<Vector3> trailPoints = new List<Vector3>();
    private float timeSinceLastRecord = 0f;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            Debug.LogError("No LineRenderer component found.");
        }
    }

    void Update()
    {
        timeSinceLastRecord += Time.deltaTime;
        if (timeSinceLastRecord >= recordInterval)
        {
            RecordTrailPoint();
            timeSinceLastRecord = 0f;
        }
        RenderTrail();
    }

    // Record current position and keep list size to maxTrailPoints.
    private void RecordTrailPoint()
    {
        trailPoints.Add(transform.position);
        if (trailPoints.Count > maxTrailPoints)
        {
            trailPoints.RemoveAt(0);
        }
    }

    // Update the LineRenderer with the stored points.
    private void RenderTrail()
    {
        lineRenderer.positionCount = trailPoints.Count;
        lineRenderer.SetPositions(trailPoints.ToArray());
    }
}
