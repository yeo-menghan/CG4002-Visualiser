using UnityEngine;

public class ProjectileCollision : MonoBehaviour
{
    [Header("Target Parameters")]
    public string targetTag = "Target";

    private bool isScheduledForDestruction = false;

    void Start()
    {
        // Schedule destruction after 2 seconds
        Invoke(nameof(DestroyProjectile), 2f);
        Debug.Log("ProjectileCollision: Projectile will self-destruct in 2 seconds if no collisions occur.");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isScheduledForDestruction) return;

        Debug.Log($"ProjectileCollision: Collision detected with: {collision.gameObject.name}");
        Debug.Log($"ProjectileCollision: My tag is looking for: {targetTag}");
        Debug.Log($"ProjectileCollision: Checking tag match: {collision.gameObject.CompareTag(targetTag)}");

        // Debug all collider pairs involved in this collision
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.Log($"ProjectileCollision: Contact between: {contact.thisCollider.name} and {contact.otherCollider.name}");
        }

        if (collision.gameObject.CompareTag(targetTag))
        {
            Debug.Log($"ProjectileCollision: Target hit: {targetTag}. Destroying projectile.");
            DestroyProjectile();
        }
    }

    void DestroyProjectile()
    {
        if (isScheduledForDestruction) return;

        isScheduledForDestruction = true;
        CancelInvoke();
        Destroy(gameObject);
        Debug.Log("ProjectileCollision: Projectile destroyed.");
    }

    void OnDestroy()
    {
        isScheduledForDestruction = true;
        CancelInvoke();
        Debug.Log("ProjectileCollision: OnDestroy triggered.");
    }

    void OnDisable()
    {
        isScheduledForDestruction = true;
        CancelInvoke();
        Debug.Log("ProjectileCollision: OnDisable triggered.");
    }
}
