using UnityEngine;

public class DebuggerToggle : MonoBehaviour
{
    // Reference to the CanvasGroup to control
    public CanvasGroup TargetCanvasGroup;

    // State to track visibility
    private bool isVisible = true;

    // Method to toggle the CanvasGroup visibility
    public void ToggleDebugger()
    {
        // Check if a CanvasGroup is assigned
        if (TargetCanvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup assigned to DebuggerToggle!");
            return;
        }

        isVisible = !isVisible;
        UpdateDebuggerState();
    }

    // New method to force hide
    public void ForceHide()
    {
        isVisible = false;
        UpdateDebuggerState();
    }

    // New method to force show
    public void ForceShow()
    {
        isVisible = true;
        UpdateDebuggerState();
    }

    // Method to update the CanvasGroup state
    private void UpdateDebuggerState()
    {
        if (TargetCanvasGroup == null) return;

        TargetCanvasGroup.alpha = isVisible ? 1 : 0;
        TargetCanvasGroup.interactable = isVisible;
        TargetCanvasGroup.blocksRaycasts = isVisible;
    }
}
