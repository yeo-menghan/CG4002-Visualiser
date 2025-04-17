using UnityEngine;
using System.Collections;

public class EnemyProjectileManager : MonoBehaviour
{
    private GameState gameState;
    public EnemyProjectileLauncher projectileLauncher;
    public EnemyBombHandler bombHandler;

    void Start()
    {
        gameState = GameState.Instance;
        gameState.enemyGameActionOccurred.AddListener(HandleProjectile);
        Debug.Log("EnemyProjectileManager: ProjectileManager Ready");
        if (projectileLauncher == null)
        {
            Debug.LogError("EnemyProjectileManager: ProjectileLauncher is not assigned. Please assign it in the Inspector.");
        }

        if (bombHandler == null)
        {
            bombHandler = GetComponent<EnemyBombHandler>();
            if (bombHandler == null)
            {
                Debug.LogError("EnemyProjectileManager: EnemyBombHandler is not assigned or found. Please assign it in the Inspector.");
            }
        }
    }

    void OnDestroy()
    {
        if(gameState != null)
        {
            gameState.enemyGameActionOccurred.RemoveListener(HandleProjectile);
        }
    }

    public void HandleProjectile(string actionType)
    {
        if (actionType == "golf")
        {
            if (projectileLauncher != null)
            {
                Debug.Log("EnemyProjectileManager: Preparing to fire Golf");
                projectileLauncher.FireProjectile("Golf");
                Debug.Log("EnemyProjectileManager: Fired Golf Projectile");
            }
        }
        else if (actionType == "badminton")
        {
            if (projectileLauncher != null)
            {
                projectileLauncher.FireProjectile("Badminton");
                Debug.Log("EnemyProjectileManager: Fired Badminton Projectile");
            }
        }
        else if (actionType == "bomb")
        {
            if (projectileLauncher != null && gameState.EnemyBombCount > 0 && bombHandler != null)
            {
                projectileLauncher.FireProjectile("Bomb");
                Debug.Log("EnemyProjectileManager: Fired Bomb Projectile");
                // if (gameState.EnemyActive)
                // {
                //     StartCoroutine(DelayBombEffect());
                //     Debug.Log("EnemyProjectileManager: Spawned snow cloud on enemy");
                // }
            }
            else
            {
                Debug.Log("EnemyProjectileManager: Cannot fire bomb - no bombs available, no active enemy, or missing bomb handler");
            }
        }
        else
        {
            Debug.LogWarning($"EnemyProjectileManager: Unhandled actionType '{actionType}' or no enemy active.");
        }
    }

    private IEnumerator DelayBombEffect()
    {
        float estimatedTimeToReachTarget = 2f;

        Debug.Log($"EnemyProjectileManager: Waiting {estimatedTimeToReachTarget} seconds for projectile to reach target");
        yield return new WaitForSeconds(estimatedTimeToReachTarget);

        bombHandler.SpawnBombOnPlayer();
        Debug.Log("EnemyProjectileManager: Spawned snow cloud on player");
    }
}
