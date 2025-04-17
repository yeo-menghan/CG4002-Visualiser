using UnityEngine;
using Vuforia;

public class PrefabToggler : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject shieldPrefab;
    public GameObject gunPrefab;

    private GameState gameState;

    void Start()
    {
        gameState = GameState.Instance;
        enemyPrefab.SetActive(true);
        shieldPrefab.SetActive(false);
    }

    void Update()
    {
        bool shouldShieldBeActive = gameState.EnemyCurrentShield > 0;
        if (shouldShieldBeActive)
        {
            shieldPrefab.SetActive(true);
            enemyPrefab.SetActive(false);
        }
        else
        {
            shieldPrefab.SetActive(false);
            enemyPrefab.SetActive(true);
        }
    }
}
