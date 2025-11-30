using UnityEngine;

public class HidingSpot : Interactable
{
    [Header("Hiding Settings")]
    public Transform hidePosition;
    public Transform exitPosition;
    public float hideTime = 3f;
    [Header("Visibility")]
    public Renderer clothRenderer;
    [Range(0f, 1f)] public float hiddenAlpha = 0.4f;
    
    private bool isPlayerHiding = false;
    private GameObject player;
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;
    private InteractionSystem interactionSystem;
    private float exitEnabledTime;

    private void Awake()
    {
        interactionSystem = InteractionSystem.Instance ?? FindFirstObjectByType<InteractionSystem>();
    }
    
    public override void Interact()
    {
        if (!isPlayerHiding)
        {
            StartHiding();
        }
        else
        {
            StopHiding();
        }
    }

    private void Update()
    {
        if (isPlayerHiding && Time.time >= exitEnabledTime && Input.GetKeyDown(KeyCode.E))
        {
            StopHiding();
        }
    }
    
    void StartHiding()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning($"HidingSpot '{name}': Player not found.");
            return;
        }
        
        // Store original position and rotation
        originalPlayerPosition = player.transform.position;
        originalPlayerRotation = player.transform.rotation;
        
        if (hidePosition == null)
        {
            Debug.LogWarning($"HidingSpot '{name}': Hide position not assigned.");
            return;
        }

        Debug.Log($"[{name}] Moving player to hide position {hidePosition.position}");
        player.transform.position = hidePosition.position;
        player.transform.rotation = hidePosition.rotation;
        Debug.Log($"[{name}] Player now at {player.transform.position}");
        
        // Disable player controller
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
        
        isPlayerHiding = true;
        interactionText = "exit hiding spot";
        exitEnabledTime = Time.time + 0.2f;

        SetClothAlpha(hiddenAlpha);
        interactionSystem?.ShowNotification("Press E to exit hiding spot");
        
        // Auto-exit after hideTime
        Invoke("StopHiding", hideTime);
    }
    
    void StopHiding()
    {
        if (!isPlayerHiding) return;
        
        // Restore player position and rotation
        if (exitPosition != null)
        {
            player.transform.position = exitPosition.position;
            player.transform.rotation = exitPosition.rotation;
        }
        else
        {
            player.transform.position = originalPlayerPosition;
            player.transform.rotation = originalPlayerRotation;
        }
        
        // Re-enable player controller
        FirstPersonController controller = player.GetComponent<FirstPersonController>();
        if (controller != null)
        {
            controller.enabled = true;
        }
        
        isPlayerHiding = false;
        interactionText = "hide";

        SetClothAlpha(1f);
        Debug.Log($"[{name}] Exiting hiding spot. Using {(exitPosition != null ? "exit position" : "original position")}.");
        
        CancelInvoke("StopHiding");
    }
    
    public bool IsPlayerHiding()
    {
        return isPlayerHiding;
    }

    private void SetClothAlpha(float alpha)
    {
        if (clothRenderer == null) return;

        foreach (var mat in clothRenderer.materials)
        {
            if (mat.HasProperty("_Color"))
            {
                Color c = mat.color;
                c.a = alpha;
                mat.color = c;
                if (alpha < 1f)
                {
                    mat.SetInt("_Surface", 1);
                    mat.SetInt("_ZWrite", 0);
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                }
                else
                {
                    mat.SetInt("_Surface", 0);
                    mat.SetInt("_ZWrite", 1);
                    mat.renderQueue = -1;
                }
            }
        }
    }
}
