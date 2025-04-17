using UnityEngine;
using UnityEngine.UI;

public class DeviceStatusManager : MonoBehaviour
{
    [Header("UI Image References")]
    [SerializeField] private Image p1GloveStatusImage;
    [SerializeField] private Image p1VestStatusImage;
    [SerializeField] private Image p1GunStatusImage;
    [SerializeField] private Image p2GloveStatusImage;
    [SerializeField] private Image p2VestStatusImage;
    [SerializeField] private Image p2GunStatusImage;

    [Header("Status Display Sprites & Options")]
    [SerializeField] private Sprite connectedSprite;
    [SerializeField] private Sprite disconnectedSprite;

    private GameState gameState;

    void Awake()
    {
        gameState = GameState.Instance;

        if (gameState == null)
        {
            Debug.LogError("GameState instance not found!");
        }
    }

    private void OnEnable()
    {
        if (gameState != null)
        {
            gameState.OnDeviceStatusChanged += UpdateDeviceStatusImages;
            Debug.Log("DeviceStatusManager subscribed to OnDeviceStatusChanged.");
            UpdateDeviceStatusImages();
        }
        else
        {
            Debug.LogWarning("GameState instance not found on Enable. DeviceStatusManager cannot subscribe yet.");
        }
    }

    private void OnDisable()
    {
        if (gameState != null)
        {
            gameState.OnDeviceStatusChanged -= UpdateDeviceStatusImages;
            Debug.Log("DeviceStatusManager unsubscribed from OnDeviceStatusChanged.");
        }
    }

    private void UpdateDeviceStatusImages()
    {
        Debug.Log("DeviceStatusManager: Updating device status images.");
        if (gameState == null)
        {
            Debug.LogError("GameState instance not found. Cannot update UI.");
            return;
        }

        if (connectedSprite == null || disconnectedSprite == null)
        {
            Debug.LogError("Connected or Disconnected Sprite is not assigned in the DeviceStatusManager Inspector!");
            return;
        }

        // Update Player 1 Images
        UpdateSingleImage(p1GunStatusImage, gameState.P1_gun);
        UpdateSingleImage(p1VestStatusImage, gameState.P1_vest);
        UpdateSingleImage(p1GloveStatusImage, gameState.P1_glove);

        // Update Player 2 Images
        UpdateSingleImage(p2GunStatusImage, gameState.P2_gun);
        UpdateSingleImage(p2VestStatusImage, gameState.P2_vest);
        UpdateSingleImage(p2GloveStatusImage, gameState.P2_glove);
    }

    private void UpdateSingleImage(Image imageElement, bool isConnected)
    {
        if (imageElement != null)
        {
            imageElement.sprite = isConnected ? connectedSprite : disconnectedSprite;
            imageElement.enabled = true;
        }
        else
        {
            Debug.LogWarning($"UI Image element is not assigned in the inspector slot for DeviceStatusManager.");
        }
    }
}
