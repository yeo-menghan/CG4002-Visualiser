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
    public float bulletTravelTime = 0.3f;

    [Header("Recoil Settings")]
    public float recoilAmount = 0.25f;
    public float backwardRecoilAmount = 0.1f;
    public float recoilRecoverySpeed = 5f;
    public AnimationCurve recoilCurve;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isRecoiling = false;
    private bool isSubscribed = false;
    [Header("Action Wait")]
    public ActionWaitBar actionWaitBar;

    private void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;

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

    private void OnGameActionOccurred(string actionType)
    {
        if (actionType == "gun" && gameState.PlayerBulletCount > 0)
        {
            FireGun();
            Debug.Log("Firing gun from action: " + actionType);
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

        if (gameState != null && gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            Debug.Log("Target position: " + gameState.EnemyCoordinateTransform.position);
            Debug.Log("Muzzle position: " + muzzlePoint.position);

            Debug.DrawLine(muzzlePoint.position, gameState.EnemyCoordinateTransform.position, Color.red, 2.0f);

            StartCoroutine(PlayImpactEffectDelayed(bulletTravelTime));
        }
        else
        {
            Debug.Log("No enemy target for impact effect");
        }

        if (!isRecoiling)
            StartCoroutine(RecoilAnimation());
    }

    private IEnumerator PlayImpactEffectDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (gameState != null && gameState.EnemyActive && gameState.EnemyCoordinateTransform != null)
        {
            CreateImpactEffect(gameState.EnemyCoordinateTransform.position, gameState.EnemyCoordinateTransform);
            Debug.Log("Impact effect created at enemy position");
        }
    }

    private void CreateImpactEffect(Vector3 position, Transform hitTransform)
    {
        if (impactEffectPrefab != null)
        {
            GameObject impact = Instantiate(impactEffectPrefab, position, Quaternion.identity, hitTransform);

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
                0,
                backwardRecoilAmount * curveValue
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
        actionWaitBar.StartWait();
    }
}
