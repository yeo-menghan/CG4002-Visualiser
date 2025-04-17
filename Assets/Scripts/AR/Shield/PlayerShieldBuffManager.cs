using UnityEngine;
using System.Collections;


public class PlayerShieldBuffManager : MonoBehaviour
{
    // Reference to GameState
    private GameState gameState;

    public GameObject playerShieldBuff;
    public GameObject temporaryShieldPrefab;

    // Audio clips for shield activation/deactivation
    public AudioClip shieldDeactivationSound;
    public AudioClip temporaryShieldSound;

    // Audio source component
    private AudioSource audioSource;

    // Internal state to track active status
    private bool isShieldActive = false;
    private bool isTemporaryShieldActive = false;

    void Start()
    {
        // Get reference to GameState
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(OnGameAction);

        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (playerShieldBuff != null)
        {
            playerShieldBuff.SetActive(false); // Ensure it's initially disabled
        }
        else
        {
            Debug.LogError("PlayerShieldBuff is not assigned!");
        }
        if (temporaryShieldPrefab != null)
        {
            temporaryShieldPrefab.SetActive(false); // Ensure it's initially disabled
        }
        else
        {
            Debug.LogError("TemporaryShieldPrefab is not assigned!");
        }

        // Audio clip validation
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
        // Unsubscribe from events when destroyed
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    void Update()
    {
        // Check if the shield should be active
        bool shouldShieldBeActive = gameState.PlayerCurrentShield > 0;

        // If the shield state changes, handle activation or deactivation
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
        // Check if the action is a shield action
        if (actionType == "shield" && !isTemporaryShieldActive)
        {
            StartCoroutine(ShowTemporaryShieldEffect());
        }
    }

    private IEnumerator ShowTemporaryShieldEffect()
    {
        isTemporaryShieldActive = true;

        // Play temporary shield sound
        if (audioSource != null && temporaryShieldSound != null)
        {
            audioSource.PlayOneShot(temporaryShieldSound);
        }

        // Activate temporary shield prefab
        if (temporaryShieldPrefab != null)
        {
            temporaryShieldPrefab.SetActive(true);
        }

        // Wait for the specified duration
        yield return new WaitForSeconds(2.0f);

        // Deactivate temporary shield prefab
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

        // Play deactivation sound
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
