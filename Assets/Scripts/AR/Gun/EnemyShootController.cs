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
    public float bulletTravelTime = 0.3f;
    public float impactDistanceFromCamera = 1.5f;

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
        gameState = GameState.Instance;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (muzzleFlash != null)
        {
            muzzleFlash.Stop();
            Debug.Log("Muzzle flash reference found: " + muzzleFlash.name);
        }
        else
        {
            Debug.LogError("Muzzle flash reference is null!");
        }

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

    private void OnGameActionOccurred(string actionType)
    {
        if (actionType == "gun" && gameState.EnemyActive && gameState.EnemyBulletCount > 0)
        {
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
        if (audioSource != null && shootSound != null)
            audioSource.PlayOneShot(shootSound);

        if (muzzleFlash != null)
        {
            Debug.Log("Playing muzzle flash");
            muzzleFlash.Play(true);
        }

        if (damageEffectController != null && gameState.EnemyActive && gameState.PlayerHit)
        {
            damageEffectController.StartDamageEffect();
            Debug.Log("Started damage effect");
        }
        else
        {
            Debug.LogWarning("Damage effect controller reference is missing!");
        }

    }

    private IEnumerator PlayImpactEffectDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (arCamera != null)
        {
            Vector3 impactPosition = arCamera.transform.position + arCamera.transform.forward * impactDistanceFromCamera;

            CreateImpactEffect(impactPosition);
            Debug.Log("Impact effect created in front of AR Camera at: " + impactPosition);
        }
    }

    private void CreateImpactEffect(Vector3 position)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity);
            impact.transform.LookAt(2 * position - arCamera.transform.position);
            if (impactSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(impactSound);
            }
            Destroy(impact, impactEffectDuration);
        }
    }

    private IEnumerator RecoilAnimation()
    {
        isRecoiling = true;
        float time = 0;
        float duration = 0.2f;

        while (time < duration)
        {
            float progress = time / duration;
            float curveValue = recoilCurve.Evaluate(progress);

            Vector3 recoilPosition = originalPosition - new Vector3(
                0,
                backwardRecoilAmount * curveValue,
                0
            );

            transform.localPosition = recoilPosition;
            transform.localRotation = originalRotation * Quaternion.Euler(-recoilAmount * 10 * curveValue, 0, 0);

            time += Time.deltaTime;
            yield return null;
        }

        time = 0;
        duration = 0.3f;
        Vector3 recoiledPosition = transform.localPosition;
        Quaternion recoiledRotation = transform.localRotation;

        while (time < duration)
        {
            float progress = time / duration;

            transform.localPosition = Vector3.Lerp(recoiledPosition, originalPosition, progress);
            transform.localRotation = Quaternion.Lerp(recoiledRotation, originalRotation, progress);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        transform.localRotation = originalRotation;
        isRecoiling = false;
    }
}
