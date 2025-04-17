using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class CardboardManager : MonoBehaviour
{
    public Button launchButton;
    public CanvasGroup debugCanvasGroup;
    private Google.XR.Cardboard.XRLoader cardboardLoader;
    private DebuggerToggle debuggerToggle;
    public Canvas[] uiCanvases;

    void Start()
    {
        cardboardLoader = ScriptableObject.CreateInstance<Google.XR.Cardboard.XRLoader>();

        if (debugCanvasGroup != null)
        {
            debuggerToggle = debugCanvasGroup.GetComponent<DebuggerToggle>();

            if (debuggerToggle == null)
            {
                debuggerToggle = FindObjectOfType<DebuggerToggle>();
            }
        }
    }

    public void LaunchGoogleCardboard()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            cardboardLoader.Initialize();
            cardboardLoader.Start();

            if (debugCanvasGroup != null)
            {
                debugCanvasGroup.alpha = 0;
                debugCanvasGroup.interactable = false;
                debugCanvasGroup.blocksRaycasts = false;

                if (debuggerToggle != null && debuggerToggle.TargetCanvasGroup == debugCanvasGroup)
                {
                    debuggerToggle.ForceHide();
                }
            }

            foreach (Canvas canvas in uiCanvases)
            {
                canvas.renderMode = RenderMode.WorldSpace;

                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraRight = Camera.main.transform.right;
                Vector3 cameraUp = Camera.main.transform.up;

                cameraForward.Normalize();
                canvas.transform.position = Camera.main.transform.position + cameraForward * 1f;
                canvas.transform.rotation = Camera.main.transform.rotation;
                canvas.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
            }
        }
    }

    void Update()
    {
        if (Google.XR.Cardboard.Api.IsCloseButtonPressed)
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                cardboardLoader.Stop();
                cardboardLoader.Deinitialize();

                if (debugCanvasGroup != null)
                {
                    debugCanvasGroup.alpha = 1;
                    debugCanvasGroup.interactable = true;
                    debugCanvasGroup.blocksRaycasts = true;

                    if (debuggerToggle != null && debuggerToggle.TargetCanvasGroup == debugCanvasGroup)
                    {
                        debuggerToggle.ForceShow();
                    }
                }

                foreach (Canvas canvas in uiCanvases)
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.transform.localScale = Vector3.one;
                }
            }
        }
    }
}
