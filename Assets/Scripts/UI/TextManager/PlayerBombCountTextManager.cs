using UnityEngine;
using TMPro;

public class PlayerBombCountTextManager : MonoBehaviour
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
        int playerBombCount = gameState.PlayerBombCount;

        bombText.text = playerBombCount.ToString();
    }
}
