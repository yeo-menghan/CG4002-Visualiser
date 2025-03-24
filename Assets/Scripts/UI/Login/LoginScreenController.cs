using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoginScreenController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown playerSelectionDropdown;
    [SerializeField] private Button loginButton;

    private UserLogManager userLogManager;

    private void Start()
    {
        // Get reference to UserLogManager
        userLogManager = UserLogManager.Instance;

        // Find components if not assigned in inspector
        if (playerSelectionDropdown == null)
            playerSelectionDropdown = transform.Find("Panel/PlayerSelectionDropdown").GetComponent<TMP_Dropdown>();

        if (loginButton == null)
            loginButton = transform.Find("LoginButton").GetComponent<Button>();

        // Ensure dropdown has Player 1 and Player 2 options
        if (playerSelectionDropdown.options.Count == 0)
        {
            playerSelectionDropdown.ClearOptions();
            playerSelectionDropdown.AddOptions(new System.Collections.Generic.List<string> { "Player 1", "Player 2" });
        }

        // Add listener to login button
        loginButton.onClick.AddListener(LoginSelectedPlayer);
    }

    public void LoginSelectedPlayer()
    {
        // Get selected player ID (adding 1 because dropdown is zero-indexed)
        int playerId = playerSelectionDropdown.value + 1;

        // Login the selected player
        userLogManager.LoginUser(playerId);
    }
}
