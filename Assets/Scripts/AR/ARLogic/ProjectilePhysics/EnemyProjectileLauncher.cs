using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyProjectileSettings
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

public class EnemyProjectileLauncher : MonoBehaviour
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

    [Header("Damage Effect")]
    public TakeDamageScript damageEffectController;


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

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
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
        Debug.Log("EnemyProjectileLauncher: Firing Projectile");
        if (!isReadyToLaunch)
        {
            Debug.Log("EnemyProjectileLauncher: Launcher is cooling down.");
            return;
        }

        isReadyToLaunch = false;
        Invoke(nameof(ResetCooldown), launchInterval);

        Debug.Log("EnemyProjectileLauncher: Preparing to launch projectile...");

        if (!projectilesDictionary.ContainsKey(projectileType))
        {
            Debug.LogError($"EnemyProjectileLauncher: Projectile type '{projectileType}' is not defined.");
            return;
        }

        ProjectileSettings selectedProjectile = projectilesDictionary[projectileType];

        if (selectedProjectile.model == null)
        {
            Debug.LogError($"EnemyProjectileLauncher: Projectile model for '{projectileType}' is missing.");
            return;
        }

        Vector3 launchPosition;

        if (gameState.EnemyCoordinateTransform != null)
        {
            launchPosition = gameState.EnemyCoordinateTransform.position;
            Debug.Log($"EnemyProjectileLauncher: Using enemy position as launch point at {launchPosition}");
        }
        else
        {
            Debug.LogError("EnemyProjectileLauncher: EnemyCoordinateTransform is null. Cannot launch projectile.");
            return;
        }

        selectedProjectile.targetTransform = Camera.main.transform;
        Debug.Log($"EnemyProjectileLauncher: Target set to AR Camera");

        if (activeProjectile != null)
        {
            Debug.Log("EnemyProjectileLauncher: Removing existing projectile...");
            Destroy(activeProjectile);
        }

        GameObject projectileInstance = Instantiate(
            selectedProjectile.model,
            launchPosition,
            Quaternion.identity
        );
        Debug.Log("EnemyProjectileLauncher: Projectile created successfully.");

        PlayLaunchSound(selectedProjectile);

        activeProjectile = projectileInstance;

        Rigidbody projectileRigidbody = activeProjectile.GetComponent<Rigidbody>();
        if (projectileRigidbody == null)
        {
            Debug.LogWarning("EnemyProjectileLauncher: Missing Rigidbody on projectile. Adding one dynamically.");
            projectileRigidbody = activeProjectile.AddComponent<Rigidbody>();
            projectileRigidbody.useGravity = true;
        }

        ProximityProjectile proximityHandler = activeProjectile.GetComponent<ProximityProjectile>();
        if (proximityHandler != null)
        {
            proximityHandler.targetTag = selectedProjectile.targetTag;
            Debug.Log($"EnemyProjectileLauncher: ProximityProjectile component exists, target tag set to: {selectedProjectile.targetTag}");
        }
        else
        {
            proximityHandler = activeProjectile.AddComponent<ProximityProjectile>();
            proximityHandler.targetTag = selectedProjectile.targetTag;
            Debug.Log($"ProjectileLauncher: Added ProximityProjectile component, target tag set to: {selectedProjectile.targetTag}");
        }

        Vector3 targetPosition = selectedProjectile.targetTransform.position;
        proximityHandler.defaultTargetPosition = targetPosition;
        proximityHandler.useDefaultTarget = false; /
        Debug.Log($"EnemyProjectileLauncher: Setting AR Camera as target at {targetPosition}");

        Vector3 toTarget = targetPosition - launchPosition;
        float distanceToTarget = toTarget.magnitude;

        // Create a natural throwing arc based on distance
        float arcHeight = Mathf.Max(0.5f, distanceToTarget * 0.3f); // Higher arc for farther targets
        Vector3 launchDirection = toTarget.normalized;

        Vector3 throwVelocity = launchDirection * selectedProjectile.forwardForce +
                              Vector3.up * (selectedProjectile.verticalForce + arcHeight);

        projectileRigidbody.linearVelocity = throwVelocity;
        Debug.Log($"EnemyProjectileLauncher: Enemy throw velocity: {projectileRigidbody.linearVelocity} (magnitude: {projectileRigidbody.linearVelocity.magnitude})");

        projectileInstance.transform.rotation = Quaternion.LookRotation(throwVelocity);
        Debug.Log($"EnemyProjectileLauncher: Orienting projectile along throw trajectory");

        Vector3 spinAxis = Vector3.Cross(Vector3.up, throwVelocity).normalized;
        projectileRigidbody.AddTorque(spinAxis * selectedProjectile.spinTorque, ForceMode.Impulse);
        Debug.Log($"EnemyProjectileLauncher: Spin torque applied");

        // Start guidance behavior targeting the AR Camera
        if (guidanceCoroutine != null)
        {
            StopCoroutine(guidanceCoroutine);
        }
        guidanceCoroutine = StartCoroutine(ApplyGuidance(projectileRigidbody, selectedProjectile));

        if(gameState.EnemyActive && gameState.PlayerHit)
        {
            damageEffectController.StartDamageEffect();
        }
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
                Debug.Log("EnemyProjectileLauncher: Playing launch sound");
            }
            else
            {
                Debug.LogWarning("EnemyProjectileLauncher: No launch sound assigned");
            }
        }
        else
        {
            Debug.LogError("EnemyProjectileLauncher: No AudioSource available for playing launch sound");
        }
    }

    IEnumerator ApplyGuidance(Rigidbody projectileRigidbody, ProjectileSettings settings)
    {
        float elapsedTime = 0f;

        while (elapsedTime < settings.guidanceDuration)
        {
            if (projectileRigidbody == null || settings.targetTransform == null)
            {
                Debug.LogWarning("EnemyProjectileLauncher: Projectile Rigidbody or target transform destroyed. Stopping guidance.");
                yield break;
            }

            Vector3 targetPos = settings.targetTransform.position;

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
