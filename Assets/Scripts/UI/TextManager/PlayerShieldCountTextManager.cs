using UnityEngine;
using TMPro;

public class PlayerShieldCountTextManager : MonoBehaviour
{
    private GameState gameState;
    public TMP_Text shieldText;

    private void Start()
    {
        gameState = GameState.Instance;
        UpdateShieldCountText();
    }

    private void Update()
    {
        UpdateShieldCountText();
    }

    private void UpdateShieldCountText()
    {
        int playerShieldCount = gameState.PlayerShieldCount;

        shieldText.text = playerShieldCount.ToString();
    }
}
