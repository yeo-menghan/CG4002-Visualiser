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
        // Get reference to GameState
        gameState = GameState.Instance;

        // Set initial state
        enemyPrefab.SetActive(true);
        shieldPrefab.SetActive(false);
        gunPrefab.SetActive(true);
    }

    void Update()
    {
        // Check if shield should be active
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
        gunPrefab.SetActive(true);
    }
}
