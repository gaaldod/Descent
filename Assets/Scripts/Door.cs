using UnityEngine;

public class Door : Interactable
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    public bool requiresKey = false;
    public string requiredKeyName = "";
    
    [Header("On Open Actions")]
    [Tooltip("Enemy AI to enable when this door is opened")]
    public EnemyAI enemyAIToEnable;
    
    private bool isOpen = false;
    private bool isOpening = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    
    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, openAngle);
    }
    
    public override void Interact()
    {
        // Don't allow interaction if door is already open or opening
        if (isOpening || isOpen) return;
        
        if (requiresKey)
        {
            // Check if player has the required key
            if (DevionGamesInventoryWrapper.HasItem(requiredKeyName))
            {
                StartCoroutine(OpenDoor());
            }
            else
            {
                Debug.Log("You need a key to open this door.");
            }
        }
        else
        {
            StartCoroutine(OpenDoor());
        }
    }
    
    private System.Collections.IEnumerator OpenDoor()
    {
        isOpening = true;
        Quaternion targetRotation = openRotation;
        
        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }
        
        transform.localRotation = targetRotation;
        isOpen = true;
        isOpening = false;
        
        // Enable enemy AI if specified
        if (enemyAIToEnable != null)
        {
            enemyAIToEnable.enabled = true;
            Debug.Log($"[Door] Enemy AI enabled after door opened: {gameObject.name}");
        }
        
        // Disable interaction after door is opened
        canInteract = false;
        
        // Disable mesh colliders that prompt interaction
        DisableInteractionColliders();
    }
    
    private void DisableInteractionColliders()
    {
        // Disable MeshCollider on this GameObject
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        if (meshCollider != null)
        {
            meshCollider.enabled = false;
        }
        
        // Disable MeshColliders on child objects
        MeshCollider[] childMeshColliders = GetComponentsInChildren<MeshCollider>();
        foreach (MeshCollider collider in childMeshColliders)
        {
            collider.enabled = false;
        }
        
        // Also disable BoxColliders and other colliders that might be used for interaction
        // (but keep any colliders that are needed for physics/walls)
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in allColliders)
        {
            // Only disable trigger colliders (typically used for interaction)
            if (collider.isTrigger)
            {
                collider.enabled = false;
            }
        }
    }
}
