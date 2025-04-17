using UnityEngine;
using UnityEngine.UI; // Import the UI namespace for Image and Sprite

public class DeviceStatusManager : MonoBehaviour
{
    [Header("UI Image References")]
    // Change the type from TextMeshProUGUI to Image
    [SerializeField] private Image p1GloveStatusImage;
    [SerializeField] private Image p1VestStatusImage;
    [SerializeField] private Image p1GunStatusImage;
    [SerializeField] private Image p2GloveStatusImage;
    [SerializeField] private Image p2VestStatusImage;
    [SerializeField] private Image p2GunStatusImage;

    [Header("Status Display Sprites & Options")]
    [SerializeField] private Sprite connectedSprite;    // Assign your Tick Sprite here in the Inspector
    [SerializeField] private Sprite disconnectedSprite; // Assign your Cross Sprite here in the Inspector
    [SerializeField] private Color connectedColor = Color.green; // Optional: Tint for the connected sprite
    [SerializeField] private Color disconnectedColor = Color.red;   // Optional: Tint for the disconnected sprite
    // You can set these colors to Color.white if your sprites already have the desired colors

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
            gameState.OnDeviceStatusChanged += UpdateDeviceStatusImages; // Renamed handler
            Debug.Log("DeviceStatusManager subscribed to OnDeviceStatusChanged.");
            UpdateDeviceStatusImages(); // Update immediately on enable
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
            gameState.OnDeviceStatusChanged -= UpdateDeviceStatusImages; // Renamed handler
            Debug.Log("DeviceStatusManager unsubscribed from OnDeviceStatusChanged.");
        }
    }

    // Method called by the event or on enable
    private void UpdateDeviceStatusImages() // Renamed method
    {
        Debug.Log("DeviceStatusManager: Updating device status images."); // Updated log message
        if (gameState == null)
        {
            Debug.LogError("GameState instance not found. Cannot update UI.");
            return;
        }

        if (connectedSprite == null || disconnectedSprite == null)
        {
            Debug.LogError("Connected or Disconnected Sprite is not assigned in the DeviceStatusManager Inspector!");
            return; // Can't update without sprites
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

    // Helper method to update a single Image element
    // Change the parameter type from TextMeshProUGUI to Image
    private void UpdateSingleImage(Image imageElement, bool isConnected)
    {
        if (imageElement != null)
        {
            // Assign the correct sprite based on connection status
            imageElement.sprite = isConnected ? connectedSprite : disconnectedSprite;

            // Assign the corresponding color tint
            // imageElement.color = isConnected ? connectedColor : disconnectedColor;

            // Ensure the image component is enabled (it might be disabled if sprites were missing previously)
            imageElement.enabled = true;
        }
        else
        {
            // Log a warning if a specific Image component is missing
            // (Use a more specific message if possible, e.g., by passing the name)
            Debug.LogWarning($"UI Image element is not assigned in the inspector slot for DeviceStatusManager.");
        }
    }
}
