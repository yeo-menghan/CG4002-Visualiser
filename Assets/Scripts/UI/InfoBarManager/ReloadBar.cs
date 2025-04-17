using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ReloadBar : MonoBehaviour
{
    private Slider reloadSlider;
    private CanvasGroup canvasGroup;
    private GameState gameState;
    private bool isReloading = false;
    public float reloadDuration = 2f;

    public AudioSource audioSource;
    public AudioClip reloadStartSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.7f;

    void Awake()
    {
        reloadSlider = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (reloadSlider == null)
        {
            Debug.LogError("ReloadBar: Slider component not found!");
        }

        if (canvasGroup == null)
        {
            Debug.LogError("ReloadBar: CanvasGroup component not found!");
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
            Debug.Log("ReloadBar: AudioSource component added.");
        }

        SetVisibility(false);
    }

    void Start()
    {
        gameState = GameState.Instance;
        Debug.Log("ReloadBar: Attempting to subscribe to PlayerReloadEvent");
        gameState.gameActionOccurred.AddListener(OnGameAction);
        Debug.Log("ReloadBar: Successfully subscribed to PlayerReloadEvent");
    }

    void OnDestroy()
    {
        Debug.Log("ReloadBar: OnDestroy called");

        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
            Debug.Log("ReloadBar: Unsubscribed from PlayerReloadEvent");
        }
    }

    private void OnGameAction(string actionType)
    {
        if (actionType == "reload")
        {
            Debug.Log("ReloadBar: Reload action received!");
            StartReload();
        }
    }

    public void StartReload()
    {
        if (isReloading)
        {
            Debug.LogWarning("ReloadBar: Already reloading!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ReloadAnimation());
    }

    private IEnumerator ReloadAnimation()
    {
        isReloading = true;
        SetVisibility(true);
        reloadSlider.value = 0;

        if (reloadStartSound != null && audioSource != null)
        {
            audioSource.clip = reloadStartSound;
            audioSource.Play();
            Debug.Log("ReloadBar: Playing reload start sound");
        }

        float elapsedTime = 0f;

        while (elapsedTime < reloadDuration)
        {
            elapsedTime += Time.deltaTime;
            reloadSlider.value = Mathf.Clamp01(elapsedTime / reloadDuration);
            yield return null;
        }

        reloadSlider.value = 1;

        SetVisibility(false);
        isReloading = false;
    }

    private void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        Debug.Log($"ReloadBar: Visibility set to {visible}");
    }
}
