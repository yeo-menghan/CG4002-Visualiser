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
    public float punchRotationAngle = 30f;
    public Vector3 leftGloveRotationAxis = new Vector3(1, 0, 0);
    public Vector3 rightGloveRotationAxis = new Vector3(1, 0, 0);

    [Header("Effect Settings")]
    public GameObject punchEffect;
    public float effectDuration = 1.0f;
    public AudioClip normalPunchSound;
    public AudioClip enemyHitSound;

    [Header("Action Wait")]
    public ActionWaitBar actionWaitBar;

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

        leftGloveStartPos = leftGlove.localPosition;
        rightGloveStartPos = rightGlove.localPosition;
        leftGloveStartRot = leftGlove.localRotation;
        rightGloveStartRot = rightGlove.localRotation;

        leftGloveOffscreenPos = new Vector3(leftGloveStartPos.x, -60f, leftGloveStartPos.z);
        rightGloveOffscreenPos = new Vector3(rightGloveStartPos.x, -60f, rightGloveStartPos.z);

        leftGlove.localPosition = leftGloveOffscreenPos;
        rightGlove.localPosition = rightGloveOffscreenPos;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log("Boxing Gloves Controller initialized. Left glove at: " + leftGlove.localPosition + ", Right glove at: " + rightGlove.localPosition);
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    private void OnGameAction(string actionType)
    {
        if (actionType == "boxing" && !isAnimating)
        {
            Debug.Log("Boxing action received, playing sequence");
            PlayBoxingSequence();
        }
    }

    public void PlayBoxingSequence()
    {
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

        yield return StartCoroutine(MoveGlovesIn());
        yield return StartCoroutine(PunchGlove(leftGlove, leftGloveStartRot, leftGloveRotationAxis, true));
        yield return StartCoroutine(PunchGlove(rightGlove, rightGloveStartRot, rightGloveRotationAxis, false));
        yield return StartCoroutine(PunchGlove(leftGlove, leftGloveStartRot, leftGloveRotationAxis, true));
        yield return StartCoroutine(MoveGlovesOut());

        actionWaitBar.StartWait();

        isAnimating = false;
        Debug.Log("Boxing sequence completed");
    }

    private IEnumerator MoveGlovesIn()
    {
        Debug.Log("Moving gloves in from: L=" + leftGlove.localPosition + ", R=" + rightGlove.localPosition);
        float timer = 0f;

        leftGlove.localRotation = leftGloveStartRot;
        rightGlove.localRotation = rightGloveStartRot;

        while (timer < enterDuration)
        {
            float t = timer / enterDuration;
            t = Mathf.SmoothStep(0, 1, t);

            leftGlove.localPosition = Vector3.Lerp(leftGloveOffscreenPos, leftGloveStartPos, t);
            rightGlove.localPosition = Vector3.Lerp(rightGloveOffscreenPos, rightGloveStartPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

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

        Quaternion punchRotation = Quaternion.AngleAxis(punchRotationAngle, rotationAxis) * startRotation;
        Debug.Log(gloveSide + " glove punching from " + startPos + " to " + punchPos + " with rotation");

        float timer = 0f;
        while (timer < punchDuration)
        {
            float t = timer / punchDuration;
            t = Mathf.SmoothStep(0, 1, t);

            glove.localPosition = Vector3.Lerp(startPos, punchPos, t);
            glove.localRotation = Quaternion.Slerp(startRotation, punchRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        glove.localPosition = punchPos;
        glove.localRotation = punchRotation;

        if (gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            audioSource.PlayOneShot(enemyHitSound);
            GameObject effect = Instantiate(punchEffect, gameState.EnemyCoordinateTransform.position, Quaternion.identity);
            ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                var main = particleSystem.main;
                main.loop = false;
            }

            Destroy(effect, effectDuration);

            Debug.Log("Created hit effect at enemy position, set to destroy in " + effectDuration + " seconds");
        }
        else
        {
            audioSource.PlayOneShot(normalPunchSound);
            Debug.Log("No enemy active, playing normal punch sound");
        }

        yield return new WaitForSeconds(returnDelay);

        timer = 0f;
        while (timer < punchDuration)
        {
            float t = timer / punchDuration;
            t = Mathf.SmoothStep(0, 1, t);

            glove.localPosition = Vector3.Lerp(punchPos, startPos, t);
            glove.localRotation = Quaternion.Slerp(punchRotation, startRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

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
            t = Mathf.SmoothStep(0, 1, t);

            leftGlove.localPosition = Vector3.Lerp(leftGloveStartPos, leftGloveOffscreenPos, t);
            rightGlove.localPosition = Vector3.Lerp(rightGloveStartPos, rightGloveOffscreenPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        leftGlove.localPosition = leftGloveOffscreenPos;
        rightGlove.localPosition = rightGloveOffscreenPos;
        Debug.Log("Gloves moved to offscreen positions: L=" + leftGlove.localPosition + ", R=" + rightGlove.localPosition);
    }
}
