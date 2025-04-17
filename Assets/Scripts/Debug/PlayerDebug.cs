using UnityEngine;
using System.Collections;

public class PlayerDebug : MonoBehaviour
{
    private GameState gameState;

    void Start()
    {
        gameState = GameState.Instance;
    }

    public void Reload()
    {
        gameState.HandleGameAction("reload");
    }

    public void UseBomb()
    {
        if (gameState.EnemyActive)
        {
            gameState.EnemyHit = true;
        }
        else
        {
            gameState.EnemyHit = false;
        }
        gameState.HandleGameAction("bomb");
    }

    public void UseShield()
    {
        gameState.HandleGameAction("shield");
    }

    public void UseBullet()
    {
        Debug.Log("Player: UseBullet method called");
        if (gameState.EnemyActive)
        {
            gameState.EnemyHit = true;
        }
        else
        {
            gameState.EnemyHit = false;
        }
        gameState.HandleGameAction("gun");
        Debug.Log("Player: UseBullet completed");
    }

    public void Badminton()
    {
        gameState.HandleGameAction("badminton");
    }

    public void Golf()
    {
        gameState.HandleGameAction("golf");
    }

    public void Boxing()
    {
        gameState.HandleGameAction("boxing");
    }

    public void Fencing()
    {
        gameState.HandleGameAction("fencing");
    }

}
