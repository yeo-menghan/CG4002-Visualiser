using UnityEngine;
using TMPro;

public class PlayerInBombCountDisplay : MonoBehaviour
{
    private TextMeshProUGUI textDisplay;
    private GameState gameState;

    void Awake()
    {
        textDisplay = GetComponent<TextMeshProUGUI>();

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
        UpdateText();
    }

    void Update()
    {
        UpdateText();
    }

    void UpdateText()
    {
        if (gameState != null && textDisplay != null)
        {
            textDisplay.text = $"Player in Bomb: {gameState.PlayerInBombCount}";
        }
    }
}
