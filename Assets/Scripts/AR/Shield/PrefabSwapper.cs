using UnityEngine;

public class PrefabSwapper : MonoBehaviour
{
    private GameState gameState;

    public GameObject playerPrefab;
    public GameObject shieldPrefab;

    private GameObject currentObject;

    private bool isShieldActive = false;

    void Start()
    {
        gameState = GameState.Instance;

        SpawnPrefab(playerPrefab);
        isShieldActive = false;
    }

    void Update()
    {
        bool shouldShieldBeActive = gameState.EnemyCurrentShield > 0;

        if (shouldShieldBeActive != isShieldActive)
        {
            if (shouldShieldBeActive)
            {
                SpawnPrefab(shieldPrefab);
                isShieldActive = true;
            }
            else
            {
                SpawnPrefab(playerPrefab);
                isShieldActive = false;
            }
        }
    }

    private void SpawnPrefab(GameObject prefab)
    {
        if (currentObject != null)
        {
            Destroy(currentObject);
        }

        currentObject = Instantiate(prefab, transform.position, transform.rotation);
        currentObject.transform.parent = transform;
    }
}
