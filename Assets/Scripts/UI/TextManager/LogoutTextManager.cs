using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI), typeof(AudioSource))]
public class LogoutTextManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip logoutSound;

    // Private references
    private TextMeshProUGUI logoutText;
    private AudioSource audioSource;
    private GameState gameState;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        // Get components
        logoutText = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Get GameState reference
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("LogoutTextManager: GameState instance not found!");
            return;
        }

        // Subscribe to game action events
        gameState.gameActionOccurred.AddListener(OnGameActionOccurred);

        // Initially hide the text (set alpha to 0)
        Color textColor = logoutText.color;
        textColor.a = 0;
        logoutText.color = textColor;
    }

    private void OnGameActionOccurred(string actionType)
    {
        if (actionType.ToLower() == "logout")
        {
            DisplayLogoutMessage();
        }
    }

    private void DisplayLogoutMessage()
    {
        // Update the message text
        logoutText.text = "Logging out...";

        // Play sound
        if (logoutSound != null)
        {
            audioSource.PlayOneShot(logoutSound);
        }

        // Cancel any active coroutine
        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        // Start the new fade in/out sequence
        activeCoroutine = StartCoroutine(FadeInOutText());

        Debug.Log("LogoutTextManager: Player is logging out!");
    }

    private IEnumerator FadeInOutText()
    {
        // Fade in
        yield return FadeText(0f, 1f, fadeInDuration);

        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out
        yield return FadeText(1f, 0f, fadeOutDuration);

        activeCoroutine = null;
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha, float duration)
    {
        float elapsedTime = 0;
        Color textColor = logoutText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Update alpha using smooth interpolation
            textColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            logoutText.color = textColor;

            yield return null;
        }

        // Ensure we end at the exact target alpha
        textColor.a = endAlpha;
        logoutText.color = textColor;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameActionOccurred);
        }
    }
}
