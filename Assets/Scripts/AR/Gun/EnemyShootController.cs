using System.Collections;
using UnityEngine;

public class EnemyShootController : MonoBehaviour
{
    private GameState gameState;

    [Header("References")]
    public Transform muzzlePoint;
    public ParticleSystem muzzleFlash;
    public Camera arCamera;
    private AudioSource audioSource;
    public AudioClip shootSound;

    [Header("Impact Effect")]
    public GameObject impactEffectPrefab;
    public AudioClip impactSound;
    public float impactEffectDuration = 1.5f;
    public float bulletTravelTime = 0.3f; // Time delay before impact effect plays
    public float impactDistanceFromCamera = 1.5f; // Distance to place impact in front of camera

    [Header("Recoil Settings")]
    public float recoilAmount = 0.25f;
    public float backwardRecoilAmount = 0.1f;
    public float recoilRecoverySpeed = 5f;
    public AnimationCurve recoilCurve;

    [Header("Damage Effect")]
    public TakeDamageScript damageEffectController;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isRecoiling = false;
    private bool isSubscribed = false;

    private void Awake()
    {
        // Get game state
        gameState = GameState.Instance;
    }

    private void Start()
    {
        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Make sure muzzle flash system is stopped at start
        if (muzzleFlash != null)
        {
            muzzleFlash.Stop(); // Make sure it's stopped initially
            Debug.Log("Muzzle flash reference found: " + muzzleFlash.name);
        }
        else
        {
            Debug.LogError("Muzzle flash reference is null!");
        }

        // Subscribe to events
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (gameState != null && !isSubscribed)
        {
            gameState.enemyGameActionOccurred.AddListener(OnGameActionOccurred);
            isSubscribed = true;
            Debug.Log("Subscribed to game action events");
        }
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        if (gameState != null && isSubscribed)
        {
            gameState.enemyGameActionOccurred.RemoveListener(OnGameActionOccurred);
            isSubscribed = false;
            Debug.Log("Unsubscribed from game action events");
        }
    }

    // Event handler for game actions
    private void OnGameActionOccurred(string actionType)
    {
        // Check if the action is a shoot action and if the gun is correctly positioned
        if (actionType == "gun" && gameState.EnemyActive && gameState.EnemyBulletCount > 0)
        {
            // Execute the shooting functionality
            FireGun();
            Debug.Log("Firing gun from action: " + actionType);
        }
        else
        {
            Debug.LogWarning("Cannot fire gun: Gun is not yet positioned on enemy");
        }
    }

    private void FireGun()
    {
        // Play sound effect
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        // Trigger muzzle flash
        if (muzzleFlash != null)
        {
            Debug.Log("Playing muzzle flash");
            muzzleFlash.Play(true); // Use Play(true) to ensure it plays from the start
        }


        // Trigger the damage effect
        if (damageEffectController != null && gameState.EnemyActive && gameState.PlayerHit)
        {
            damageEffectController.StartDamageEffect();
            Debug.Log("Started damage effect");
        }
        else
        {
            Debug.LogWarning("Damage effect controller reference is missing!");
        }

        // Trigger recoil animation
        // if (!isRecoiling)
        //     StartCoroutine(RecoilAnimation());
    }

    private IEnumerator PlayImpactEffectDelayed(float delay)
    {
        // Wait for the bullet to "travel" to the target
        yield return new WaitForSeconds(delay);

        // Create impact effect in front of the AR camera
        if (arCamera != null)
        {
            // Calculate position in front of camera
            Vector3 impactPosition = arCamera.transform.position + arCamera.transform.forward * impactDistanceFromCamera;

            // Create the impact effect
            CreateImpactEffect(impactPosition);
            Debug.Log("Impact effect created in front of AR Camera at: " + impactPosition);
        }
    }

    private void CreateImpactEffect(Vector3 position)
    {
        // Create impact effect at the hit point
        if (impactEffectPrefab != null)
        {
            // Create impact effect
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity);

            // Orient the impact effect toward the camera
            impact.transform.LookAt(2 * position - arCamera.transform.position);

            // Play impact sound if available
            if (impactSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(impactSound);
            }

            // Destroy impact effect after duration
            Destroy(impact, impactEffectDuration);
        }
    }

    private IEnumerator RecoilAnimation()
    {
        isRecoiling = true;
        float time = 0;
        float duration = 0.2f; // Recoil duration

        while (time < duration)
        {
            // Calculate progress and use animation curve for more natural motion
            float progress = time / duration;
            float curveValue = recoilCurve.Evaluate(progress);

            // Apply recoil position and rotation with backward movement
            // The negative Y value moves the gun backward in its local space
            Vector3 recoilPosition = originalPosition - new Vector3(
                0,
                backwardRecoilAmount * curveValue, // Backward movement
                0
            );

            // Apply both position and rotation changes
            transform.localPosition = recoilPosition;
            transform.localRotation = originalRotation * Quaternion.Euler(-recoilAmount * 10 * curveValue, 0, 0);

            time += Time.deltaTime;
            yield return null;
        }

        // Recover from recoil
        time = 0;
        duration = 0.3f; // Recovery duration
        Vector3 recoiledPosition = transform.localPosition;
        Quaternion recoiledRotation = transform.localRotation;

        while (time < duration)
        {
            float progress = time / duration;

            // Smoothly interpolate back to original position and rotation
            transform.localPosition = Vector3.Lerp(recoiledPosition, originalPosition, progress);
            transform.localRotation = Quaternion.Lerp(recoiledRotation, originalRotation, progress);

            time += Time.deltaTime;
            yield return null;
        }

        // Ensure we end exactly at original position
        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isRecoiling = false;
    }
}
