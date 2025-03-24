using UnityEngine;
using TMPro;

public class EnemyShieldCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text shieldText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the bullet text based on the current bullet count
        UpdateShieldCountText();
    }

    private void Update()
    {
        // Continuously check if the bullet count has changed, and update the text accordingly
        UpdateShieldCountText();
    }

    private void UpdateShieldCountText()
    {
        int enemyShieldCount = gameState.EnemyShieldCount;

        shieldText.text = enemyShieldCount.ToString();
    }
}
