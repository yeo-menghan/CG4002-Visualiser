using UnityEngine;
using Vuforia;
using System.Collections.Generic;

public class TargetSwitchManager : MonoBehaviour
{
    public GameObject VuforiaEnemyTracker;

    public static TargetSwitchManager Instance { get; private set; }

    string dataSetPath = "Vuforia/Players.xml";

    void Start()
    {
        // Singleton TargetSwitchManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        VuforiaApplication.Instance.OnVuforiaInitialized += OnVuforiaInitialized;
    }

    void OnVuforiaInitialized(VuforiaInitError error)
    {
        if(error == VuforiaInitError.NONE)
        {
          TargetSwitch();
        }
    }

    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        System.Type type = original.GetType();
        var dst = destination.AddComponent(type);
        var fields = type.GetFields(System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);
        foreach (var field in fields)
        {
            field.SetValue(dst, field.GetValue(original));
        }
        return dst as T;
    }

    public async void TargetSwitch()
    {
        string playerCreate = GameState.Instance.PlayerID == 1 ? "player_2" : "player_1";
        var imageTargetBehaviour = VuforiaEnemyTracker.GetComponent<ImageTargetBehaviour>();

        if (imageTargetBehaviour == null)
        {
            Debug.LogError("ImageTargetBehaviour not found");
            return;
        }
        else if (imageTargetBehaviour.TargetName == playerCreate)
        {
            Debug.Log("Already tracking the selected player");
            return;
        }
        else
        {
            // Store the components you need to copy
            var componentsToCopy = VuforiaEnemyTracker.GetComponents<Component>();

            // Destroy the existing ImageTargetBehaviour
            Destroy(imageTargetBehaviour);

            // Create the new ImageTargetBehaviour
            var newBehaviour = await VuforiaBehaviour.Instance.ObserverFactory.CreateImageTargetAsync(
                dataSetPath, playerCreate);

            if (newBehaviour == null)
            {
                Debug.LogError("Failed to create new ImageTargetBehaviour");
                return;
            }

            // Set the new GameObject's transform to match the old one
            newBehaviour.transform.position = VuforiaEnemyTracker.transform.position;
            newBehaviour.transform.rotation = VuforiaEnemyTracker.transform.rotation;
            newBehaviour.transform.localScale = VuforiaEnemyTracker.transform.localScale;

            // Copy necessary components to the new GameObject
            foreach (var component in componentsToCopy)
            {
                if (component is Transform || component is ImageTargetBehaviour)
                    continue; // Skip Transform and ImageTargetBehaviour

                CopyComponent(component, newBehaviour.gameObject);
            }

            // Transfer child objects
            List<Transform> children = new List<Transform>();
            foreach (Transform child in VuforiaEnemyTracker.transform)
            {
                children.Add(child);
            }
            foreach (Transform child in children)
            {
                child.SetParent(newBehaviour.transform);
            }

            // Destroy the old GameObject
            Destroy(VuforiaEnemyTracker);

            // Update the reference to point to the new GameObject
            VuforiaEnemyTracker = newBehaviour.gameObject;
        }
    }

}
