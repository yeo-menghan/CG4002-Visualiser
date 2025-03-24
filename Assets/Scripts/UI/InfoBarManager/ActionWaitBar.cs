using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionWaitBar : MonoBehaviour
{
    private Slider actionSlider;
    private CanvasGroup canvasGroup;
    private GameState gameState;
    private bool isWaiting = false;
    public float waitDuration = 1f;

    // Audio components
    public AudioSource audioSource;
    public AudioClip actionWaitSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.6f;

    void Awake()
    {
        actionSlider = GetComponent<Slider>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (actionSlider == null)
        {
            Debug.LogError("ActionWaitBar: Slider component not found!");
        }

        if (canvasGroup == null)
        {
            Debug.LogError("ActionWaitBar: CanvasGroup component not found!");
        }

        // Initialize AudioSource if needed
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
            Debug.Log("ActionWaitBar: AudioSource component added.");
        }

        SetVisibility(false);
    }

    void Start()
    {
        gameState = GameState.Instance;
    }

    // Method to start the reload animation
    public void StartWait()
    {
        if (isWaiting)
        {
            Debug.LogWarning("ActionWaitBar: Already waiting!");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(WaitAnimation());
    }

    private IEnumerator WaitAnimation()
    {
        isWaiting = true;
        SetVisibility(true);
        actionSlider.value = 0; // Reset the slider value

        if (actionWaitSound != null && audioSource != null)
        {
            audioSource.clip = actionWaitSound;
            audioSource.Play();
            Debug.Log("ActionWaitBar: Playing reload start sound");
        }

        float elapsedTime = 0f;

        while (elapsedTime < waitDuration)
        {
            elapsedTime += Time.deltaTime;
            actionSlider.value = Mathf.Clamp01(elapsedTime / waitDuration); // Update slider value
            yield return null; // Wait for the next frame
        }

        actionSlider.value = 1; // Ensure the slider is full

        SetVisibility(false);
        isWaiting = false;
    }

    // Simple helper method to control visibility using the CanvasGroup
    private void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        Debug.Log($"ActionWaitBar: Visibility set to {visible}");
    }
}
