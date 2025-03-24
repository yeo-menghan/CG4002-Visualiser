using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldTester : MonoBehaviour
{
    private GameState gameState;

    // UI References
    public Button addShieldButton;
    public Button removeShieldButton;
    public TextMeshProUGUI statusText;

    void Start()
    {
        // Get reference to GameState
        gameState = GameState.Instance;

        // Set up button listeners
        if (addShieldButton != null)
            addShieldButton.onClick.AddListener(AddShield);

        if (removeShieldButton != null)
            removeShieldButton.onClick.AddListener(RemoveShield);

        UpdateStatusText();
    }

    void Update()
    {
        // Optional keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.A))
            AddShield();

        if (Input.GetKeyDown(KeyCode.R))
            RemoveShield();

        // Keep UI text updated
        UpdateStatusText();
    }

    public void AddShield()
    {
        gameState.EnemyCurrentShield = GameState.MAX_SHIELD;
        Debug.Log("Added shield to enemy. Value: " + gameState.EnemyCurrentShield);
        UpdateStatusText();
    }

    public void RemoveShield()
    {
        gameState.EnemyCurrentShield = GameState.MIN_SHIELD;
        // Also trigger shield hit event to test the breaking animation
        gameState.EnemyShieldHit = true;
        Debug.Log("Removed shield from enemy. Value: " + gameState.EnemyCurrentShield);
        UpdateStatusText();
    }

    void UpdateStatusText()
    {
        if (statusText != null)
        {
            statusText.text = $"Enemy Shield: {gameState.EnemyCurrentShield}/{GameState.MAX_SHIELD}";
        }
    }

    // This will simulate shield getting hit (optional)
    public void HitShield()
    {
        gameState.EnemyShieldHit = true;
        Debug.Log("Shield hit triggered");
    }
}
