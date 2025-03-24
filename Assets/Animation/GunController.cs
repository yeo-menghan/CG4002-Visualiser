using UnityEngine;

public class GunController : MonoBehaviour
{
    private GameState gameState;
    public Animator gunAnimator;
    public GameObject bulletHitEffectPrefab;

    private void Start()
    {
        gameState = GameState.Instance;
        gameState.gameActionOccurred.AddListener(HandleShoot);
    }

    void OnDestroy()
    {
        gameState.gameActionOccurred.RemoveListener(HandleShoot);
    }

    public void HandleShoot(string actionType)
    {
        if (actionType == "shoot")
        {
            if (gameState.EnemyActive && gameState.PlayerBulletCount > 0)
            {
                Shoot(); // TODO: add muzzle flash
                SpawnPrefabOnEnemy(bulletHitEffectPrefab);
            }
            else
            {
                Shoot();
            }
        }
    }

    public void Shoot()
    {
        if (gunAnimator != null)
        {
            gunAnimator.SetTrigger("Shoot");
            Debug.Log("GunController: Shooting");
        }
    }

    private GameObject SpawnPrefabOnEnemy(GameObject prefab)
    {
        if (prefab == null || gameState.EnemyCoordinateTransform == null)
        {
            Debug.LogWarning("GunController: Prefab or enemy transform is null.");
            return null;
        }

        GameObject hitEffect = Instantiate(prefab, gameState.EnemyCoordinateTransform.position, Quaternion.identity);
        Debug.Log($"Spawned {prefab.name} at enemy position {gameState.EnemyCoordinateTransform.position}");
        Destroy(hitEffect, 2f);
        return hitEffect;
    }


}
