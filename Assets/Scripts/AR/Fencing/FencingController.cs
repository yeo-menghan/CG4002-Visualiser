using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class FencingController : MonoBehaviour
{
    [Header("Sword Reference")]
    public Transform sword;

    [Header("Animation Settings")]
    public float enterDuration = 0.5f;
    public float firstJabDuration = 0.2f;
    public float firstJabDistance = 0.3f;
    public float secondJabDuration = 0.4f;
    public float secondJabDistance = 0.7f;
    public float returnDelay = 0.2f;
    public float exitDuration = 0.5f;

    [Header("Jab Motion Settings")]
    public float jabRotationAngle = 15f; // Rotation angle in degrees
    public Vector3 swordRotationAxis = new Vector3(0, 0, -1); // Negative Z-axis rotation for opposite direction

    [Header("Effect Settings")]
    public GameObject slashEffect;
    public float effectDuration = 1.0f; // Duration before the effect is destroyed
    public AudioClip normalSlashSound;
    public AudioClip enemyHitSound;

    private GameState gameState;

    private Vector3 swordStartPos;
    private Vector3 swordOffscreenPos;
    private Quaternion swordStartRot;
    private AudioSource audioSource;
    private bool isAnimating = false;

    private void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(OnGameAction);

        // Ensure sword renderer is enabled
        if (sword != null)
        {
            Renderer swordRenderer = sword.GetComponent<Renderer>();
            if (swordRenderer) swordRenderer.enabled = true;
        }

        // Store initial position and rotation
        swordStartPos = sword.localPosition;
        swordStartRot = sword.localRotation;

        // Set offscreen position (bottom of screen)
        swordOffscreenPos = new Vector3(swordStartPos.x, -60f, swordStartPos.z);

        // Position sword offscreen initially
        sword.localPosition = swordOffscreenPos;

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Debug log to verify initialization
        Debug.Log("Sword Controller initialized. Sword at: " + sword.localPosition);
    }

    private void OnDestroy()
    {
        // Clean up event listener when the object is destroyed
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    // Event handler for game actions
    private void OnGameAction(string actionType)
    {
        // Check if the action type is "sword"
        if (actionType == "fencing" && !isAnimating)
        {
            Debug.Log("Fencing action received, playing sequence");
            PlaySwordSequence();
        }
    }

    public void PlaySwordSequence()
    {
        // Prevent multiple animations from running
        if (isAnimating)
        {
            Debug.Log("Already animating, ignoring request");
            return;
        }

        StartCoroutine(SwordSequenceCoroutine());
    }

    private IEnumerator SwordSequenceCoroutine()
    {
        isAnimating = true;
        Debug.Log("Starting sword sequence");

        // Move sword into view
        yield return StartCoroutine(MoveSwordIn());

        // First jab (shorter)
        yield return StartCoroutine(JabSword(firstJabDuration, firstJabDistance));

        // Second jab (longer)
        yield return StartCoroutine(JabSword(secondJabDuration, secondJabDistance));

        // Move sword out of view
        yield return StartCoroutine(MoveSwordOut());

        isAnimating = false;
        Debug.Log("Sword sequence completed");
    }

    private IEnumerator MoveSwordIn()
    {
        Debug.Log("Moving sword in from: " + sword.localPosition);
        float timer = 0f;

        // Reset rotation to starting orientation
        sword.localRotation = swordStartRot;

        while (timer < enterDuration)
        {
            float t = timer / enterDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing

            sword.localPosition = Vector3.Lerp(swordOffscreenPos, swordStartPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure sword is exactly at start position
        sword.localPosition = swordStartPos;
        Debug.Log("Sword moved to start position: " + sword.localPosition);
    }

    private IEnumerator JabSword(float jabDuration, float jabDistance)
    {
        Vector3 startPos = sword.localPosition;
        Vector3 jabDirection = Vector3.forward;
        Vector3 jabPos = startPos + jabDirection * jabDistance;

        // Calculate jab rotation - now in the opposite direction with negative angle
        Quaternion jabRotation = Quaternion.AngleAxis(-jabRotationAngle, swordRotationAxis) * swordStartRot;

        Debug.Log("Sword jabbing from " + startPos + " to " + jabPos + " with rotation");

        // Jab forward with rotation
        float timer = 0f;
        while (timer < jabDuration)
        {
            float t = timer / jabDuration;
            t = Mathf.SmoothStep(0, 1, t); // Quick acceleration

            // Position interpolation
            sword.localPosition = Vector3.Lerp(startPos, jabPos, t);

            // Rotation interpolation
            sword.localRotation = Quaternion.Slerp(swordStartRot, jabRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure sword is at full jab position and rotation
        sword.localPosition = jabPos;
        sword.localRotation = jabRotation;

        // Play sound effect and create slash effect
        if (gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            // Play enemy hit sound
            audioSource.PlayOneShot(enemyHitSound);

            // Instantiate hit effect at enemy position with auto-destruction
            GameObject effect = Instantiate(slashEffect, gameState.EnemyCoordinateTransform.position, Quaternion.identity);

            // Make sure the effect doesn't loop and gets destroyed after a set time
            ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                // Make the particle system non-looping
                var main = particleSystem.main;
                main.loop = false;
            }

            // Destroy the effect after a set duration
            Destroy(effect, effectDuration);

            Debug.Log("Created slash effect at enemy position, set to destroy in " + effectDuration + " seconds");
        }
        else
        {
            // Play normal slash sound
            audioSource.PlayOneShot(normalSlashSound);
            Debug.Log("No enemy active, playing normal slash sound");
        }

        // Slight delay at full extension
        yield return new WaitForSeconds(returnDelay);

        // Return to starting position and rotation
        timer = 0f;
        while (timer < jabDuration)
        {
            float t = timer / jabDuration;
            t = Mathf.SmoothStep(0, 1, t); // Quick acceleration

            // Position interpolation
            sword.localPosition = Vector3.Lerp(jabPos, startPos, t);

            // Rotation interpolation
            sword.localRotation = Quaternion.Slerp(jabRotation, swordStartRot, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure sword is exactly at start position and rotation
        sword.localPosition = startPos;
        sword.localRotation = swordStartRot;
        Debug.Log("Sword returned to position " + startPos + " with original rotation");
    }

    private IEnumerator MoveSwordOut()
    {
        Debug.Log("Moving sword out");
        float timer = 0f;

        while (timer < exitDuration)
        {
            float t = timer / exitDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing

            sword.localPosition = Vector3.Lerp(swordStartPos, swordOffscreenPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure sword is exactly at offscreen position
        sword.localPosition = swordOffscreenPos;
        Debug.Log("Sword moved to offscreen position: " + sword.localPosition);
    }
}
