using UnityEngine;
using TMPro;

public class EnemyShieldCountTextManager : MonoBehaviour
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
        int enemyShieldCount = gameState.EnemyShieldCount;

        shieldText.text = enemyShieldCount.ToString();
    }
}
