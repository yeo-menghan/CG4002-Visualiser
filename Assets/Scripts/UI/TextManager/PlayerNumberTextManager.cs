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
        playerIdText = GetComponent<TextMeshProUGUI>();

        if (playerIdText == null)
        {
            Debug.LogError("PlayerNumberTextManager: TextMeshProUGUI component not found!");
        }
    }

    private void Start()
    {
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("PlayerNumberTextManager: GameState instance not found!");
            return;
        }

        UpdatePlayerText();
    }

    private void Update()
    {
        UpdatePlayerText();
    }

    private void UpdatePlayerText()
    {
        if (playerIdText == null || gameState == null)
            return;

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
