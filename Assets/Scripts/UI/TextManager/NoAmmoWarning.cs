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

    private TextMeshProUGUI statusText;
    private AudioSource audioSource;
    private GameState gameState;
    private Coroutine activeCoroutine;

    private void Awake()
    {
        statusText = GetComponent<TextMeshProUGUI>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("ResourceStatusDisplay: GameState instance not found!");
            return;
        }

        gameState.gameActionOccurred.AddListener(OnGameActionOccurred);

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
            statusText.text = $"Out of {resourceType}";

            if (outOfResourcesSound != null)
            {
                audioSource.PlayOneShot(outOfResourcesSound);
            }

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            activeCoroutine = StartCoroutine(FadeInOutText());

            Debug.Log($"ResourceStatusDisplay: Player is out of {resourceType}!");
        }
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
        Color textColor = statusText.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            textColor.a = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
            statusText.color = textColor;

            yield return null;
        }

        textColor.a = endAlpha;
        statusText.color = textColor;
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameActionOccurred);
        }
    }
}
