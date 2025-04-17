using UnityEngine;
using TMPro;
using System.Collections;

public class PlayerNumberTextManager : MonoBehaviour
{
    private TextMeshProUGUI playerIdText;
    private GameState gameState;

    [SerializeField] private string playerPrefix = "Player ";
    [SerializeField] private string noPlayerText = "Not Logged In";

    private void Awake()
    {
        // Get components
        playerIdText = GetComponent<TextMeshProUGUI>();

        if (playerIdText == null)
        {
            Debug.LogError("PlayerNumberTextManager: TextMeshProUGUI component not found!");
        }
    }

    private void Start()
    {
        // Get GameState reference
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("PlayerNumberTextManager: GameState instance not found!");
            return;
        }

        // Update the text initially
        UpdatePlayerText();
    }

    private void Update()
    {
        // Check for changes in PlayerID and update text
        UpdatePlayerText();
    }

    private void UpdatePlayerText()
    {
        if (playerIdText == null || gameState == null)
            return;

        // Check if player is logged in (assuming PlayerID of 0 means not logged in)
        if (gameState.PlayerID <= 0)
        {
            playerIdText.text = noPlayerText;
        }
        else
        {
            playerIdText.text = playerPrefix + gameState.PlayerID.ToString();
        }
    }
}
