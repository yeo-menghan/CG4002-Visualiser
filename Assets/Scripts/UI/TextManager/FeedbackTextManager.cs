using UnityEngine;
using TMPro;
using System.Collections;

public class FeedbackTextManager : MonoBehaviour
{
    [SerializeField] private float displayDuration = 1f;
    private TMP_Text textComponent;
    private GameState gameState;
    private Coroutine currentTextCoroutine;
    private Color greenColor = Color.green;
    private Color redColor = Color.red;

    void Awake()
    {
        Debug.Log($"FeedbackTextManager: Awake called on {gameObject.name}");
        textComponent = GetComponent<TMP_Text>();

        if (textComponent == null)
        {
            Debug.LogError("FeedbackTextManager: No TMP_Text component found!");
            return;
        }

        // Hide the text initially, but keep GameObject active
        textComponent.enabled = false;
        Debug.Log("FeedbackTextManager: Text component hidden initially");
    }

    void Start()
    {
        // Get reference to GameState singleton
        gameState = GameState.Instance;
        if (gameState == null)
        {
            Debug.LogError("FeedbackTextManager: GameState.Instance not found!");
            return;
        }

        // Subscribe to events
        // gameState.EnemyHitEvent += OnEnemyHit;
        // gameState.EnemyShieldHitEvent += OnEnemyShieldHit;
        gameState.gameActionOccurred.AddListener(OnGameAction);

        Debug.Log("FeedbackTextManager: Subscribed to GameState events");
    }

    void OnDestroy()
    {
        // Unsubscribe from events if GameState still exists
        if (gameState != null)
        {
            // gameState.EnemyHitEvent -= OnEnemyHit;
            // gameState.EnemyShieldHitEvent -= OnEnemyShieldHit;
            gameState.gameActionOccurred.RemoveListener(OnGameAction);
        }
    }

    // private bool enemyWasHit = false;
    private string currentAction = "";

    // private void OnEnemyHit()
    // {
    //     enemyWasHit = true;

    //     // If we already have an action, process the feedback now
    //     if (!string.IsNullOrEmpty(currentAction))
    //     {
    //         ProcessActionFeedback(currentAction);
    //     }
    // }

    // private void OnEnemyShieldHit()
    // {
    //     enemyShieldWasHit = true;

    //     // If we already have an action, process the feedback now
    //     if (!string.IsNullOrEmpty(currentAction))
    //     {
    //         ProcessActionFeedback(currentAction);
    //     }
    // }

    private void OnGameAction(string actionType)
    {
        // Store the current action
        currentAction = actionType;

        // Process immediately or wait for hit events
        if (actionType == "Shield")
        {
            // Shield doesn't depend on hit events
            DisplayText("Shield Up", greenColor);
        }
        else if (actionType == "Shoot" || actionType == "Bomb" || actionType == "Badminton" ||
                 actionType == "Golf" || actionType == "Boxing" || actionType == "Fencing")
        {
            // For attack actions, we need to check if a hit occurred
            // If hit already happened, process now, otherwise wait for hit event
            ProcessActionFeedback(actionType);
        }
    }

    private void ProcessActionFeedback(string actionType)
    {
        string result;
        Color resultColor;
        Debug.Log($"FeedbackTextManager: Processing Action Feedback {actionType}");

        if (gameState.EnemyHit == true)
        {
            result = $"{actionType} - Hit";
            resultColor = greenColor;
        }
        else
        {
            result = $"{actionType} - Miss";
            resultColor = redColor;
        }

        DisplayText(result, resultColor);
        currentAction = "";
    }

    private void DisplayText(string message, Color color)
    {
        // Stop any existing text display
        if (currentTextCoroutine != null)
        {
            StopCoroutine(currentTextCoroutine);
        }

        // Start new text display
        currentTextCoroutine = StartCoroutine(ShowTextForDuration(message, color, displayDuration));
    }

    private IEnumerator ShowTextForDuration(string message, Color color, float duration)
    {
        // Set text and color
        textComponent.text = message;
        textComponent.color = color;

        // Show text
        textComponent.enabled = true;

        // Wait for duration
        yield return new WaitForSeconds(duration);

        // Hide text
        textComponent.enabled = false;
        currentTextCoroutine = null;
        OnHitAnimationComplete();
    }

    public void OnHitAnimationComplete()
    {
        gameState.PlayerHit = false;
        gameState.EnemyHit = false;
    }
}
