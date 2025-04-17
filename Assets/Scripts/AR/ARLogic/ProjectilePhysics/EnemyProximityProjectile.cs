using UnityEngine;

public class EnemyProximityProjectile : MonoBehaviour
{
    [Header("Target Parameters")]
    public string targetTag = "Target";
    public float hitDistance = 0.5f;
    public bool debugMode = true;

    [Header("Default Target")]
    public Vector3 defaultTargetPosition;
    public bool useDefaultTarget = false;

    [Header("Lifetime Settings")]
    public float lifetime = 4f;

    [Header("Homing Settings")]
    public float homingStrength = 5f;
    public float maxSpeed = 10f;
    public float initialSpeed = 5f;
    public float accelerationRate = 1.5f;
    public float homingDelay = 0.5f;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip hitSound;
    public float hitSoundVolume = 1.0f;

    private bool isScheduledForDestruction = false;
    private Transform targetTransform;
    private GameObject defaultTargetObject;
    private Vector3 lastPosition;
    private Rigidbody rb;
    private float currentSpeed;
    private float elapsedTime = 0f;
    private GameState gameState;

    void Start()
    {
        gameState = GameState.Instance;
        if (useDefaultTarget)
        {
            defaultTargetObject = new GameObject("DefaultTarget");
            defaultTargetObject.transform.position = defaultTargetPosition;
            targetTransform = defaultTargetObject.transform;
            Debug.Log($"EnemyProximityProjectile: Using default target at {defaultTargetPosition}");
        }
        else
        {
            GameObject targetObj = GameObject.FindWithTag(targetTag);
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
                Debug.Log($"EnemyProximityProjectile: Found target '{targetObj.name}' with tag '{targetTag}'");
            }
            else
            {
                Debug.LogWarning($"EnemyProximityProjectile: No object with tag '{targetTag}' found.");

                targetObj = GameObject.Find("MainCamera");
                if (targetObj != null)
                {
                    targetTransform = targetObj.transform;
                    Debug.Log($"EnemyProximityProjectile: Found target by name 'Enemy'");
                }
                else
                {
                    defaultTargetObject = new GameObject("FallbackTarget");
                    defaultTargetObject.transform.position = gameState.EnemyCoordinateTransform.position + gameState.EnemyCoordinateTransform.forward * 10;
                    targetTransform = defaultTargetObject.transform;
                    Debug.Log($"EnemyProximityProjectile: Created fallback target at {defaultTargetObject.transform.position}");
                }
            }
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f;
                audioSource.minDistance = 1f;
                audioSource.maxDistance = 20f;
                audioSource.playOnAwake = false;
                Debug.Log("EnemyProximityProjectile: Added AudioSource component");
            }
        }

        Invoke(nameof(DestroyProjectile), lifetime);
        Debug.Log($"EnemyProximityProjectile: Projectile will self-destruct in {lifetime} seconds if no hit occurs.");

        lastPosition = transform.position;

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.linearDamping = 0.5f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            currentSpeed = initialSpeed;
            rb.linearVelocity = transform.forward * currentSpeed;
            Debug.Log($"EnemyProximityProjectile: Created new Rigidbody with velocity {rb.linearVelocity}");
        }
        else
        {
            currentSpeed = rb.linearVelocity.magnitude;
            Debug.Log($"EnemyProximityProjectile: Using existing Rigidbody with velocity {rb.linearVelocity} (magnitude: {currentSpeed})");
        }
    }

    void Update()
    {
        if (isScheduledForDestruction || targetTransform == null) return;

        elapsedTime += Time.deltaTime;
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

        if (distanceToTarget <= hitDistance)
        {
            if (!useDefaultTarget && targetTransform.gameObject != defaultTargetObject)
            {
                Debug.Log($"EnemyProximityProjectile: Target hit! Distance: {distanceToTarget:F2}");
                HandleHit();
            }
            else
            {
                Debug.Log($"EnemyProximityProjectile: Reached default target position. Destroying projectile.");
                DestroyProjectile();
            }
        }
    }

    void FixedUpdate()
    {
        if (isScheduledForDestruction || targetTransform == null) return;

        if (elapsedTime < homingDelay)
        {
            return;
        }

        if (rb.useGravity)
        {
            rb.useGravity = false;
            Debug.Log("EnemyProximityProjectile: Disabling gravity as homing begins");
        }

        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        currentSpeed = Mathf.Min(currentSpeed + (accelerationRate * Time.fixedDeltaTime), maxSpeed);
        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, directionToTarget * currentSpeed, homingStrength * Time.fixedDeltaTime);

        if (rb.linearVelocity.magnitude < initialSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * initialSpeed;
        }

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
        }
    }

    void HandleHit()
    {
        PlayHitSound();
        TriggerTargetEffects();
        DestroyProjectile();
    }

    void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
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

        if (useDefaultTarget || targetTransform.gameObject == defaultTargetObject) return;

        targetTransform.SendMessage("EnemyProximityProjectile: OnProjectileHit", this, SendMessageOptions.DontRequireReceiver);
        TargetBehavior targetBehavior = targetTransform.GetComponentInChildren<TargetBehavior>();
        if (targetBehavior != null)
        {
            targetBehavior.OnHit();
        }
        else
        {
            Debug.Log("EnemyProximityProjectile: Target hit but no TargetBehavior component found");
        }

        Debug.Log("EnemyProximityProjectile: Target hit effect triggered!");
    }

    void DestroyProjectile()
    {
        if (isScheduledForDestruction) return;

        isScheduledForDestruction = true;
        CancelInvoke();

        if (defaultTargetObject != null)
        {
            Destroy(defaultTargetObject);
        }

        Debug.Log("EnemyProximityProjectile: Projectile destroyed.");
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (defaultTargetObject != null)
        {
            Destroy(defaultTargetObject);
        }
    }
}
