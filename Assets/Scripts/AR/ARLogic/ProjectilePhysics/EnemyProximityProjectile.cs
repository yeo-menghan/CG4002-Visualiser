using UnityEngine;

public class EnemyProximityProjectile : MonoBehaviour
{
    [Header("Target Parameters")]
    public string targetTag = "Target";
    public float hitDistance = 0.5f; // Distance threshold for "hit" detection
    public bool debugMode = true;    // Enable for visual debugging

    [Header("Default Target")]
    public Vector3 defaultTargetPosition; // Set by ProjectileLauncher
    public bool useDefaultTarget = false; // Flag set by ProjectileLauncher

    [Header("Lifetime Settings")]
    public float lifetime = 4f;

    [Header("Homing Settings")]
    public float homingStrength = 5f;       // How strongly to home toward target
    public float maxSpeed = 10f;            // Maximum speed
    public float initialSpeed = 5f;         // Starting speed
    public float accelerationRate = 1.5f;   // How quickly to accelerate
    public float homingDelay = 0.5f;        // Brief delay before homing starts

    [Header("Audio Settings")]
    public AudioSource audioSource;         // Audio source component
    public AudioClip hitSound;              // Sound to play when projectile hits
    public float hitSoundVolume = 1.0f;     // Volume for hit sound

    private bool isScheduledForDestruction = false;
    private Transform targetTransform;
    private GameObject defaultTargetObject; // For holding the default target
    private Vector3 lastPosition;
    private Rigidbody rb;
    private float currentSpeed;
    private float elapsedTime = 0f;
    private GameState gameState;

    void Start()
    {
        gameState = GameState.Instance;
        // If we're using a default target (no enemy visible), create a target object at that position
        if (useDefaultTarget)
        {
            // Create a temporary gameobject to serve as the target
            defaultTargetObject = new GameObject("DefaultTarget");
            defaultTargetObject.transform.position = defaultTargetPosition;
            targetTransform = defaultTargetObject.transform;
            Debug.Log($"EnemyProximityProjectile: Using default target at {defaultTargetPosition}");
        }
        else
        {
            // Find target by tag as before
            GameObject targetObj = GameObject.FindWithTag(targetTag);
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
                Debug.Log($"EnemyProximityProjectile: Found target '{targetObj.name}' with tag '{targetTag}'");
            }
            else
            {
                Debug.LogWarning($"EnemyProximityProjectile: No object with tag '{targetTag}' found.");

                // Optionally find by name if tag isn't set
                targetObj = GameObject.Find("MainCamera");
                if (targetObj != null)
                {
                    targetTransform = targetObj.transform;
                    Debug.Log($"EnemyProximityProjectile: Found target by name 'Enemy'");
                }
                else
                {
                    // If we still couldn't find a target, create a default one
                    // This ensures the projectile will always have somewhere to go
                    defaultTargetObject = new GameObject("FallbackTarget");
                    defaultTargetObject.transform.position = gameState.EnemyCoordinateTransform.position + gameState.EnemyCoordinateTransform.forward * 10;
                    targetTransform = defaultTargetObject.transform;
                    Debug.Log($"EnemyProximityProjectile: Created fallback target at {defaultTargetObject.transform.position}");
                }
            }
        }

