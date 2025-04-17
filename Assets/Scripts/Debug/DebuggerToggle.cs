using UnityEngine;

public class DebuggerToggle : MonoBehaviour
{
    public CanvasGroup TargetCanvasGroup;

    private bool isVisible = true;

    public void ToggleDebugger()
    {
        if (TargetCanvasGroup == null)
        {
            Debug.LogWarning("No CanvasGroup assigned to DebuggerToggle!");
            return;
        }

        isVisible = !isVisible;
        UpdateDebuggerState();
    }

    public void ForceHide()
    {
        isVisible = false;
        UpdateDebuggerState();
    }

    public void ForceShow()
    {
        isVisible = true;
        UpdateDebuggerState();
    }

    private void UpdateDebuggerState()
    {
        if (TargetCanvasGroup == null) return;

        TargetCanvasGroup.alpha = isVisible ? 1 : 0;
        TargetCanvasGroup.interactable = isVisible;
        TargetCanvasGroup.blocksRaycasts = isVisible;
    }
}
