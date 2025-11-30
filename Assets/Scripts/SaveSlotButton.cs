using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SaveSlotButton : MonoBehaviour
{
    public Text label;

    private string slotName;
    private SaveMenuUI menu;
    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    public void Setup(SaveMenuUI menu, string slotName, string displayText)
    {
        this.menu = menu;
        this.slotName = slotName;

        if (label != null)
        {
            label.text = displayText;
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void HandleClick()
    {
        if (menu != null && !string.IsNullOrEmpty(slotName))
        {
            menu.SelectSlot(slotName);
        }
    }
}