        // Set up audio source if needed
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // Make sound 3D
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 20f;
                audioSource.playOnAwake = false;
                Debug.Log("EnemyProximityProjectile: Added AudioSource component");
            }
        }

        // Schedule destruction after lifetime
        Invoke(nameof(DestroyProjectile), lifetime);
        Debug.Log($"EnemyProximityProjectile: Projectile will self-destruct in {lifetime} seconds if no hit occurs.");

        lastPosition = transform.position;

        // Get rigidbody but preserve its initial velocity if it exists
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;  // Disable gravity for better homing
            rb.linearDamping = 0.5f;         // Add some drag for stability
            rb.interpolation = RigidbodyInterpolation.Interpolate;  // Smoother movement

            // Only set initial velocity if it's a newly created Rigidbody
            currentSpeed = initialSpeed;
            rb.linearVelocity = transform.forward * currentSpeed;
            Debug.Log($"EnemyProximityProjectile: Created new Rigidbody with velocity {rb.linearVelocity}");
        }
        else
        {
            // Use existing velocity from launcher
            currentSpeed = rb.linearVelocity.magnitude;
            Debug.Log($"EnemyProximityProjectile: Using existing Rigidbody with velocity {rb.linearVelocity} (magnitude: {currentSpeed})");
        }
    }

    void Update()
    {
        if (isScheduledForDestruction || targetTransform == null) return;

        elapsedTime += Time.deltaTime;

        // Draw trajectory line
        if (debugMode)
        {
            Debug.DrawLine(lastPosition, transform.position, Color.yellow, 0.5f);
            lastPosition = transform.position;
        }

        // Calculate distance to target
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        // Optional debug visualization
        if (debugMode && Time.frameCount % 10 == 0)
        {
            Debug.Log($"EnemyProximityProjectile: Distance to target: {distanceToTarget:F2} units");
            Debug.DrawLine(transform.position, targetTransform.position,
                (distanceToTarget <= hitDistance) ? Color.green : Color.red, 0.1f);
        }

        // Check if projectile is close enough to target
        if (distanceToTarget <= hitDistance)
        {
            // Only consider it a hit if it's not the default target and it has a tag
            if (!useDefaultTarget && targetTransform.gameObject != defaultTargetObject)
            {
                Debug.Log($"EnemyProximityProjectile: Target hit! Distance: {distanceToTarget:F2}");
                HandleHit();
            }
            else
            {
                // For default targets, just destroy the projectile
                Debug.Log($"EnemyProximityProjectile: Reached default target position. Destroying projectile.");
                DestroyProjectile();
            }
        }
    }

    void FixedUpdate()
    {
        if (isScheduledForDestruction || targetTransform == null) return;

        // Wait for delay before applying homing
        if (elapsedTime < homingDelay)
        {
            // During initial launch phase, let gravity do its thing for a natural arc
            return;
        }

        // After delay - disable gravity when homing starts for better control
        if (rb.useGravity)
        {
            rb.useGravity = false;
            Debug.Log("EnemyProximityProjectile: Disabling gravity as homing begins");
        }

        // Calculate direction to target
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;

        // Gradually increase speed for more aggressive homing
        currentSpeed = Mathf.Min(currentSpeed + (accelerationRate * Time.fixedDeltaTime), maxSpeed);

        // Apply homing force (stronger than before)
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, directionToTarget * currentSpeed, homingStrength * Time.fixedDeltaTime);

        // Ensure projectile doesn't go too slow
        if (rb.linearVelocity.magnitude < initialSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
        }

        // Orient projectile in the direction it's moving
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    void HandleHit()
    {
        // Play hit sound
        PlayHitSound();

        // Trigger any effects on the target
        TriggerTargetEffects();

        // Destroy the projectile
        DestroyProjectile();
    }

    void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            // Use AudioSource.PlayClipAtPoint for one-shot audio that continues after object destruction
            AudioSource.PlayClipAtPoint(hitSound, transform.position, hitSoundVolume);
            Debug.Log("EnemyProximityProjectile: Playing hit sound");
        }
        else
        {
            Debug.LogWarning("EnemyProximityProjectile: Missing hit sound or audio source");
        }
    }

    void TriggerTargetEffects()
    {
        if (targetTransform == null) return;

        // Don't trigger effects for default targets
        if (useDefaultTarget || targetTransform.gameObject == defaultTargetObject) return;

        // Option 1: Send message to target
        targetTransform.SendMessage("EnemyProximityProjectile: OnProjectileHit", this, SendMessageOptions.DontRequireReceiver);

        // Option 2: Find and call specific component
        TargetBehavior targetBehavior = targetTransform.GetComponentInChildren<TargetBehavior>();
        if (targetBehavior != null)
        {
            targetBehavior.OnHit();
        }
        else
        {
            Debug.Log("EnemyProximityProjectile: Target hit but no TargetBehavior component found");
        }

        // Visual feedback
        Debug.Log("EnemyProximityProjectile: Target hit effect triggered!");

        // Add any particle effects/sounds here
    }

    void DestroyProjectile()
    {
        if (isScheduledForDestruction) return;

        isScheduledForDestruction = true;
        CancelInvoke();

        // Clean up any temporary objects we created
        if (defaultTargetObject != null)
        {
            Destroy(defaultTargetObject);
        }

        Debug.Log("EnemyProximityProjectile: Projectile destroyed.");
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        // Debug visualization in editor
        if (targetTransform != null && debugMode)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            float distance = Vector3.Distance(transform.position, targetTransform.position);
            Gizmos.color = (distance <= hitDistance) ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, targetTransform.position);
        }
    }

    // Called on object destruction to clean up
    void OnDestroy()
    {
        if (defaultTargetObject != null)
        {
            Destroy(defaultTargetObject);
        }
    }
}
