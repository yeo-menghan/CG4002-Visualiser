using UnityEngine;

public class BombTracker : MonoBehaviour
{
    private GameState gameState;
    private int bombId;

    public void Initialize(int id)
    {
        gameState = GameState.Instance;
        bombId = id;
    }

    private void Update()
    {
        if (gameState != null)
        {
            gameState.UpdateBombPosition(bombId, transform.position);
        }
    }
}
