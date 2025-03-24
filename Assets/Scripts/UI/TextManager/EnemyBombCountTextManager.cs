using UnityEngine;
using TMPro;

public class EnemyBombCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text bombText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the bullet text based on the current bullet count
        UpdateBombCountText();
    }

    private void Update()
    {
        // Continuously check if the bullet count has changed, and update the text accordingly
        UpdateBombCountText();
    }

    private void UpdateBombCountText()
    {
        int enemyBombCount = gameState.EnemyBombCount;

        bombText.text = enemyBombCount.ToString();
    }
}
