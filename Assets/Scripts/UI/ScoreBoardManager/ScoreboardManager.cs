using UnityEngine;
using TMPro;

public class ScoreboardManager : MonoBehaviour
{
    private GameState gameState;
    public TMP_Text playerScoreText;
    public TMP_Text enemyScoreText;

    private void Start()
    {
        gameState = GameState.Instance;

        UpdateScoreboard();
    }

    private void Update()
    {
        UpdateScoreboard();
    }

    private void UpdateScoreboard()
    {
        int playerScore = gameState.PlayerScore;
        int enemyScore = gameState.EnemyScore;

        playerScoreText.text = playerScore.ToString();
        enemyScoreText.text = enemyScore.ToString();
    }
}
