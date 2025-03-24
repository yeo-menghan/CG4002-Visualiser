using System.Collections;
using UnityEngine;

public class ShootController : MonoBehaviour
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

    [Header("Recoil Settings")]
    public float recoilAmount = 0.25f;
    public float backwardRecoilAmount = 0.1f; // Added backward recoil amount
    public float recoilRecoverySpeed = 5f;
    public AnimationCurve recoilCurve;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isRecoiling = false;
    private bool isSubscribed = false;

    private void Start()
    {
        // Store original position and rotation for recoil animation
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

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

        // Get game state and subscribe to events
        gameState = GameState.Instance;
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        if (gameState != null && !isSubscribed)
        {
            gameState.gameActionOccurred.AddListener(OnGameActionOccurred);
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
            gameState.gameActionOccurred.RemoveListener(OnGameActionOccurred);
            isSubscribed = false;
            Debug.Log("Unsubscribed from game action events");
        }
    }

    // Event handler for game actions
    private void OnGameActionOccurred(string actionType)
    {
        // Check if the action is a shoot action
        if (actionType == "gun" && gameState.PlayerBulletCount > 0)
        {
            // Execute the shooting functionality
            FireGun();
            Debug.Log("Firing gun from action: " + actionType);
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

        // Check if we have an enemy target to create impact effect
        if (gameState != null && gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            // Debug info
            Debug.Log("Target position: " + gameState.EnemyCoordinateTransform.position);
            Debug.Log("Muzzle position: " + muzzlePoint.position);

            // Visualize the trajectory with a debug line
            Debug.DrawLine(muzzlePoint.position, gameState.EnemyCoordinateTransform.position, Color.red, 2.0f);

            // Start coroutine to play impact effect after bullet travel time
            StartCoroutine(PlayImpactEffectDelayed(bulletTravelTime));
        }
        else
        {
            Debug.Log("No enemy target for impact effect");
        }

        // Trigger recoil animation
        if (!isRecoiling)
            StartCoroutine(RecoilAnimation());
    }

    private IEnumerator PlayImpactEffectDelayed(float delay)
    {
        // Wait for the bullet to "travel" to the target
        yield return new WaitForSeconds(delay);

        // Create impact effect at the enemy transform
        if (gameState != null && gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            CreateImpactEffect(gameState.EnemyCoordinateTransform.position, gameState.EnemyCoordinateTransform);
            Debug.Log("Impact effect created at enemy position");
        }
    }

    private void CreateImpactEffect(Vector3 position, Transform hitTransform)
    {
        // Create impact effect at the hit point
        if (impactEffectPrefab != null)
        {
            // Attach impact effect to the hit transform
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity, hitTransform);

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
            // The negative Z value moves the gun backward in its local space
            Vector3 recoilPosition = originalPosition - new Vector3(
                0,
                0,
                backwardRecoilAmount * curveValue // Added backward movement
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
