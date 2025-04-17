using UnityEngine;

public class ProximityProjectile : MonoBehaviour
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

    void Start()
    {
        if (useDefaultTarget)
        {
            defaultTargetObject = new GameObject("DefaultTarget");
            defaultTargetObject.transform.position = defaultTargetPosition;
            targetTransform = defaultTargetObject.transform;
            Debug.Log($"ProximityProjectile: Using default target at {defaultTargetPosition}");
        }
        else
        {
            GameObject targetObj = GameObject.FindWithTag(targetTag);
            if (targetObj != null)
            {
                targetTransform = targetObj.transform;
                Debug.Log($"ProximityProjectile: Found target '{targetObj.name}' with tag '{targetTag}'");
            }
            else
            {
                Debug.LogWarning($"ProximityProjectile: No object with tag '{targetTag}' found.");

                targetObj = GameObject.Find("Enemy");
                if (targetObj != null)
                {
                    targetTransform = targetObj.transform;
                    Debug.Log($"ProximityProjectile: Found target by name 'Enemy'");
                }
                else
                {
                    defaultTargetObject = new GameObject("FallbackTarget");
                    defaultTargetObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
                    targetTransform = defaultTargetObject.transform;
                    Debug.Log($"ProximityProjectile: Created fallback target at {defaultTargetObject.transform.position}");
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
                Debug.Log("ProximityProjectile: Added AudioSource component");
            }
        }

        Invoke(nameof(DestroyProjectile), lifetime);
        Debug.Log($"ProximityProjectile: Projectile will self-destruct in {lifetime} seconds if no hit occurs.");

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
            Debug.Log($"ProximityProjectile: Created new Rigidbody with velocity {rb.linearVelocity}");
        }
        else
        {
            currentSpeed = rb.linearVelocity.magnitude;
            Debug.Log($"ProximityProjectile: Using existing Rigidbody with velocity {rb.linearVelocity} (magnitude: {currentSpeed})");
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
                Debug.Log($"ProximityProjectile: Target hit! Distance: {distanceToTarget:F2}");
                HandleHit();
            }
            else
            {
                Debug.Log($"ProximityProjectile: Reached default target position. Destroying projectile.");
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
            Debug.Log("ProximityProjectile: Disabling gravity as homing begins");
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
            Debug.Log("ProximityProjectile: Playing hit sound");
        }
        else
        {
            Debug.LogWarning("ProximityProjectile: Missing hit sound or audio source");
        }
    }

    void TriggerTargetEffects()
    {
        if (targetTransform == null) return;

        if (useDefaultTarget || targetTransform.gameObject == defaultTargetObject) return;

        targetTransform.SendMessage("OnProjectileHit", this, SendMessageOptions.DontRequireReceiver);

        TargetBehavior targetBehavior = targetTransform.GetComponentInChildren<TargetBehavior>();
        if (targetBehavior != null)
        {
            targetBehavior.OnHit();
        }
        else
        {
            Debug.Log("ProximityProjectile: Target hit but no TargetBehavior component found");
        }

        Debug.Log("ProximityProjectile: Target hit effect triggered!");
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

        Debug.Log("ProximityProjectile: Projectile destroyed.");
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
