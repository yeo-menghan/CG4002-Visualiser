using UnityEngine;
using UnityEngine.UI;

public class PlayerShieldBar : MonoBehaviour
{
    private GameState gameState;
    [SerializeField] private Slider shieldBarSlider;

    private void Start()
    {
        gameState = GameState.Instance;
        if (gameState == null)
        {
            return;
        }

        if (shieldBarSlider == null)
        {
            return;
        }

        UpdateShieldBar();
    }

    void Update()
    {
        if (gameState != null && shieldBarSlider != null)
        {
            UpdateShieldBar();
        }
    }

    private void UpdateShieldBar()
    {
        if (gameState == null || shieldBarSlider == null) return;

        int shield = gameState.PlayerCurrentShield;
        shieldBarSlider.value = shield;
    }
}
