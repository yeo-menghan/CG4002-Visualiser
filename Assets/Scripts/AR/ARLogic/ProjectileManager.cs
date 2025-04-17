using UnityEngine;
using System.Collections;

public class ProjectileManager : MonoBehaviour
{
    private GameState gameState;
    public ProjectileLauncher projectileLauncher;
    public PlayerBombHandler bombHandler;
    public ActionWaitBar actionWaitBar;

    void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(HandleProjectile);
        Debug.Log("ProjectileManager: ProjectileManager Ready");
        if (projectileLauncher == null)
        {
            Debug.LogError("ProjectileManager: ProjectileLauncher is not assigned. Please assign it in the Inspector.");
        }

        if (bombHandler == null)
        {
            bombHandler = GetComponent<PlayerBombHandler>();
            if (bombHandler == null)
            {
                Debug.LogError("ProjectileManager: PlayerBombHandler is not assigned or found. Please assign it in the Inspector.");
            }
        }
    }

    void OnDestroy()
    {
        if(gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(HandleProjectile);
        }
    }

    public void HandleProjectile(string actionType)
    {
        if (actionType == "golf")
        {
            if (projectileLauncher != null)
            {
                Debug.Log("ProjectileManager: Preparing to fire Golf");
                projectileLauncher.FireProjectile("Golf");
                Debug.Log("ProjectileManager: Fired Golf Projectile");
            }
            StartCoroutine(DelayWaitAction());
            Debug.Log("ProjectileManager: Delay Wait Golf Projectile");
        }
        else if (actionType == "badminton")
        {
            if (projectileLauncher != null)
            {
                projectileLauncher.FireProjectile("Badminton");
                Debug.Log("ProjectileManager: Fired Badminton Projectile");
            }
            StartCoroutine(DelayWaitAction());
            Debug.Log("ProjectileManager: Delay Wait Badminton Projectile");
        }
        else if (actionType == "bomb")
        {
            if (projectileLauncher != null && gameState.PlayerBombCount > 0 && bombHandler != null)
            {
                projectileLauncher.FireProjectile("Bomb");
                Debug.Log("ProjectileManager: Fired Bomb Projectile");
                if (gameState.EnemyActive)
                {
                    StartCoroutine(DelayBombEffect());
                    Debug.Log("ProjectileManager: Spawned snow cloud on enemy");
                }
            }
            else
            {
                Debug.Log("ProjectileManager: Cannot fire bomb - no bombs available, no active enemy, or missing bomb handler");
            }

            StartCoroutine(DelayWaitAction());
        }
        else
        {
            Debug.LogWarning($"ProjectileManager: Unhandled actionType '{actionType}' or no enemy active.");
        }
    }

    private IEnumerator DelayBombEffect()
    {
        float estimatedTimeToReachTarget = 2f;

        Debug.Log($"ProjectileManager: Waiting {estimatedTimeToReachTarget} seconds for projectile to reach target");
        yield return new WaitForSeconds(estimatedTimeToReachTarget);

        bombHandler.SpawnBombOnEnemy();
        Debug.Log("ProjectileManager: Spawned snow cloud on enemy");
    }

    private IEnumerator DelayWaitAction()
    {
        float estimatedTimeToReachTarget = 2f;

        Debug.Log($"ProjectileManager: Waiting {estimatedTimeToReachTarget} seconds for projectile to reach target");
        yield return new WaitForSeconds(estimatedTimeToReachTarget);

        actionWaitBar.StartWait();
    }
}
