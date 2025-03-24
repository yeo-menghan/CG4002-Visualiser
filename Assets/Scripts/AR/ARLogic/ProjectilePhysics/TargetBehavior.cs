using UnityEngine;

public class TargetBehavior : MonoBehaviour
{
    [Header("Hit Effects")]
    public bool flashOnHit = true;
    public float flashDuration = 0.3f;
    public Color hitColor = Color.red;

    private Renderer[] renderers;
    private Color[] originalColors;

    private GameState gameState;

    void Start()
    {
        gameState = GameState.Instance;
        // Cache renderers for quick access
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

        // Store original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null)
            {
                originalColors[i] = renderers[i].material.color;
            }
        }
    }

    public void OnHit()
    {
        Debug.Log($"TargetBehavior: Target '{gameObject.name}' was hit!");

        // Visual feedback (flash effect)
        if (flashOnHit)
        {
            FlashEffect();
        }

        // Add any other effects like animations, sound, etc.

        // Notify GameManager or other systems

        if (gameState != null)
        {
            // Optionally update game state
            gameState.SendMessage("TargetBehavior: OnEnemyHit", SendMessageOptions.DontRequireReceiver);
        }
    }

    void FlashEffect()
    {
        // Change all renderer colors to hit color
        foreach (Renderer rend in renderers)
        {
            if (rend.material != null)
            {
                rend.material.color = hitColor;
            }
        }

        // Reset colors after flash duration
        Invoke(nameof(ResetColors), flashDuration);
    }

    void ResetColors()
    {
        // Restore original colors
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
