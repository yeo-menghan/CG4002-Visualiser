using UnityEngine;
using TMPro;

public class EnemyBombCountTextManager : MonoBehaviour
{
    private GameState gameState;
    public TMP_Text bombText;

    private void Start()
    {
        gameState = GameState.Instance;

        UpdateBombCountText();
    }

    private void Update()
    {
        UpdateBombCountText();
    }

    private void UpdateBombCountText()
    {
        int enemyBombCount = gameState.EnemyBombCount;

        bombText.text = enemyBombCount.ToString();
    }
}
