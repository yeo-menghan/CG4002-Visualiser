using UnityEngine;

public class CollisionDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"CollisionDebugger: CollisionDebugger started on {gameObject.name}, tag: {gameObject.tag}");
    }

    private void Update()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        Debug.Log($"Found {colliders.Length} colliders in the hierarchy of {gameObject.name}");
        foreach (Collider col in colliders)
        {
            Debug.Log($"CollisionDebugger: Collider: {col.gameObject.name}, isTrigger: {col.isTrigger}, enabled: {col.enabled}");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"CollisionDebugger: TARGET COLLISION: {gameObject.name} collided with {collision.gameObject.name}");
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"CollisionDebugger: TARGET TRIGGER: {gameObject.name} triggered by {other.gameObject.name}");
    }
}
