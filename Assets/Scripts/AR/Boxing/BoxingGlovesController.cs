using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BoxingGlovesController : MonoBehaviour
{
    [Header("Glove References")]
    public Transform leftGlove;
    public Transform rightGlove;

    [Header("Animation Settings")]
    public float enterDuration = 0.5f;
    public float punchDuration = 0.3f;
    public float punchDistance = 0.5f;
    public float returnDelay = 0.2f;
    public float exitDuration = 0.5f;

    [Header("Punch Motion Settings")]
    public float punchRotationAngle = 30f; // Rotation angle in degrees
    public Vector3 leftGloveRotationAxis = new Vector3(1, 0, 0); // X-axis rotation for left glove
    public Vector3 rightGloveRotationAxis = new Vector3(1, 0, 0); // X-axis rotation for right glove

    [Header("Effect Settings")]
    public GameObject punchEffect;
    public float effectDuration = 1.0f; // Duration before the effect is destroyed
    public AudioClip normalPunchSound;
    public AudioClip enemyHitSound;

    private GameState gameState;

    private Vector3 leftGloveStartPos;
    private Vector3 rightGloveStartPos;
    private Vector3 leftGloveOffscreenPos;
    private Vector3 rightGloveOffscreenPos;
    private Quaternion leftGloveStartRot;
    private Quaternion rightGloveStartRot;
    private AudioSource audioSource;
    private bool isAnimating = false;

    private void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(OnGameAction);

        // Ensure glove renderers are enabled
        if (leftGlove != null)
        {
            Renderer leftRenderer = leftGlove.GetComponent<Renderer>();
            if (leftRenderer) leftRenderer.enabled = true;
        }

        if (rightGlove != null)
        {
            Renderer rightRenderer = rightGlove.GetComponent<Renderer>();
            if (rightRenderer) rightRenderer.enabled = true;
        }

        // Store initial positions and rotations
        leftGloveStartPos = leftGlove.localPosition;
        rightGloveStartPos = rightGlove.localPosition;
        leftGloveStartRot = leftGlove.localRotation;
        rightGloveStartRot = rightGlove.localRotation;

        // Set offscreen positions (below camera)
        leftGloveOffscreenPos = new Vector3(leftGloveStartPos.x, -60f, leftGloveStartPos.z);
        rightGloveOffscreenPos = new Vector3(rightGloveStartPos.x, -60f, rightGloveStartPos.z);

        // Position gloves offscreen initially
        leftGlove.localPosition = leftGloveOffscreenPos;
        rightGlove.localPosition = rightGloveOffscreenPos;

        // Get or add AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Debug log to verify initialization
        Debug.Log("Boxing Gloves Controller initialized. Left glove at: " + leftGlove.localPosition + ", Right glove at: " + rightGlove.localPosition);
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
        // Check if the action type is "boxing"
        if (actionType == "boxing" && !isAnimating)
        {
            Debug.Log("Boxing action received, playing sequence");
            PlayBoxingSequence();
        }
    }

    public void PlayBoxingSequence()
    {
        // Prevent multiple animations from running
        if (isAnimating)
        {
            Debug.Log("Already animating, ignoring request");
            return;
        }

        StartCoroutine(BoxingSequenceCoroutine());
    }

    private IEnumerator BoxingSequenceCoroutine()
    {
        isAnimating = true;
        Debug.Log("Starting boxing sequence");

        // Move gloves into view
        yield return StartCoroutine(MoveGlovesIn());

        // Left punch
        yield return StartCoroutine(PunchGlove(leftGlove, leftGloveStartRot, leftGloveRotationAxis, true));

        // Right punch
        yield return StartCoroutine(PunchGlove(rightGlove, rightGloveStartRot, rightGloveRotationAxis, false));

        // Left punch again
        yield return StartCoroutine(PunchGlove(leftGlove, leftGloveStartRot, leftGloveRotationAxis, true));

        // Move gloves out of view
        yield return StartCoroutine(MoveGlovesOut());

        isAnimating = false;
        Debug.Log("Boxing sequence completed");
    }

    private IEnumerator MoveGlovesIn()
    {
        Debug.Log("Moving gloves in from: L=" + leftGlove.localPosition + ", R=" + rightGlove.localPosition);
        float timer = 0f;

        // Reset rotations to starting orientation
        leftGlove.localRotation = leftGloveStartRot;
        rightGlove.localRotation = rightGloveStartRot;

        while (timer < enterDuration)
        {
            float t = timer / enterDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing

            leftGlove.localPosition = Vector3.Lerp(leftGloveOffscreenPos, leftGloveStartPos, t);
            rightGlove.localPosition = Vector3.Lerp(rightGloveOffscreenPos, rightGloveStartPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure gloves are exactly at start positions
        leftGlove.localPosition = leftGloveStartPos;
        rightGlove.localPosition = rightGloveStartPos;
        Debug.Log("Gloves moved to start positions: L=" + leftGlove.localPosition + ", R=" + rightGlove.localPosition);
    }

    private IEnumerator PunchGlove(Transform glove, Quaternion startRotation, Vector3 rotationAxis, bool isLeft)
    {
        string gloveSide = isLeft ? "Left" : "Right";
        Vector3 startPos = glove.localPosition;
        Vector3 punchDirection = Vector3.forward;
        Vector3 punchPos = startPos + punchDirection * punchDistance;

        // Calculate punch rotation
        Quaternion punchRotation = Quaternion.AngleAxis(punchRotationAngle, rotationAxis) * startRotation;

        Debug.Log(gloveSide + " glove punching from " + startPos + " to " + punchPos + " with rotation");

        // Punch forward with rotation
        float timer = 0f;
        while (timer < punchDuration)
        {
            float t = timer / punchDuration;
            t = Mathf.SmoothStep(0, 1, t); // Quick acceleration

            // Position interpolation
            glove.localPosition = Vector3.Lerp(startPos, punchPos, t);

            // Rotation interpolation
            glove.localRotation = Quaternion.Slerp(startRotation, punchRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure glove is at full punch position and rotation
        glove.localPosition = punchPos;
        glove.localRotation = punchRotation;

        // Play sound effect
        if (gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            // Play enemy hit sound
            audioSource.PlayOneShot(enemyHitSound);

            // Instantiate hit effect at enemy position with auto-destruction
            GameObject effect = Instantiate(punchEffect, gameState.EnemyCoordinateTransform.position, Quaternion.identity);

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

            Debug.Log("Created hit effect at enemy position, set to destroy in " + effectDuration + " seconds");
        }
        else
        {
            // Play normal punch sound
            audioSource.PlayOneShot(normalPunchSound);
            Debug.Log("No enemy active, playing normal punch sound");
        }

        // Slight delay at full extension
        yield return new WaitForSeconds(returnDelay);

        // Return to starting position and rotation
        timer = 0f;
        while (timer < punchDuration)
        {
            float t = timer / punchDuration;
            t = Mathf.SmoothStep(0, 1, t); // Quick acceleration

            // Position interpolation
            glove.localPosition = Vector3.Lerp(punchPos, startPos, t);

            // Rotation interpolation
            glove.localRotation = Quaternion.Slerp(punchRotation, startRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure glove is exactly at start position and rotation
        glove.localPosition = startPos;
        glove.localRotation = startRotation;
        Debug.Log(gloveSide + " glove returned to position " + startPos + " with original rotation");
    }

    private IEnumerator MoveGlovesOut()
    {
        Debug.Log("Moving gloves out");
        float timer = 0f;

        while (timer < exitDuration)
        {
            float t = timer / exitDuration;
            t = Mathf.SmoothStep(0, 1, t); // Smooth easing

            leftGlove.localPosition = Vector3.Lerp(leftGloveStartPos, leftGloveOffscreenPos, t);
            rightGlove.localPosition = Vector3.Lerp(rightGloveStartPos, rightGloveOffscreenPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        // Ensure gloves are exactly at offscreen positions
        leftGlove.localPosition = leftGloveOffscreenPos;
        rightGlove.localPosition = rightGloveOffscreenPos;
        Debug.Log("Gloves moved to offscreen positions: L=" + leftGlove.localPosition + ", R=" + rightGlove.localPosition);
    }
}
