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

    [Header("Audio Settings")]
    public AudioClip launchSound;
}

public class ProjectileLauncher : MonoBehaviour
{
    [Header("Projectile Settings")]
    public List<ProjectileSettings> projectileList = new List<ProjectileSettings>();
    private Dictionary<string, ProjectileSettings> projectilesDictionary = new Dictionary<string, ProjectileSettings>();

    [Header("Cooldown Settings")]
    public float launchInterval = 0.5f;
    private bool isReadyToLaunch = true;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip defaultLaunchSound;
    private GameObject activeProjectile;
    private Coroutine guidanceCoroutine;
    private GameState gameState;

    void Awake()
    {
        foreach (var projectile in projectileList)
        {
            if (!projectilesDictionary.ContainsKey(projectile.projectileType))
            {
                projectilesDictionary.Add(projectile.projectileType, projectile);
            }
        }

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

        if (!projectilesDictionary.ContainsKey(projectileType))
        {
            Debug.LogError($"Projectile type '{projectileType}' is not defined.");
            return;
        }

        ProjectileSettings selectedProjectile = projectilesDictionary[projectileType];

        if (selectedProjectile.model == null)
        {
            Debug.LogError($"Projectile model for '{projectileType}' is missing.");
            return;
        }

        Vector3 leftHandOffset = new Vector3(-0.3f, -0.4f, 0.2f); // Left, down, forward
        Vector3 launchPosition;

        if (selectedProjectile.launchPosition != null)
        {
            launchPosition = selectedProjectile.launchPosition.position;
        }
        else
        {
            launchPosition = Camera.main.transform.position +
                            Camera.main.transform.right * leftHandOffset.x +
                            Camera.main.transform.up * leftHandOffset.y +
                            Camera.main.transform.forward * leftHandOffset.z;
            Debug.Log($"ProjectileLauncher: Using left hand throw position at {launchPosition}");
        }

        if (activeProjectile != null)
        {
            Debug.Log("Removing existing projectile...");
            Destroy(activeProjectile);
        }

        GameObject projectileInstance = Instantiate(
            selectedProjectile.model,
            launchPosition,
            Quaternion.identity
        );
        Debug.Log("Projectile created successfully.");

        PlayLaunchSound(selectedProjectile);

        activeProjectile = projectileInstance;

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

        Vector3 targetPosition;
        Transform targetEntity = null;

        if (!gameState.EnemyActive)
        {
            targetPosition = Camera.main.transform.position +
                              Camera.main.transform.forward * 4 -
                              Camera.main.transform.up * 3;
            proximityHandler.defaultTargetPosition = targetPosition;
            proximityHandler.useDefaultTarget = true;
            Debug.Log($"ProjectileLauncher: Setting default target at {targetPosition} for proximity projectile");
        }
        else
        {
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
                targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 4;
                proximityHandler.defaultTargetPosition = targetPosition;
                proximityHandler.useDefaultTarget = true;
                Debug.Log($"ProjectileLauncher: Setting fallback target at {targetPosition}");
            }
        }

        Vector3 toTarget = targetPosition - launchPosition;
        float distanceToTarget = toTarget.magnitude;

        float arcHeight = Mathf.Max(0.5f, distanceToTarget * 0.3f);
        Vector3 launchDirection = toTarget.normalized;

        Vector3 throwVelocity = launchDirection * selectedProjectile.forwardForce +
                              Vector3.up * (selectedProjectile.verticalForce + arcHeight);

        projectileRigidbody.linearVelocity = throwVelocity;
        Debug.Log($"ProjectileLauncher: Left hand throw velocity: {projectileRigidbody.linearVelocity} (magnitude: {projectileRigidbody.linearVelocity.magnitude})");

        projectileInstance.transform.rotation = Quaternion.LookRotation(throwVelocity);
        Debug.Log($"ProjectileLauncher: Orienting projectile along throw trajectory");

        Vector3 spinAxis = Vector3.Cross(Vector3.up, throwVelocity).normalized;
        projectileRigidbody.AddTorque(spinAxis * selectedProjectile.spinTorque, ForceMode.Impulse);
        Debug.Log($"ProjectileLauncher: Spin torque applied for left-handed throw");

        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
        }
        guidanceCoroutine = StartCoroutine(ApplyGuidance(projectileRigidbody, selectedProjectile, targetPosition, targetEntity));
    }

    private void PlayLaunchSound(ProjectileSettings projectile)
    {
        if (audioSource != null)
        {
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
