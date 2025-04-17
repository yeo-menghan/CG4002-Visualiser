using UnityEngine;
using TMPro;

public class PlayerInBombCountDisplay : MonoBehaviour
{
    private TextMeshProUGUI textDisplay;
    private GameState gameState;

    void Awake()
    {
        // Get the TextMeshProUGUI component on this GameObject
        textDisplay = GetComponent<TextMeshProUGUI>();

        // Find the GameState instance in the scene
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("GameState instance not found!");
        }

        if (textDisplay == null)
        {
            Debug.LogError("TextMeshProUGUI component not found on this GameObject!");
        }
    }

    void Start()
    {
        // Initialize the text
        UpdateText();
    }

    void Update()
    {
        // Update the text every frame to reflect current value
        UpdateText();
    }

    void UpdateText()
    {
        if (gameState != null && textDisplay != null)
        {
            // Display the enemy in bomb count
            textDisplay.text = $"Player in Bomb: {gameState.PlayerInBombCount}";
        }
    }
}
