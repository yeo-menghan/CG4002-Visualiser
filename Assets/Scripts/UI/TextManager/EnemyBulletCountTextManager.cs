using UnityEngine;
using TMPro;

public class EnemyBulletCountTextManager : MonoBehaviour
{
    private GameState gameState;

    public TMP_Text bulletText;

    private void Start()
    {
        gameState = GameState.Instance;
        UpdateBulletCountText();
    }

    private void Update()
    {
        UpdateBulletCountText();
    }

    private void UpdateBulletCountText()
    {
        int enemyBulletCount = gameState.EnemyBulletCount;

        bulletText.text = enemyBulletCount.ToString();
    }
}
