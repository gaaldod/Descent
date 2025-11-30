using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    public string interactionText = "interact";
    public bool canInteract = true;
    
    public virtual bool CanInteract()
    {
        return canInteract;
    }
    
    public virtual string GetInteractionText()
    {
        return interactionText;
    }
    
    public abstract void Interact();

    public virtual void OnFocus()
    {
    }

    public virtual void OnFocusExit()
    {
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
