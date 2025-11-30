using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Trigger that displays a popup message, pauses the game, and disappears after being read.
/// </summary>
public class PopupTrigger : MonoBehaviour
{
    [Header("Popup Settings")]
    [TextArea(3, 8)]
    [Tooltip("The message to display in the popup")]
    public string popupMessage = "Enter your message here...";
    
    [Header("UI Settings")]
    [Tooltip("Background color of the popup")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.95f);
    [Tooltip("Text color")]
    public Color textColor = Color.white;
    [Tooltip("Font size")]
    public int fontSize = 24;
    
    [Header("Behavior")]
    [Tooltip("Disable this trigger after it's been read")]
    public bool disableAfterRead = true;
    
    private bool hasBeenTriggered = false;
    private static PopupUI popupUI;
    
    private void Awake()
    {
        // Ensure this is a trigger collider
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning($"[PopupTrigger] {gameObject.name} needs a Collider component set as trigger!");
        }
        
        // Make sure the collider is invisible (disable renderer if present)
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenTriggered) return;
        
        // Check if it's the player
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            ShowPopup();
        }
    }
    
    private void ShowPopup()
    {
        hasBeenTriggered = true;
        
        // Get or create the popup UI
        if (popupUI == null)
        {
            popupUI = PopupUI.Instance;
        }
        
        // Show the popup with custom colors
        popupUI.Show(popupMessage, backgroundColor, textColor, fontSize);
        
        // Disable this trigger after being read
        if (disableAfterRead)
        {
            StartCoroutine(DisableAfterClose());
        }
    }
    
    private IEnumerator DisableAfterClose()
    {
        // Wait until popup is closed
        while (popupUI != null && popupUI.IsShowing)
        {
            yield return null;
        }
        
        // Disable the collider and this component
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        this.enabled = false;
    }
}

