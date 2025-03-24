using UnityEngine;

public class PlayerShieldManager : MonoBehaviour
{
    // Reference to GameState
    private GameState gameState;

    // Reference to the PlayerShieldOverlay GameObject
    public GameObject playerShieldOverlay;

    // Shield animation duration
    public float fadeDuration = 2f;

    // Audio clips for shield activation/deactivation
    public AudioClip shieldActivationSound;
    public AudioClip shieldDeactivationSound;

    // Audio source component
    private AudioSource audioSource;

    // Internal state to track active status
    private bool isShieldActive = false;

    void Start()
    {
        // Get reference to GameState
        gameState = GameState.Instance;

        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure the PlayerShieldOverlay has a CanvasGroup for fading
        if (playerShieldOverlay != null)
        {
            CanvasGroup overlayCanvasGroup = playerShieldOverlay.GetComponent<CanvasGroup>();
            if (overlayCanvasGroup == null)
            {
                overlayCanvasGroup = playerShieldOverlay.AddComponent<CanvasGroup>();
            }
            overlayCanvasGroup.alpha = 0; // Start fully transparent
            playerShieldOverlay.SetActive(false); // Ensure it's initially disabled
        }
        else
        {
            Debug.LogError("PlayerShieldOverlay is not assigned!");
        }

        // Audio clip validation
        if (shieldActivationSound == null)
        {
            Debug.LogWarning("Shield activation sound is not assigned!");
        }
        if (shieldDeactivationSound == null)
        {
            Debug.LogWarning("Shield deactivation sound is not assigned!");
        }
    }

    void Update()
    {
        // Check if the shield should be active
        bool shouldShieldBeActive = gameState.PlayerCurrentShield > 0;

        // If the shield state changes, handle activation or deactivation
        if (shouldShieldBeActive && !isShieldActive)
        {
            ActivateShieldOverlay();
        }
        else if (!shouldShieldBeActive && isShieldActive)
        {
            DeactivateShieldOverlay();
        }
    }

    private void ActivateShieldOverlay()
    {
        isShieldActive = true;

        // Play activation sound
        if (audioSource != null && shieldActivationSound != null)
        {
            audioSource.clip = shieldActivationSound;
            audioSource.Play();
        }

        if (playerShieldOverlay != null)
        {
            playerShieldOverlay.SetActive(true);
            StartCoroutine(FadeCanvasGroup(playerShieldOverlay.GetComponent<CanvasGroup>(), 0, 1, fadeDuration)); // Fade from black
        }
    }

    private void DeactivateShieldOverlay()
    {
        isShieldActive = false;

        // Play deactivation sound
        if (audioSource != null && shieldDeactivationSound != null)
        {
            audioSource.clip = shieldDeactivationSound;
            audioSource.Play();
        }

        if (playerShieldOverlay != null)
        {
            StartCoroutine(FadeCanvasGroup(playerShieldOverlay.GetComponent<CanvasGroup>(), 1, 0, fadeDuration, () =>
            {
                playerShieldOverlay.SetActive(false); // Disable after fade-out
            }));
        }
    }

    // Coroutine to fade a CanvasGroup's alpha value
    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        onComplete?.Invoke();
    }
}
