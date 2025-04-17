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

    private TextMeshProUGUI logoutText;
    private AudioSource audioSource;
    private GameState gameState;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        logoutText = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("LogoutTextManager: GameState instance not found!");
            return;
        }

        gameState.gameActionOccurred.AddListener(OnGameActionOccurred);

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
        logoutText.text = "Logging out...";

        if (logoutSound != null)
        {
            audioSource.PlayOneShot(logoutSound);
        }

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
        }

        activeCoroutine = StartCoroutine(FadeInOutText());

        Debug.Log("LogoutTextManager: Player is logging out!");
    }

    private IEnumerator FadeInOutText()
    {
        yield return FadeText(0f, 1f, fadeInDuration);
        yield return new WaitForSeconds(displayDuration);
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

            textColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            logoutText.color = textColor;

            yield return null;
        }

        textColor.a = endAlpha;
        logoutText.color = textColor;
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameActionOccurred);
        }
    }
}
