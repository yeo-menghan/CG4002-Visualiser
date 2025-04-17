using UnityEngine;
using UnityEngine.UI;
using M2MqttUnity;
using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt.Messages;
using System;
using TMPro;

public class MQTTCommsManager : M2MqttUnityClient
{
    public static MQTTCommsManager Instance { get; private set; }

    [Header("MQTT Settings")]
    [SerializeField] private string mqttGameStateTopic = "visualiser/game_state"; // Subscribe
    [SerializeField] private string mqttVisibilityCheck = "visualiser/visibility_feedback"; // Publish
    [SerializeField] private string mqttVisibilityRequest = "visualiser/req_visibility"; // Subscribe to Server pings
    [SerializeField] private string mqttDeviceStatusTopic = "visualiser/device_status"; // Subscribe (Device connection status)

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI deviceDisconnectPromptText;

    [Serializable]
    public class PlayerState
    {
        public int hp;
        public int bullets;
        public int bombs;
        public int shield_hp;
        public int deaths;
        public int shields;
        public bool is_visible;
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
        public string hit;
        public GameStateData game_state;
    }

    [Serializable]
    public class VisibilityRequestMessage
    {
        public int player_id;
        public string topic;
    }

    [Serializable]
    public class PlayerDeviceStatus
    {
        public bool gun_connected;
        public bool vest_connected;
        public bool glove_connected;
    }

    [Serializable]
    public class DeviceStatusMessage
    {
        public PlayerDeviceStatus player_1;
        public PlayerDeviceStatus player_2;
    }

    // List to store incoming messages
    private List<string> messageQueue = new List<string>();
    private bool isConnected = false;
    private GameState gameState;

    // Track previous state for publishing changes
    private PlayerState previousLocalPlayerState = new PlayerState();
    private PlayerState previousEnemyPlayerState = new PlayerState();

    private void Awake()
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

        if (deviceDisconnectPromptText != null)
        {
            deviceDisconnectPromptText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("MQTTCommsManager: deviceDisconnectPromptText is not assigned in the Inspector!");
        }


