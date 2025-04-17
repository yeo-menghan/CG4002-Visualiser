using UnityEngine;
using System.Collections;


public class PlayerShieldBuffManager : MonoBehaviour
{
    private GameState gameState;

    public GameObject playerShieldBuff;
    public GameObject temporaryShieldPrefab;

    public AudioClip shieldDeactivationSound;
    public AudioClip temporaryShieldSound;

    private AudioSource audioSource;

    private bool isShieldActive = false;
    private bool isTemporaryShieldActive = false;

    void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(OnGameAction);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerShieldBuff != null)
        {
            playerShieldBuff.SetActive(false);
        }
        else
        {
            Debug.LogError("PlayerShieldBuff is not assigned!");
        }
        if (temporaryShieldPrefab != null)
        {
            temporaryShieldPrefab.SetActive(false);
        }
        else
        {
            Debug.LogError("TemporaryShieldPrefab is not assigned!");
        }

        if (shieldDeactivationSound == null)
        {
            Debug.LogWarning("Shield deactivation sound is not assigned!");
        }
        if (temporaryShieldSound == null)
        {
            Debug.LogWarning("Temporary shield sound is not assigned!");
        }
    }

    void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    void Update()
    {
        bool shouldShieldBeActive = gameState.PlayerCurrentShield > 0;

        if (shouldShieldBeActive && !isShieldActive)
        {
            ActivateShieldBuff();
        }
        else if (!shouldShieldBeActive && isShieldActive)
        {
            DeactivateShieldBuff();
        }
    }

    private void OnGameAction(string actionType)
    {
        if (actionType == "shield" && !isTemporaryShieldActive)
        {
            StartCoroutine(ShowTemporaryShieldEffect());
        }
    }

    private IEnumerator ShowTemporaryShieldEffect()
    {
        isTemporaryShieldActive = true;

        if (audioSource != null && temporaryShieldSound != null)
        {
            audioSource.PlayOneShot(temporaryShieldSound);
        }

        if (temporaryShieldPrefab != null)
        {
            temporaryShieldPrefab.SetActive(true);
        }

        yield return new WaitForSeconds(2.0f);

        if (temporaryShieldPrefab != null)
        {
            temporaryShieldPrefab.SetActive(false);
        }

        isTemporaryShieldActive = false;
    }

    private void ActivateShieldBuff()
    {
        isShieldActive = true;

        if (playerShieldBuff != null)
        {
            playerShieldBuff.SetActive(true);
        }
    }

    private void DeactivateShieldBuff()
    {
        isShieldActive = false;

        if (audioSource != null && shieldDeactivationSound != null)
        {
            audioSource.clip = shieldDeactivationSound;
            audioSource.Play();
        }

        if (playerShieldBuff != null)
        {
            playerShieldBuff.SetActive(false);
        }
    }
}
