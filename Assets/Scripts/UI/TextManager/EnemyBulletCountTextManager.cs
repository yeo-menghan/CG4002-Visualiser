using UnityEngine;
using TMPro;

public class EnemyBulletCountTextManager : MonoBehaviour
{
    // Reference to the GameState object
    private GameState gameState;

    // Reference to the TMP Text component
    public TMP_Text bulletText;

    private void Start()
    {
        // Get reference to the GameState singleton
        gameState = GameState.Instance;

        // Initialize the bullet text based on the current bullet count
        UpdateBulletCountText();
    }

    private void Update()
    {
        // Continuously check if the bullet count has changed, and update the text accordingly
        UpdateBulletCountText();
    }

    private void UpdateBulletCountText()
    {
        int enemyBulletCount = gameState.EnemyBulletCount;

        // Update the bullet text component with the current bullet count
        bulletText.text = enemyBulletCount.ToString();
    }
}
