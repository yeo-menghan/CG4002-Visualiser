using UnityEngine;
using UnityEngine.UI;
using Google.XR.Cardboard;

public class CardboardManager : MonoBehaviour
{
    public Button launchButton;
    public CanvasGroup debugCanvasGroup; // Add reference to the canvas group
    private Google.XR.Cardboard.XRLoader cardboardLoader;
    private DebuggerToggle debuggerToggle; // Reference to your DebuggerToggle script
    public Canvas[] uiCanvases; // Assign your UI canvases in the inspector

    void Start()
    {
        cardboardLoader = ScriptableObject.CreateInstance<Google.XR.Cardboard.XRLoader>();

        // Try to find the DebuggerToggle script if debugCanvasGroup is assigned
        if (debugCanvasGroup != null)
        {
            debuggerToggle = debugCanvasGroup.GetComponent<DebuggerToggle>();

            // If there's no DebuggerToggle on the CanvasGroup, look elsewhere
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

            // Hide the canvas group
            if (debugCanvasGroup != null)
            {
                debugCanvasGroup.alpha = 0;
                debugCanvasGroup.interactable = false;
                debugCanvasGroup.blocksRaycasts = false;

                // If we have access to the DebuggerToggle, update its state too
                if (debuggerToggle != null && debuggerToggle.TargetCanvasGroup == debugCanvasGroup)
                {
                    // This keeps the internal state consistent
                    debuggerToggle.ForceHide();
                }
            }

            // Set canvases to render in world space for VR
            foreach (Canvas canvas in uiCanvases)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                // Position directly in front of camera without any tilt
                Vector3 cameraForward = Camera.main.transform.forward;
                Vector3 cameraRight = Camera.main.transform.right;
                Vector3 cameraUp = Camera.main.transform.up;

                // Ensure we're using normalized vectors
                cameraForward.Normalize();

                // Position the canvas exactly in front of the camera
                canvas.transform.position = Camera.main.transform.position + cameraForward * 2f;

                // Make sure canvas is perfectly aligned with the camera's forward direction
                canvas.transform.rotation = Camera.main.transform.rotation;
                // canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - Camera.main.transform.position);
                canvas.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f); // Adjust scale as needed
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

                // Show the canvas group again
                if (debugCanvasGroup != null)
                {
                    debugCanvasGroup.alpha = 1;
                    debugCanvasGroup.interactable = true;
                    debugCanvasGroup.blocksRaycasts = true;

                    // If we have access to the DebuggerToggle, update its state too
                    if (debuggerToggle != null && debuggerToggle.TargetCanvasGroup == debugCanvasGroup)
                    {
                        // This keeps the internal state consistent
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
