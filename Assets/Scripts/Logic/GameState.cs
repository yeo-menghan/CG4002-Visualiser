using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class GameState : MonoBehaviour
{
    // Singleton instance
    public static GameState Instance { get; private set; }

    // Health and Shield Constants
    public const int MAX_HEALTH = 100;
    public const int MIN_HEALTH = 0;
    public const int MAX_SHIELD = 30;
    public const int MIN_SHIELD = 0;

     // Ammo Constants
    public const int MAX_SHIELD_COUNT = 3;
    public const int MIN_SHIELD_COUNT = 0;

    public const int MAX_BULLET_COUNT = 6;
    public const int MIN_BULLET_COUNT = 0;

    public const int MAX_BOMB_COUNT = 2;
    public const int MIN_BOMB_COUNT = 0;

    // Damage
    public const int BULLET_DAMAGE = 5;
    public const int BOMB_DAMAGE = 5;
    public const int ACTION_DAMAGE = 10;


    // Player Stats
    private int playerId, playerCurrentHealth, playerCurrentShield, playerScore, playerShieldCount, playerBulletCount, playerBombCount;
    private int playerInBombCount;
    private bool playerVisibleToEnemy, playerHit, playerShieldHit;

    // Enemy Stats
    private int enemyCurrentHealth, enemyCurrentShield, enemyScore, enemyShieldCount, enemyBulletCount, enemyBombCount;
    private int enemyInBombCount;
    private bool enemyActive, enemyHit, enemyShieldHit;
    private Transform enemyCoordinateTransform;
    private Dictionary<int, Vector3> activeEnemyBombs = new Dictionary<int, Vector3>();
    private int nextEnemyBombId = 0;

    // Comms
    private bool mqConnected, actionQueueConnected, statusUpdateConnected, predictionConnected;

    // Events for property changes
    public event Action EnemyActiveChanged; // Enemy Visibility

    // Events for playing hit animations
    public event Action PlayerHitEvent;
    public event Action PlayerShieldHitEvent;
    public event Action EnemyHitEvent;
    public event Action EnemyShieldHitEvent;

    // UnityEvent for game actions
    public UnityActionEvent gameActionOccurred;

    // UnityEvent for enemy game actions
    public UnityActionEvent enemyGameActionOccurred;

    // UnityEvent for received predictions
    public UnityPredictionEvent predictionOccurred;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Initialize player stats
        playerId = 1;
        playerCurrentHealth = MAX_HEALTH;
        playerCurrentShield = MIN_SHIELD;
        playerScore = 0;
        playerShieldCount = MAX_SHIELD_COUNT;
        playerBulletCount = MAX_BULLET_COUNT;
        playerBombCount = MAX_BOMB_COUNT;

        // Initialize enemy stats
        enemyCurrentHealth = MAX_HEALTH;
        enemyCurrentShield = MIN_SHIELD;
        enemyShieldCount = MAX_SHIELD_COUNT;
        enemyBulletCount = MAX_BULLET_COUNT;
        enemyBombCount = MAX_BOMB_COUNT;
        enemyScore = 0;
        enemyActive = false;

        // Initialize visibility
        playerVisibleToEnemy = false;

        // Comms
        mqConnected = false;
        actionQueueConnected = false;
        statusUpdateConnected = false;
        predictionConnected = false;
    }

    // =============================
    // Connection Methods
    // =============================

    public bool MqConnected
    {
        get { return mqConnected; }
        set { mqConnected = value; }
    }

    public bool ActionQueueConnected
    {
        get { return actionQueueConnected; }
        set { actionQueueConnected = value; }
    }

    public bool StatusUpdateConnected
    {
        get { return statusUpdateConnected; }
        set { statusUpdateConnected = value; }
    }

    public bool PredictionConnected
    {
        get { return predictionConnected; }
        set { predictionConnected = value; }
    }

    // =============================
    // Player Methods
    // =============================

    public bool PlayerVisibleToEnemy
    {
        get { return playerVisibleToEnemy; }
        set { playerVisibleToEnemy = value; }
    }

    public bool PlayerHit
    {
        get { return playerHit; }
        set
        {
            playerHit = value;
            if (playerHit)
            {
                PlayerHitEvent?.Invoke();
            }
        }
    }

    public bool PlayerShieldHit
    {
        get { return playerShieldHit; }
        set
        {
            playerShieldHit = value;
            if (playerShieldHit)
            {
                PlayerShieldHitEvent?.Invoke();
            }
        }
    }

    public bool EnemyHit
    {
        get { return enemyHit; }
        set
        {
            enemyHit = value;
            if (enemyHit)
            {
                EnemyHitEvent?.Invoke();
            }
        }
    }

    public bool EnemyShieldHit
    {
        get { return enemyShieldHit; }
        set
        {
            enemyShieldHit = value;
            if (enemyShieldHit)
            {
                EnemyShieldHitEvent?.Invoke();
            }
        }
    }

    public bool EnemyActive
    {
      get { return enemyActive; }
      set{
          if (enemyActive != value)
          {
              enemyActive = value;
              EnemyActiveChanged?.Invoke();
          }
      }
    }

    public int PlayerID
    {
        get { return playerId; }
        set { playerId = value; }
    }

    public int PlayerCurrentHealth
    {
        get { return playerCurrentHealth; }
        set { playerCurrentHealth = Mathf.Clamp(value, MIN_HEALTH, MAX_HEALTH); }
    }

    public int PlayerCurrentShield
    {
        get { return playerCurrentShield; }
        set { playerCurrentShield = Mathf.Clamp(value, MIN_SHIELD, MAX_SHIELD); }
    }

    public int EnemyCurrentHealth
    {
        get { return enemyCurrentHealth; }
        set { enemyCurrentHealth = Mathf.Clamp(value, MIN_HEALTH, MAX_HEALTH); }
    }

    public int EnemyCurrentShield
    {
        get { return enemyCurrentShield; }
        set { enemyCurrentShield = Mathf.Clamp(value, MIN_SHIELD, MAX_SHIELD); }
    }

    public int PlayerScore
    {
        get { return playerScore; }
        set { playerScore = value; }
    }

    public int EnemyScore
    {
        get { return enemyScore; }
        set { enemyScore = value; }
    }

    public int PlayerShieldCount
    {
        get { return playerShieldCount; }
        set { playerShieldCount = Mathf.Clamp(value, MIN_SHIELD_COUNT, MAX_SHIELD_COUNT); }
    }

    public int PlayerBulletCount
    {
        get { return playerBulletCount; }
        set { playerBulletCount = Mathf.Clamp(value, MIN_BULLET_COUNT, MAX_BULLET_COUNT); }
    }

    public int PlayerBombCount
    {
        get { return playerBombCount; }
        set { playerBombCount = Mathf.Clamp(value, MIN_BOMB_COUNT, MAX_BOMB_COUNT); }
    }

    public int EnemyShieldCount
    {
        get { return enemyShieldCount; }
        set { enemyShieldCount = Mathf.Clamp(value, MIN_SHIELD_COUNT, MAX_SHIELD_COUNT); }
    }

    public int EnemyBulletCount
    {
        get { return enemyBulletCount; }
        set { enemyBulletCount = Mathf.Clamp(value, MIN_BULLET_COUNT, MAX_BULLET_COUNT); }
    }

    public int EnemyBombCount
    {
        get { return enemyBombCount; }
        set { enemyBombCount = Mathf.Clamp(value, MIN_BOMB_COUNT, MAX_BOMB_COUNT); }
    }

    public Transform EnemyCoordinateTransform
    {
      get { return enemyCoordinateTransform; }
      set { enemyCoordinateTransform = value; }
    }

    public int PlayerInBombCount
    {
        get { return playerInBombCount; }
        set { playerInBombCount = Math.Clamp(value, 0, 999); }
    }
    public int EnemyInBombCount
    {
        get { return enemyInBombCount; }
        set { enemyInBombCount = Math.Clamp(value, 0, 999); }
    }

    // TODO: Abstract this away into a separate class (snowChecker?)
    // Method to register a new bomb
    public int RegisterBomb(Vector3 position)
    {
        int bombId = nextEnemyBombId++;
        activeEnemyBombs.Add(bombId, position);
        return bombId;
    }

    // Method to update the enemy's position in a bomb
    public void UpdateBombPosition(int bombId, Vector3 position)
    {
        if (activeEnemyBombs.ContainsKey(bombId))
        {
            activeEnemyBombs[bombId] = position;
            // Recalculate the enemy in bomb count
            UpdateEnemyInBombCount();
        }
    }

    // Method to update the count of bombs the enemy is inside
    public void UpdateEnemyInBombCount()
    {
        if (enemyCoordinateTransform == null) return;

        int count = 0;
        Vector3 enemyPosition = enemyCoordinateTransform.position;

        foreach (Vector3 bombPosition in activeEnemyBombs.Values)
        {
            if (Vector3.Distance(enemyPosition, bombPosition) <= 1.0f)
            {
                count++;
            }
        }

        EnemyInBombCount = count;
        Debug.Log($"GameState: EnemyInBombCount - {EnemyInBombCount}");
    }

    // If needed for debugging or game management
    public void ClearAllBombs()
    {
        activeEnemyBombs.Clear();
        EnemyInBombCount = 0;
    }

    // =============================
    // Increment / Decrement Methods
    // =============================
    public void IncrementBombCount()
    {
        PlayerBombCount = Mathf.Min(PlayerBombCount + 1, MAX_BOMB_COUNT);
    }

    public void DecrementBombCount()
    {
        PlayerBombCount = Mathf.Max(PlayerBombCount - 1, MIN_BOMB_COUNT);
    }

    public void IncrementShieldCount()
    {
        PlayerShieldCount = Mathf.Min(PlayerShieldCount + 1, MAX_SHIELD_COUNT);
    }

    public void DecrementShieldCount()
    {
        PlayerShieldCount = Mathf.Max(PlayerShieldCount - 1, MIN_SHIELD_COUNT);
    }

    public void IncrementBulletCount()
    {
        PlayerBulletCount = Mathf.Min(PlayerBulletCount + 1, MAX_BULLET_COUNT);
    }

    public void DecrementBulletCount()
    {
        PlayerBulletCount = Mathf.Max(PlayerBulletCount - 1, MIN_BULLET_COUNT);
    }

    public void PlayerReload() // Remove in production
    {
        PlayerBulletCount = MAX_BULLET_COUNT;
        HandleGameAction("Reload");
    }

    public void EnemyReload() // Remove in production
    {
        EnemyBulletCount = MAX_BULLET_COUNT;
    }

    // =============================
    // Game Action Methods
    // =============================

    public void HandleGameAction(string actionType)
    {
      Debug.Log($"HandleGameAction called with actionType: {actionType}");
      gameActionOccurred.Invoke(actionType);
      Debug.Log($"Game action occurred: {actionType}");
    }

    public void HandleEnemyGameAction(string actionType)
    {
      Debug.Log($"HandleEnemyGameAction called with actionType: {actionType}");
      enemyGameActionOccurred.Invoke(actionType);
      Debug.Log($"Enemy game action occurred: {actionType}");
    }

    // Method to handle game predictions sent
    public void HandlePredictionMessage(string predictionType, float predictionValue)
    {
      Debug.Log($"HandleGamePrediction called with predictionType: {predictionType} and predictionValue: {predictionValue}");
      predictionOccurred.Invoke(predictionType, predictionValue);
      Debug.Log($"Game prediction occurred: {predictionType} with value: {predictionValue}");
    }

    // UnityEvent for game actions
    [System.Serializable]
    public class UnityActionEvent : UnityEvent<string> { }

    [System.Serializable]
    public class UnityPredictionEvent : UnityEvent<string, float> { }
}