        gameState = GameState.Instance;

        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        if (messageQueue.Count > 0)
        {
            foreach (string message in messageQueue)
            {
                ProcessMessage(message);
            }
            messageQueue.Clear();
        }
    }

    public override void Connect()
    {
        if (!IsNetworkAvailable())
        {
            OnConnectionFailed("No internet connection available");
            return;
        }

        Debug.Log($"Attempting to connect to MQTT broker at {brokerAddress}:{brokerPort}");

        try
        {
            base.Connect();
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

    private bool IsNetworkAvailable()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("No internet connection available!");
            return false;
        }
        return true;
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Connected to MQTT broker: " + brokerAddress);
        isConnected = true;
        gameState.MqConnected = true;

    }

    protected override void SubscribeTopics()
    {

        string[] topics = new string[] {
            mqttGameStateTopic,
            mqttVisibilityRequest,
            mqttDeviceStatusTopic
        };

        byte[] qosLevels = new byte[] {
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, // QOS for game state/actions
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE, // QOS for visibility requests
            MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE  // QOS for device status
        };

        client.Subscribe(topics, qosLevels);
        Debug.Log("Subscribed to topics: " + string.Join(", ", topics));
    }

    protected override void UnsubscribeTopics()
    {
        if (client != null && client.IsConnected)
        {
            string[] topics = new string[] {
                mqttGameStateTopic,
                mqttVisibilityRequest,
                mqttDeviceStatusTopic
            };
            Debug.Log("Unsubscribing from topics: " + string.Join(", ", topics));
            client.Unsubscribe(topics);
        }
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        if (topic == mqttVisibilityRequest)
        {
            ProcessVisibilityRequest(msg);
        }
        else if (topic == mqttDeviceStatusTopic)
        {
            ProcessDeviceStatusMessage(msg);
        }
        else if (topic == mqttGameStateTopic)
        {
            lock(messageQueue)
            {
                messageQueue.Add(msg);
            }
        }
        else
        {
            Debug.LogWarning($"Received message on unhandled topic: {topic}");
        }
    }

    private void ProcessVisibilityRequest(string message)
    {
        Debug.Log("Processing visibility request: " + message);

        try
        {
            VisibilityRequestMessage requestMsg = JsonUtility.FromJson<VisibilityRequestMessage>(message);

            if (requestMsg.player_id == gameState.PlayerID)
            {
                SendPlayerVisibilityMessage();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse visibility request message: " + e.Message);
        }
    }


    private void ProcessDeviceStatusMessage(string message)
    {
        Debug.Log("Processing device status message: " + message);

        try
        {
            DeviceStatusMessage statusMsg = JsonUtility.FromJson<DeviceStatusMessage>(message);

            if (statusMsg != null && statusMsg.player_1 != null && statusMsg.player_2 != null)
            {
                bool changed = false;

                PlayerDeviceStatus localPlayerStatus = (gameState.PlayerID == 1) ? statusMsg.player_1 : statusMsg.player_2;
                PlayerDeviceStatus remotePlayerStatus = (gameState.PlayerID == 1) ? statusMsg.player_2 : statusMsg.player_1; // Not strictly needed for this feature, but good for context

                if (gameState.P1_gun != statusMsg.player_1.gun_connected) { gameState.P1_gun = statusMsg.player_1.gun_connected; changed = true; }
                if (gameState.P1_vest != statusMsg.player_1.vest_connected) { gameState.P1_vest = statusMsg.player_1.vest_connected; changed = true; }
                if (gameState.P1_glove != statusMsg.player_1.glove_connected) { gameState.P1_glove = statusMsg.player_1.glove_connected; changed = true; }
                if (gameState.P2_gun != statusMsg.player_2.gun_connected) { gameState.P2_gun = statusMsg.player_2.gun_connected; changed = true; }
                if (gameState.P2_vest != statusMsg.player_2.vest_connected) { gameState.P2_vest = statusMsg.player_2.vest_connected; changed = true; }
                if (gameState.P2_glove != statusMsg.player_2.glove_connected) { gameState.P2_glove = statusMsg.player_2.glove_connected; changed = true; }


                if (deviceDisconnectPromptText != null)
                {
                    bool allLocalDevicesDisconnected = !localPlayerStatus.gun_connected &&
                                                        !localPlayerStatus.vest_connected &&
                                                        !localPlayerStatus.glove_connected;

                    if (allLocalDevicesDisconnected)
                    {
                         if (!deviceDisconnectPromptText.gameObject.activeSelf)
                        {
                            deviceDisconnectPromptText.text = $"Please scan the ground QR code";
                            deviceDisconnectPromptText.gameObject.SetActive(true);
                            Debug.Log($"Player {gameState.PlayerID} all devices disconnected, showing prompt.");
                        }
                    }
                    else
                    {
                        if (deviceDisconnectPromptText.gameObject.activeSelf)
                        {
                            deviceDisconnectPromptText.gameObject.SetActive(false);
                            Debug.Log($"Player {gameState.PlayerID} has connected devices, hiding prompt.");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("deviceDisconnectPromptText not assigned, cannot show device status prompt.");
                }


                Debug.Log($"Updated device status: P1(G:{gameState.P1_gun}, V:{gameState.P1_vest}, Gl:{gameState.P1_glove}), P2(G:{gameState.P2_gun}, V:{gameState.P2_vest}, Gl:{gameState.P2_glove})");

                if (changed)
                {
                    gameState.NotifyDeviceStatusChanged();
                }
            }
            else
            {
                Debug.LogError("Failed to parse device status message or message structure is incorrect. Message: " + message);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing device status message: {e.Message}\nStackTrace: {e.StackTrace}\nMessage: {message}");
        }
    }

    private void ProcessMessage(string message)
    {
        Debug.Log("Processing MQTT message: " + message);

        try
        {
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

    private void ProcessActionMessage(ActionMessage actionMsg)
    {
        bool isFromOpponent = actionMsg.player_id != gameState.PlayerID;
        Debug.Log($"MQTTCommsManager actionMsg.hit: {actionMsg.hit}");

        if (actionMsg.hit == "true")
        {
            if (isFromOpponent)
            {
                gameState.PlayerHit = true;
                Debug.Log("Generic player hit animation triggered");
            }
            else
            {
                gameState.EnemyHit = true;
                Debug.Log("Generic enemy hit animation triggered");
            }
        }
        else{
          if (isFromOpponent)
          {
              gameState.PlayerHit = false;

          }
          else
          {
              gameState.EnemyHit = false;
          }
        }

        if (isFromOpponent)
        {
            gameState.HandleEnemyGameAction(actionMsg.action);
            Debug.Log($"Enemy action '{actionMsg.action}' handled");
        }
        else
        {
            gameState.HandleGameAction(actionMsg.action);
            Debug.Log($"Local player action '{actionMsg.action}' handled");
        }

        if (actionMsg.game_state != null)
        {
            ProcessGameStateMessage(actionMsg.game_state);
        }
    }

    private void ProcessGameStateMessage(GameStateData stateData)
    {
        if (stateData == null)
            return;

        PlayerState localPlayerState = gameState.PlayerID == 1 ? stateData.p1 : stateData.p2;
        PlayerState enemyPlayerState = gameState.PlayerID == 1 ? stateData.p2 : stateData.p1;

        if (localPlayerState != null)
        {
            gameState.PlayerCurrentHealth = localPlayerState.hp;
            gameState.PlayerBulletCount = localPlayerState.bullets;
            gameState.PlayerBombCount = localPlayerState.bombs;
            gameState.PlayerCurrentShield = localPlayerState.shield_hp;
            gameState.PlayerScore = enemyPlayerState.deaths;
            gameState.PlayerShieldCount = localPlayerState.shields;

            if (localPlayerState.login)
            {
                Debug.Log("Local player logged in");
            }

            if (localPlayerState.disconnected)
            {
                Debug.Log("Local player disconnected");
            }
        }

        if (enemyPlayerState != null)
        {
            gameState.EnemyCurrentHealth = enemyPlayerState.hp;
            gameState.EnemyBulletCount = enemyPlayerState.bullets;
            gameState.EnemyBombCount = enemyPlayerState.bombs;
            gameState.EnemyCurrentShield = enemyPlayerState.shield_hp;
            gameState.EnemyScore = localPlayerState.deaths;
            gameState.EnemyShieldCount = enemyPlayerState.shields;
        }
    }

    public void SendPlayerVisibilityMessage()
    {
        if (client != null && client.IsConnected)
        {
            string jsonMessage = $"{{\"player_id\": {gameState.PlayerID}, \"is_visible\": \"{gameState.EnemyActive.ToString().ToLower()}\", \"bombs_on_player\": {gameState.EnemyInBombCount}}}";

            client.Publish(mqttVisibilityCheck, System.Text.Encoding.UTF8.GetBytes(jsonMessage), MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, false);
            Debug.Log("Published visibility feedback message: " + jsonMessage);
        }
        else
        {
            Debug.LogWarning("Cannot publish - not connected to broker");
        }
    }

    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.LogError("Connection failed: " + errorMessage);
        gameState.MqConnected = false;
    }

    protected override void OnDisconnected()
    {
        Debug.Log("Disconnected from broker");
        isConnected = false;
        gameState.MqConnected = false;
    }

    private void OnApplicationQuit()
    {
        Disconnect();
    }
}
