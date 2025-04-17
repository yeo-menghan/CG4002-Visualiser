using UnityEngine;
using Vuforia;
using TMPro;

public class UserLogManager : MonoBehaviour
{
    public static UserLogManager Instance { get; private set; }
    // public TargetSwitchManager TargetSwitchManager;
    public GameObject LoginScreen;
    public GameObject Player;
    public GameObject PlayerGUICanvas;
    public MQTTCommsManager mqttCommsManager;
    private bool loggedIn = false;

    // Add these tags or customize them based on your prefab tagging system
    [SerializeField] private string[] prefabTagsToDestroy = { "GamePrefab", "PlayerPrefab" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        LoginScreen.SetActive(true);
        VuforiaBehaviour.Instance.enabled = loggedIn; // Begin Vuforia Lifecycle
        PlayerGUICanvas.SetActive(false);
        Player.SetActive(false);
    }

    public void LoginUser(int userId)
    {
        Debug.Log($"Logging in for player {userId}.");
        loggedIn = true;
        GameState.Instance.PlayerID = userId;
        // TargetSwitchManager.TargetSwitch();
        VuforiaBehaviour.Instance.enabled = loggedIn;
        LoginScreen.SetActive(!loggedIn);
        PlayerGUICanvas.SetActive(loggedIn);
        Player.SetActive(loggedIn);
        Debug.Log($"Player {userId} logged in.");
    }

    public void LogoutUser()
    {
        Debug.Log($"Logging out player {GameState.Instance.PlayerID}.");
        loggedIn = false;

        // Clear all prefabs from the scene
        ClearAllPrefabs();

        LoginScreen.SetActive(!loggedIn);
        PlayerGUICanvas.SetActive(loggedIn);
        Player.SetActive(loggedIn);
        VuforiaBehaviour.Instance.enabled = loggedIn;
        Debug.Log($"Player {GameState.Instance.PlayerID} logged out.");
    }

    // New methods for button-based login
    public void LoginPlayer1()
    {
        LoginUser(1);
    }

    public void LoginPlayer2()
    {
        LoginUser(2);
    }

    private void ClearAllPrefabs()
    {
        // Method 1: Destroy objects by tag
        foreach (string tag in prefabTagsToDestroy)
        {
            if (!string.IsNullOrEmpty(tag))
            {
                GameObject[] objectsToDestroy = GameObject.FindGameObjectsWithTag(tag);
                foreach (GameObject obj in objectsToDestroy)
                {
                    Destroy(obj);
                }
            }
        }

        Debug.Log("Cleared all prefabs from the scene");
    }
}
