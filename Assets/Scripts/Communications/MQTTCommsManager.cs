using UnityEngine;
using UnityEngine.UI;
using M2MqttUnity;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;

public class MQTTCommsManager : M2MqttUnityClient
{
    // Singleton pattern
    public static MQTTCommsManager Instance { get; private set; }

    [Header("MQTT Settings")]
    [SerializeField] private string mqttGameStateTopic = "visualiser/game_state"; // Subscribe
    [SerializeField] private string mqttVisibilityCheck = "visualiser/visibility_feedback"; // Publish
    [SerializeField] private string mqttVisibilityRequest = "visualiser/req_visibility"; // Subscribe to Server pings

    // Data structures from CommsManager
    [Serializable]
    public class PlayerState
    {
        public int hp;
        public int bullets;
        public int bombs;
        public int shield_hp;
        public int deaths;
        public int shields;
        public bool opponent_hit;
        public bool opponent_shield_hit;
        public bool is_visible;
        public int num_bombs_player_on;
        public bool glove_connected;
        public bool vest_connected;
        public bool disconnected;
        public bool login;
    }

    [Serializable]
    public class GameStateData
    {
        public PlayerState p1;
        public PlayerState p2;
    }

    [Serializable]
    public class GameStateMessage
    {
        public GameStateData game_state;
    }

    [Serializable]
    public class ActionMessage
    {
        public string action;
        public int player_id;
        public bool hit;
        public GameStateData game_state;
    }

    [Serializable]
    public class VisibilityRequestMessage
    {
        public int player_id;
        public string topic;
    }

    // List to store incoming messages
    private List<string> messageQueue = new List<string>();
    private bool isConnected = false;
    private GameState gameState;

    // Track previous state for publishing changes
    private PlayerState previousLocalPlayerState = new PlayerState();
    private PlayerState previousEnemyPlayerState = new PlayerState();

