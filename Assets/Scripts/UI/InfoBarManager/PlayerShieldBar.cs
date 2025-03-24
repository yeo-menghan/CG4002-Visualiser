using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : MonoBehaviour
{
    private GameState gameState;
    [SerializeField] private Slider shieldBarSlider;

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
        if (shieldBarSlider == null)
        {
            // Debug.LogError("Shield Bar Slider reference is missing!");
            return;
        }

        UpdateShieldBar();
        // Debug.Log("Player Shield Value: " + gameState.PlayerCurrentShield);
    }

    void Update()
    {
        // Only update if references are valid
        if (gameState != null && shieldBarSlider != null)
        {
            UpdateShieldBar();
        }
    }

    private void UpdateShieldBar()
    {
        // Safety check
        if (gameState == null || shieldBarSlider == null) return;

        int shield = gameState.PlayerCurrentShield;
        shieldBarSlider.value = shield;
        // Debug.Log("Updating Player Shield Value: " + shield);
    }
}
