using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EnemyFencingController : MonoBehaviour
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
    public float jabRotationAngle = 15f;
    public Vector3 swordRotationAxis = new Vector3(1, 0, 0);

    [Header("Effect Settings")]
    public GameObject slashEffect;
    public float effectDuration = 1.0f;
    public AudioClip normalSlashSound;
    public AudioClip enemyHitSound;
    public float impactDistanceFromCamera = 1.5f;

    [Header("AR Camera")]
    public Camera arCamera;

    [Header("Damage Effect")]
    public TakeDamageScript damageEffectController;

    private GameState gameState;

    private Vector3 swordStartPos;
    private Vector3 swordOffscreenPos;
    private Quaternion swordStartRot;
    private AudioSource audioSource;
    private bool isAnimating = false;

    private void Start()
    {
        gameState = GameState.Instance;
        gameState.enemyGameActionOccurred.AddListener(OnGameAction);

        if (sword != null)
        {
            Renderer swordRenderer = sword.GetComponent<Renderer>();
            if (swordRenderer) swordRenderer.enabled = true;
        }

        swordStartPos = sword.localPosition;
        swordStartRot = sword.localRotation;
        sword.gameObject.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        Debug.Log("Sword Controller initialized. Sword at: " + sword.localPosition);
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.enemyGameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    private void OnGameAction(string actionType)
    {
        if (actionType == "fencing" && gameState.EnemyActive && !isAnimating)
        {
            Debug.Log("Fencing action received, playing sequence");
            PlaySwordSequence();
        }
    }

    public void PlaySwordSequence()
    {
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
        sword.gameObject.SetActive(true);
        Debug.Log("Sword activated");

        yield return StartCoroutine(JabSword(firstJabDuration, firstJabDistance));
        if(gameState.EnemyActive)
        {
            damageEffectController.StartDamageEffect();
        }
        yield return StartCoroutine(JabSword(secondJabDuration, secondJabDistance));
        if(gameState.EnemyActive)
        {
            damageEffectController.StartDamageEffect();
        }

        sword.gameObject.SetActive(false);
        Debug.Log("Sword deactivated");

        isAnimating = false;
        Debug.Log("Sword sequence completed");
    }

    private IEnumerator MoveSwordIn()
    {
        Debug.Log("Moving sword in from: " + sword.localPosition);
        float timer = 0f;

        sword.localRotation = swordStartRot;

        while (timer < enterDuration)
        {
            float t = timer / enterDuration;
            t = Mathf.SmoothStep(0, 1, t);

            sword.localPosition = Vector3.Lerp(swordOffscreenPos, swordStartPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        sword.localPosition = swordStartPos;
        Debug.Log("Sword moved to start position: " + sword.localPosition);
    }

    private IEnumerator JabSword(float jabDuration, float jabDistance)
    {
        Vector3 startPos = sword.localPosition;
        Vector3 jabDirection = Vector3.up;
        Vector3 jabPos = startPos + jabDirection * jabDistance;

        Quaternion jabRotation = Quaternion.AngleAxis(-jabRotationAngle, swordRotationAxis) * swordStartRot;

        Debug.Log("Sword jabbing from " + startPos + " to " + jabPos + " with rotation");

        float timer = 0f;
        while (timer < jabDuration)
        {
            float t = timer / jabDuration;
            t = Mathf.SmoothStep(0, 1, t);

            sword.localPosition = Vector3.Lerp(startPos, jabPos, t);

            sword.localRotation = Quaternion.Slerp(swordStartRot, jabRotation, t);

            timer += Time.deltaTime;
            yield return null;
        }

        sword.localPosition = jabPos;
        sword.localRotation = jabRotation;

        if (gameState.EnemyActive)
        {
            audioSource.PlayOneShot(enemyHitSound);
        }
        else
        {
            audioSource.PlayOneShot(normalSlashSound);
            Debug.Log("No enemy active, playing normal slash sound");
        }

        yield return new WaitForSeconds(returnDelay);

        timer = 0f;
        while (timer < jabDuration)
        {
            float t = timer / jabDuration;
            t = Mathf.SmoothStep(0, 1, t);

            sword.localPosition = Vector3.Lerp(jabPos, startPos, t);
            sword.localRotation = Quaternion.Slerp(jabRotation, swordStartRot, t);

            timer += Time.deltaTime;
            yield return null;
        }

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
            t = Mathf.SmoothStep(0, 1, t);

            sword.localPosition = Vector3.Lerp(swordStartPos, swordOffscreenPos, t);

            timer += Time.deltaTime;
            yield return null;
        }

        sword.localPosition = swordOffscreenPos;
        Debug.Log("Sword moved to offscreen position: " + sword.localPosition);
    }
}
