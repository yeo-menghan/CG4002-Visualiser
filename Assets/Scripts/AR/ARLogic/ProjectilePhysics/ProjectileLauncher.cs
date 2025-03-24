using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ProjectileSettings
{
    public string projectileType;
    public GameObject model;

    [Header("Launch Parameters")]
    public Transform launchPosition;
    public Transform targetTransform;
    public float forwardForce = 4f;
    public float verticalForce = 4f;
    public float guidanceForce = 7f;
    public float guidanceDuration = 2f;

    [Header("Spin Parameters")]
    public float spinTorque = 10f;

    [Header("Targeting Parameters")]
    public string targetTag = "Target";

    [Header("Left Hand Throw Settings")]
    public Vector3 leftHandOffset = new Vector3(-0.3f, -0.4f, 0.2f); // Left, down, and slightly forward

    [Header("Audio Settings")]
    public AudioClip launchSound; // Sound to play when projectile is launched

    // [Header("Action Wait")]
    // public GameObject ActionWaitBar;
}

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public List<ProjectileSettings> projectileList = new List<ProjectileSettings>(); // Assign projectiles in Inspector
    private Dictionary<string, ProjectileSettings> projectilesDictionary = new Dictionary<string, ProjectileSettings>();

    [Header("Cooldown Settings")]
    public float launchInterval = 0.5f;  // Delay between launches
    private bool isReadyToLaunch = true;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Audio source for playing sounds
    public AudioClip defaultLaunchSound; // Default launch sound if none specified in projectile settings

    private GameObject activeProjectile;
    private Coroutine guidanceCoroutine;
    private GameState gameState;

    void Awake()
    {
        // Map projectiles from list to dictionary
        foreach (var projectile in projectileList)
        {
            if (!projectilesDictionary.ContainsKey(projectile.projectileType))
            {
                projectilesDictionary.Add(projectile.projectileType, projectile);
            }
        }

        // Create audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.minDistance = 1f;
            audioSource.maxDistance = 20f;
        }

    }

    void Start()
    {
        gameState = GameState.Instance;
    }

    public void FireProjectile(string projectileType)
    {
        Debug.Log("ProjectileLauncher: Firing Projectile");
        if (!isReadyToLaunch)
        {
            Debug.Log("Launcher is cooling down.");
            return;
        }

        isReadyToLaunch = false;
        Invoke(nameof(ResetCooldown), launchInterval);

        Debug.Log("Preparing to launch projectile...");

        // Check if projectile type is valid
        if (!projectilesDictionary.ContainsKey(projectileType))
        {
            Debug.LogError($"Projectile type '{projectileType}' is not defined.");
            return;
        }

        ProjectileSettings selectedProjectile = projectilesDictionary[projectileType];

        // Validate model and launch position
        if (selectedProjectile.model == null)
        {
            Debug.LogError($"Projectile model for '{projectileType}' is missing.");
            return;
        }

        // Set up left hand throwing position
        Vector3 leftHandOffset = new Vector3(-0.3f, -0.4f, 0.2f); // Left, down, and slightly forward
        Vector3 launchPosition;

        if (selectedProjectile.launchPosition != null)
        {
            launchPosition = selectedProjectile.launchPosition.position;
        }
        else
        {
            // Use camera-relative position to simulate left hand
            launchPosition = Camera.main.transform.position +
                            Camera.main.transform.right * leftHandOffset.x +
                            Camera.main.transform.up * leftHandOffset.y +
                            Camera.main.transform.forward * leftHandOffset.z;
            Debug.Log($"ProjectileLauncher: Using left hand throw position at {launchPosition}");
        }

        // Destroy current projectile if it exists
        if (activeProjectile != null)
        {
            Debug.Log("Removing existing projectile...");
            Destroy(activeProjectile);
        }

        // Instantiate new projectile at the launch position
        GameObject projectileInstance = Instantiate(
            selectedProjectile.model,
            launchPosition,
            Quaternion.identity
        );
        Debug.Log("Projectile created successfully.");

        // Play launch sound
        PlayLaunchSound(selectedProjectile);

        activeProjectile = projectileInstance;

        // Ensure projectile has a Rigidbody component
        Rigidbody projectileRigidbody = activeProjectile.GetComponent<Rigidbody>();
        if (projectileRigidbody == null)
        {
            Debug.LogWarning("ProjectileLauncher: Missing Rigidbody on projectile. Adding one dynamically.");
            projectileRigidbody = activeProjectile.AddComponent<Rigidbody>();
            projectileRigidbody.useGravity = true;
        }

        ProximityProjectile proximityHandler = activeProjectile.GetComponent<ProximityProjectile>();
        if (proximityHandler != null)
        {
            proximityHandler.targetTag = selectedProjectile.targetTag;
            Debug.Log($"ProjectileLauncher: ProximityProjectile component exists, target tag set to: {selectedProjectile.targetTag}");
        }
        else
        {
            proximityHandler = activeProjectile.AddComponent<ProximityProjectile>();
            proximityHandler.targetTag = selectedProjectile.targetTag;
            Debug.Log($"ProjectileLauncher: Added ProximityProjectile component, target tag set to: {selectedProjectile.targetTag}");
        }

        // Determine target position
        Vector3 targetPosition;
        Transform targetEntity = null;

        if (!gameState.EnemyActive)
        {
            // Default target: position 4 units forward from the camera
            targetPosition = Camera.main.transform.position +
                              Camera.main.transform.forward * 4 -
                              Camera.main.transform.up * 3;
            proximityHandler.defaultTargetPosition = targetPosition;
            proximityHandler.useDefaultTarget = true;
            Debug.Log($"ProjectileLauncher: Setting default target at {targetPosition} for proximity projectile");
        }
        else
        {
            // Use assigned targetTransform when enemy is visible
            if (gameState.EnemyCoordinateTransform != null)
            {
                selectedProjectile.targetTransform = gameState.EnemyCoordinateTransform;
                Debug.LogWarning($"ProjectileLauncher: TargetTransform was not set; assigned dynamically from GameState instance at {selectedProjectile.targetTransform}.");
                targetEntity = selectedProjectile.targetTransform;
                targetPosition = targetEntity.position;
                proximityHandler.useDefaultTarget = false;
                Debug.Log($"ProjectileLauncher: Using enemy target at {targetPosition}");
            }
            else
            {
                Debug.LogError("ProjectileLauncher: Cannot assign TargetTransform dynamically because GameState.Instance.EnemyCoordinateTransform is null. Creating default target instead.");
                // Fall back to default target
                targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 4;
                proximityHandler.defaultTargetPosition = targetPosition;
                proximityHandler.useDefaultTarget = true;
                Debug.Log($"ProjectileLauncher: Setting fallback target at {targetPosition}");
            }
        }

        // Calculate throw trajectory for left-handed throw
        Vector3 toTarget = targetPosition - launchPosition;
        float distanceToTarget = toTarget.magnitude;

        // Create a natural throwing arc based on distance
        float arcHeight = Mathf.Max(0.5f, distanceToTarget * 0.3f); // Higher arc for farther targets
        Vector3 launchDirection = toTarget.normalized;

        // Calculate throw velocity with extra vertical component for arc
        Vector3 throwVelocity = launchDirection * selectedProjectile.forwardForce +
                              Vector3.up * (selectedProjectile.verticalForce + arcHeight);

        // Apply the throwing velocity
        projectileRigidbody.linearVelocity = throwVelocity;
        Debug.Log($"ProjectileLauncher: Left hand throw velocity: {projectileRigidbody.linearVelocity} (magnitude: {projectileRigidbody.linearVelocity.magnitude})");

        // Orient projectile along initial trajectory
        projectileInstance.transform.rotation = Quaternion.LookRotation(throwVelocity);
        Debug.Log($"ProjectileLauncher: Orienting projectile along throw trajectory");

        // Apply spin torque for a left-handed throw (more realistic spin)
        Vector3 spinAxis = Vector3.Cross(Vector3.up, throwVelocity).normalized;
        projectileRigidbody.AddTorque(spinAxis * selectedProjectile.spinTorque, ForceMode.Impulse);
        Debug.Log($"ProjectileLauncher: Spin torque applied for left-handed throw");

        // Start guidance behavior
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
        }
        guidanceCoroutine = StartCoroutine(ApplyGuidance(projectileRigidbody, selectedProjectile, targetPosition, targetEntity));

        // TODO: insert ActionWaitBar
    }

    private void PlayLaunchSound(ProjectileSettings projectile)
    {
        if (audioSource != null)
        {
            // Use projectile-specific sound if available, otherwise use default
            AudioClip soundToPlay = projectile.launchSound != null ? projectile.launchSound : defaultLaunchSound;

            if (soundToPlay != null)
            {
                audioSource.clip = soundToPlay;
                audioSource.Play();
                Debug.Log("ProjectileLauncher: Playing launch sound");
            }
            else
            {
                Debug.LogWarning("ProjectileLauncher: No launch sound assigned");
            }
        }
        else
        {
            Debug.LogError("ProjectileLauncher: No AudioSource available for playing launch sound");
        }

        // ActionWaitBar.StartWait(); // 1 second wait action
    }

    IEnumerator ApplyGuidance(Rigidbody projectileRigidbody, ProjectileSettings settings, Vector3 initialTarget, Transform movingTarget)
    {
        float elapsedTime = 0f;

        while (elapsedTime < settings.guidanceDuration)
        {
            if (projectileRigidbody == null)
            {
                Debug.LogWarning("ProjectileLauncher: Projectile Rigidbody destroyed. Stopping guidance.");
                yield break;
            }

            Vector3 targetPos = initialTarget;

            if (gameState.EnemyActive && movingTarget != null)
            {
                targetPos = movingTarget.position;
            }

            Vector3 guidanceDirection = (targetPos - projectileRigidbody.position).normalized;
            projectileRigidbody.linearVelocity += guidanceDirection * settings.guidanceForce * Time.deltaTime;

            projectileRigidbody.linearVelocity = Vector3.ClampMagnitude(projectileRigidbody.linearVelocity, settings.forwardForce * 2);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    void ResetCooldown()
    {
        isReadyToLaunch = true;
    }
}
