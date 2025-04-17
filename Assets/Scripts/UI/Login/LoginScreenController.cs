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
        userLogManager = UserLogManager.Instance;

        if (playerSelectionDropdown == null)
            playerSelectionDropdown = transform.Find("Panel/PlayerSelectionDropdown").GetComponent<TMP_Dropdown>();

        if (loginButton == null)
            loginButton = transform.Find("LoginButton").GetComponent<Button>();

        if (playerSelectionDropdown.options.Count == 0)
        {
            playerSelectionDropdown.ClearOptions();
            playerSelectionDropdown.AddOptions(new System.Collections.Generic.List<string> { "Player 1", "Player 2" });
        }

        loginButton.onClick.AddListener(LoginSelectedPlayer);
    }

    public void LoginSelectedPlayer()
    {
        int playerId = playerSelectionDropdown.value + 1;

        userLogManager.LoginUser(playerId);
    }
}
