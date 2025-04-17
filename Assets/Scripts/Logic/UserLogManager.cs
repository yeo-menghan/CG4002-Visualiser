using UnityEngine;
using Vuforia;
using TMPro;

public class UserLogManager : MonoBehaviour
{
    public static UserLogManager Instance { get; private set; }
    public GameObject LoginScreen;
    public GameObject Player;
    public GameObject PlayerGUICanvas;
    public MQTTCommsManager mqttCommsManager;
    private bool loggedIn = false;

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
        VuforiaBehaviour.Instance.enabled = loggedIn;
        PlayerGUICanvas.SetActive(false);
        Player.SetActive(false);
    }

    public void LoginUser(int userId)
    {
        Debug.Log($"Logging in for player {userId}.");
        loggedIn = true;
        GameState.Instance.PlayerID = userId;
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

        ClearAllPrefabs();

        LoginScreen.SetActive(!loggedIn);
        PlayerGUICanvas.SetActive(loggedIn);
        Player.SetActive(loggedIn);
        VuforiaBehaviour.Instance.enabled = loggedIn;
        Debug.Log($"Player {GameState.Instance.PlayerID} logged out.");
    }

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
