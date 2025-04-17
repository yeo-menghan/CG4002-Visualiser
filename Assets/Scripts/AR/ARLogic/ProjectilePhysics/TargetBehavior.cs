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
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length];

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

        if (flashOnHit)
        {
            FlashEffect();
        }
    }

    void FlashEffect()
    {
        foreach (Renderer rend in renderers)
        {
            if (rend.material != null)
            {
                rend.material.color = hitColor;
            }
        }

        Invoke(nameof(ResetColors), flashDuration);
    }

    void ResetColors()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i].material != null && i < originalColors.Length)
            {
                renderers[i].material.color = originalColors[i];
            }
        }
    }
}
