using UnityEngine;
using TMPro;

public class PlayerBulletCountTextManager : MonoBehaviour
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
        int playerBulletCount = gameState.PlayerBulletCount;

        bulletText.text = playerBulletCount.ToString();
    }
}
