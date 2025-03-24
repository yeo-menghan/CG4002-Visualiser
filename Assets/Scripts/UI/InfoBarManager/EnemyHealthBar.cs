using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private GameState gameState;
    public Slider healthBarSlider; // Changed from GameObject to Slider

    private void Start()
    {
        // Make sure GameState is initialized first
        gameState = GameState.Instance;
        if (gameState == null)
        {
            // Debug.LogError("GameState.Instance is null!");
            return;
        }

        // Check if the reference is set
        if (healthBarSlider == null)
        {
            // Debug.LogError("Health Bar Slider reference is missing!");
            return;
        }

        UpdateHealthBar();
        // Debug.Log("Enemy Health Value: " + gameState.EnemyCurrentHealth);
    }

    void Update()
    {
        // Only update if references are valid
        if (gameState != null && healthBarSlider != null)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        // Safety check
        if (gameState == null || healthBarSlider == null) return;

        int health = gameState.EnemyCurrentHealth;
        healthBarSlider.value = health;
        // Debug.Log("Updating Enemy Health Value: " + health);
    }
}
