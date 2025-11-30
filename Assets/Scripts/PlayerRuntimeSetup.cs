using UnityEngine;

[RequireComponent(typeof(FirstPersonController))]
public class PlayerRuntimeSetup : MonoBehaviour
{
    [Header("Optional references")]
    public InteractionSystem interactionSystem;
    public DevionGamesInventoryWrapper inventoryWrapper;

    private void Awake()
    {
        if (interactionSystem == null)
        {
            interactionSystem = GetComponent<InteractionSystem>();
            if (interactionSystem == null)
            {
                interactionSystem = gameObject.AddComponent<InteractionSystem>();
            }
        }

        // DevionGamesInventoryWrapper should be added to a persistent GameObject (like a GameManager)
        // It doesn't need to be on the player
        if (inventoryWrapper == null)
        {
            inventoryWrapper = FindFirstObjectByType<DevionGamesInventoryWrapper>();
            if (inventoryWrapper == null)
            {
                // Create a GameManager object for the inventory wrapper
                GameObject gameManager = new GameObject("GameManager");
                inventoryWrapper = gameManager.AddComponent<DevionGamesInventoryWrapper>();
                DontDestroyOnLoad(gameManager);
            }
        }
    }
}

