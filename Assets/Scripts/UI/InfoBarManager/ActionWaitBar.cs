using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ActionWaitBar : MonoBehaviour
{
    private Slider actionSlider;
    private CanvasGroup canvasGroup;
    private GameState gameState;
    private CanvasGroup crosshairCanvasGroup;
    private bool isWaiting = false;
    public float waitDuration = 2f;

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

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.volume = soundVolume;
            Debug.Log("ActionWaitBar: AudioSource component added.");
        }

        GameObject crosshair = GameObject.Find("Crosshair");
        if (crosshair != null)
        {
            crosshairCanvasGroup = crosshair.GetComponent<CanvasGroup>();
            if (crosshairCanvasGroup == null)
            {
                crosshairCanvasGroup = crosshair.AddComponent<CanvasGroup>();
                Debug.Log("ActionWaitBar: CanvasGroup added to crosshair.");
            }
        }
        else
        {
            Debug.LogError("ActionWaitBar: Crosshair GameObject not found!");
        }

        SetVisibility(false);
    }

    void Start()
    {
        gameState = GameState.Instance;
    }

    public void StartWait()
    {
        if (isWaiting)
        {
            Debug.LogWarning("ActionWaitBar: Already waiting!");
            return;
        }
        Debug.Log("ActionWaitBar: Starting Wait");

        StopAllCoroutines();
        StartCoroutine(WaitAnimation());
    }

    private IEnumerator WaitAnimation()
    {
        isWaiting = true;
        SetVisibility(true);
        actionSlider.value = 0;
        SetCrosshairVisibility(false);
        Debug.Log("ActionWaitBar: Crosshair set to active");

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
            actionSlider.value = Mathf.Clamp01(elapsedTime / waitDuration);
            yield return null;
        }

        actionSlider.value = 1;

        SetVisibility(false);
        SetCrosshairVisibility(true);
        Debug.Log("ActionWaitBar: Crosshair set to inactive by WaitAnimation");
        isWaiting = false;
    }

    private void SetVisibility(bool visible)
    {
        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        Debug.Log($"ActionWaitBar: Visibility set to {visible}");
    }

    private void SetCrosshairVisibility(bool visible)
    {
        if (crosshairCanvasGroup != null)
        {
            crosshairCanvasGroup.alpha = visible ? 1f : 0f;
            crosshairCanvasGroup.interactable = visible;
            crosshairCanvasGroup.blocksRaycasts = visible;
            Debug.Log($"ActionWaitBar: Crosshair visibility set to {visible}");
        }
        else
        {
            Debug.LogError("ActionWaitBar: Crosshair CanvasGroup is null!");
        }
    }
}
