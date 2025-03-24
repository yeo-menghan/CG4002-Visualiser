using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TextMeshProUGUI), typeof(AudioSource))]
public class NoAmmoWarning : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float fadeOutDuration = 0.8f;

    [Header("Audio")]
    [SerializeField] private AudioClip outOfResourcesSound;

    // Private references
    private TextMeshProUGUI statusText;
    private AudioSource audioSource;
    private GameState gameState;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        // Get components
        statusText = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Get GameState reference
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("ResourceStatusDisplay: GameState instance not found!");
            return;
        }

        // Subscribe to game action events
        gameState.gameActionOccurred.AddListener(OnGameActionOccurred);

        // Initially hide the text (set alpha to 0)
        Color textColor = statusText.color;
        textColor.a = 0;
        statusText.color = textColor;
    }

    private void OnGameActionOccurred(string actionType)
    {
        switch (actionType.ToLower())
        {
            case "bomb":
                CheckAndDisplayResource("bomb", gameState.PlayerBombCount);
                break;

            case "gun":
                CheckAndDisplayResource("bullets", gameState.PlayerBulletCount);
                break;

            case "shield":
                CheckAndDisplayResource("shield", gameState.PlayerShieldCount);
                break;
        }
    }

    private void CheckAndDisplayResource(string resourceType, int count)
    {
        if (count <= 0)
        {
            // Update the message text
            statusText.text = $"Out of {resourceType}";

            // Play sound
            if (outOfResourcesSound != null)
            {
                audioSource.PlayOneShot(outOfResourcesSound);
            }

            // Cancel any active coroutine
            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            // Start the new fade in/out sequence
            activeCoroutine = StartCoroutine(FadeInOutText());

            Debug.Log($"ResourceStatusDisplay: Player is out of {resourceType}!");
        }
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
        Color textColor = statusText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Update alpha using smooth interpolation
            textColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            statusText.color = textColor;

            yield return null;
        }

        // Ensure we end at the exact target alpha
        textColor.a = endAlpha;
        statusText.color = textColor;
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
