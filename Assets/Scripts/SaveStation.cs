using UnityEngine;

public class SaveStation : Interactable
{
    [Header("Visuals")]
    public SimpleOutline outlineEffect;
    public GlowLight glowLight;

    [Header("UI")]
    public SaveMenuUI saveMenu;

    private void Awake()
    {
        interactionText = "save game";
    }

    private void Start()
    {
        glowLight?.SetActive(true);
    }

    public override void Interact()
    {
        if (saveMenu == null)
        {
            Debug.LogWarning("Save menu not assigned to SaveStation.");
            return;
        }

        saveMenu.Open();
    }

    public override void OnFocus()
    {
        outlineEffect?.SetHighlighted(true);
        glowLight?.SetActive(true);
    }

    public override void OnFocusExit()
    {
        outlineEffect?.SetHighlighted(false);
        glowLight?.SetActive(false);
    }
}

