using UnityEngine;
using System.Collections;

public class LogoutManager : MonoBehaviour
{
    private GameState gameState;
    private UserLogManager userLogManager;

    void Start()
    {
        gameState = GameState.Instance;
        userLogManager = UserLogManager.Instance;
        gameState.gameActionOccurred.AddListener(OnGameActionOccurred);
    }

    private void OnDestroy()
    {
        if (gameState != null)
        {
            gameState.gameActionOccurred.RemoveListener(OnGameActionOccurred);
        }
    }

    private void OnGameActionOccurred(string actionType)
    {
        if (actionType == "logout")
        {
            StartCoroutine(DelayedLogout());
        }
    }

    private IEnumerator DelayedLogout()
    {
        yield return new WaitForSeconds(6f);

        // Execute logout
        // userLogManager.LogoutUser();
    }
}
