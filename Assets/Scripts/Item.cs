using UnityEngine;
using DevionGames.InventorySystem;

public class Item : Interactable
{
    [Header("Item Settings")]
    public string itemName;
    public string itemDescription;
    public bool isKey = false;
    public bool isConsumable = false;
    
    [Header("Note Settings")]
    [Tooltip("If this is a note, the text to display when picked up")]
    [TextArea(4, 10)]
    public string noteText;
    [Tooltip("Check this if this item is a note that should display text when picked up")]
    public bool isNote = false;
    [Tooltip("Seconds before the note UI hides automatically. Set to zero to keep it open.")]
    public float noteAutoHideDelay = 8f;
    
    public override void Interact()
    {
        // If this is a note, display the text first
        if (isNote && !string.IsNullOrEmpty(noteText))
        {
            NoteDisplayUI.Instance.Show(noteText);
            
            if (noteAutoHideDelay > 0f)
            {
                StartCoroutine(HideNoteAfterDelay());
            }
        }
        
        if (DevionGamesInventoryWrapper.AddItem(itemName))
        {
            // Play pickup sound
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
            // Hide visuals instead of destroying (for potential respawn)
            HideVisuals();
            
            if (!isNote)
            {
                InteractionSystem.Instance?.ShowNotification("Picked up " + itemName + ".");
            }
        }
        else
        {
            InteractionSystem.Instance?.ShowNotification("Inventory is full!");
        }
    }
    
    private System.Collections.IEnumerator HideNoteAfterDelay()
    {
        yield return new WaitForSeconds(noteAutoHideDelay);
        NoteDisplayUI.Instance.Hide();
    }
    
    private void HideVisuals()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        foreach (var collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        canInteract = false;
    }
    
    public override string GetInteractionText()
    {
        return "pick up " + itemName;
    }
}
