using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    private GameState gameState;
    public Slider healthBarSlider;

    private void Start()
    {
        gameState = GameState.Instance;
        if (gameState == null)
        {
            return;
        }

        if (healthBarSlider == null)
        {
            return;
        }

        UpdateHealthBar();
    }

    void Update()
    {
        if (gameState != null && healthBarSlider != null)
        {
            UpdateHealthBar();
        }
    }

    private void UpdateHealthBar()
    {
        if (gameState == null || healthBarSlider == null) return;

        int health = gameState.EnemyCurrentHealth;
        healthBarSlider.value = health;
    }
}