    // Awake is called before Start
    private void Awake()
    {
        // Singleton implementation
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

    // Start is called before the first frame update
    protected override void Start()
    {
        try {
            Debug.Log("Available TLS protocols:");
            foreach (var protocol in System.Enum.GetValues(typeof(uPLibrary.Networking.M2Mqtt.MqttSslProtocols))) {
                Debug.Log($"- {protocol}");
            }
        } catch (Exception e) {
            Debug.LogError($"Error checking TLS protocols: {e.Message}");
        }


        gameState = GameState.Instance;

        // Call base Start() which will handle the connection
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update(); // Call base method to process MQTT events

        // Process any messages in the queue
        if (messageQueue.Count > 0)
        {
            foreach (string message in messageQueue)
            {
                ProcessMessage(message);
            }
            messageQueue.Clear();
        }

        // No need to check for state changes here anymore since we're using the event
    }

    // Override the Connect method with the same access modifier as the parent class
    public override void Connect()
    {
        // Check network availability before trying to connect
        if (!IsNetworkAvailable())
        {
            OnConnectionFailed("No internet connection available");
            return;
        }

        Debug.Log($"Attempting to connect to MQTT broker at {brokerAddress}:{brokerPort}");

        try
        {
            base.Connect(); // Call the parent's Connect method
        }
        catch (Exception e)
        {
            string errorDetails = $"{e.GetType().Name}: {e.Message}";

            if (e.InnerException != null)
                errorDetails += $" | Inner: {e.InnerException.GetType().Name}: {e.InnerException.Message}";

            Debug.LogError("MQTT connection failed: " + errorDetails);
            OnConnectionFailed(errorDetails);
        }
    }

    // Keep this helper method
    private bool IsNetworkAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection available!");
            return false;
        }
        return true;
    }

    // Called when connected to broker
    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Connected to MQTT broker: " + brokerAddress);
        isConnected = true;
        gameState.MqConnected = true;

    }

    // Subscribe to topics
    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] {
            mqttGameStateTopic,
            mqttVisibilityRequest  // Subscribe to visibility request pings
        }, new byte[] {
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE,
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE
        });

        Debug.Log("Subscribed to topics: " + mqttGameStateTopic + ", " + mqttVisibilityRequest);
    }

    // Process messages received from the broker
    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);

        // If this is a visibility request ping, handle it immediately
        if (topic == mqttVisibilityRequest)
        {
            ProcessVisibilityRequest(msg);
        }
        else
        {
            // Otherwise queue up for normal processing
            messageQueue.Add(msg);
        }
    }

    // Process visibility request pings
    private void ProcessVisibilityRequest(string message)
    {
        Debug.Log("Processing visibility request: " + message);

        try
        {
            VisibilityRequestMessage requestMsg = JsonUtility.FromJson<VisibilityRequestMessage>(message);

            // Check if this request is for the current player
            if (requestMsg.player_id == gameState.PlayerID)
            {
                // Server is requesting our current visibility state, so send it
                SendPlayerVisibilityMessage();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse visibility request message: " + e.Message);
        }
    }

    // Process received messages
    private void ProcessMessage(string message)
    {
        Debug.Log("Processing MQTT message: " + message);

        try
        {
            // Try to parse as action message first
            ActionMessage actionMsg = JsonUtility.FromJson<ActionMessage>(message);

            if (!string.IsNullOrEmpty(actionMsg.action))
            {
                Debug.Log($"Received action message: {actionMsg.action} from player {actionMsg.player_id}");
                ProcessActionMessage(actionMsg);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.Log("Not an action message: " + e.Message);
        }

        try
        {
            // Try to parse as game state message
            GameStateMessage stateMsg = JsonUtility.FromJson<GameStateMessage>(message);

            if (stateMsg.game_state != null)
            {
                Debug.Log("Received game state message");
                ProcessGameStateMessage(stateMsg.game_state);
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse message: " + e.Message);
        }
    }

    // Process action messages
    private void ProcessActionMessage(ActionMessage actionMsg)
    {
        // Determine if this is a message from the opponent
        bool isFromOpponent = actionMsg.player_id != gameState.PlayerID;
        Debug.Log($"MQTTCommsManager actionMsg.hit: {actionMsg.hit}");

        // Handle hit animations generically based on the hit flag
        if (actionMsg.hit)
        {
            if (isFromOpponent)
            {
                // Opponent hit this player (either shield or HP)
                // Trigger a generic player hit animation
                gameState.PlayerHit = true;
                Debug.Log("Generic player hit animation triggered");
            }
            else
            {
                // Local player hit the opponent (either shield or HP)
                // Trigger a generic enemy hit animation
                gameState.EnemyHit = true;
                Debug.Log("Generic enemy hit animation triggered");
            }
        }

        if (isFromOpponent)
        {
            // If the opponent performed an action, trigger it in the game
            gameState.HandleEnemyGameAction(actionMsg.action);
            Debug.Log($"Enemy action '{actionMsg.action}' handled");
        }
        else
        {
            // This is a confirmation of the local player's action
            gameState.HandleGameAction(actionMsg.action);
            Debug.Log($"Local player action '{actionMsg.action}' handled");
        }

        // Update game state from the provided state data
        if (actionMsg.game_state != null)
        {
            ProcessGameStateMessage(actionMsg.game_state);
        }
    }

    // Process game state data
    private void ProcessGameStateMessage(GameStateData stateData)
    {
        if (stateData == null)
            return;

        PlayerState localPlayerState = gameState.PlayerID == 1 ? stateData.p1 : stateData.p2;
        PlayerState enemyPlayerState = gameState.PlayerID == 1 ? stateData.p2 : stateData.p1;

        // Update local player state
        if (localPlayerState != null)
        {
            gameState.PlayerCurrentHealth = localPlayerState.hp;
            gameState.PlayerBulletCount = localPlayerState.bullets;
            gameState.PlayerBombCount = localPlayerState.bombs;
            gameState.PlayerCurrentShield = localPlayerState.shield_hp;
            gameState.PlayerScore = enemyPlayerState.deaths;
            gameState.PlayerShieldCount = localPlayerState.shields;

            // Handle login status
            if (localPlayerState.login)
            {
                Debug.Log("Local player logged in");
            }

            // Handle disconnection
            if (localPlayerState.disconnected)
            {
                Debug.Log("Local player disconnected");
            }
        }

        // Update enemy player state
        if (enemyPlayerState != null)
        {
            gameState.EnemyCurrentHealth = enemyPlayerState.hp;
            gameState.EnemyBulletCount = enemyPlayerState.bullets;
            gameState.EnemyBombCount = enemyPlayerState.bombs;
            gameState.EnemyCurrentShield = enemyPlayerState.shield_hp;
            gameState.EnemyScore = localPlayerState.deaths;
            gameState.EnemyShieldCount = enemyPlayerState.shields;
            gameState.EnemyHit = localPlayerState.opponent_hit;
            gameState.EnemyShieldHit = localPlayerState.opponent_shield_hit;
        }
    }

    // Send player visibility message to the visibility feedback channel
    public void SendPlayerVisibilityMessage()
    {
        if (client != null && client.IsConnected)
        {
            // But JsonUtility can't serialize anonymous types, so we'll create a proper string
            string jsonMessage = $"{{\"player_id\": {gameState.PlayerID}, \"is_visible\": \"{gameState.EnemyActive.ToString().ToLower()}\", \"bombs_on_player\": {gameState.EnemyInBombCount}}}";

            client.Publish(mqttVisibilityCheck, System.Text.Encoding.UTF8.GetBytes(jsonMessage), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            Debug.Log("Published visibility feedback message: " + jsonMessage);
        }
        else
        {
            Debug.LogWarning("Cannot publish - not connected to broker");
        }
    }

    // Handle connection failure
    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.LogError("Connection failed: " + errorMessage);
        gameState.MqConnected = false;
    }

    // Handle disconnection
    protected override void OnDisconnected()
    {
        Debug.Log("Disconnected from broker");
        isConnected = false;
        gameState.MqConnected = false;
    }

    // Clean up when the application quits
    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
