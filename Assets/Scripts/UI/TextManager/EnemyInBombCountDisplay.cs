using UnityEngine;
using TMPro;

public class EnemyInBombCountDisplay : MonoBehaviour
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
            textDisplay.text = $"Enemy in Bomb: {gameState.EnemyInBombCount}";
        }
    }
}
