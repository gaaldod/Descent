using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenuUI : MonoBehaviour
{
    public GameObject menuRoot;
    public Transform slotContainer;
    public GameObject slotButtonPrefab;
    public Button overwriteButton;
    public Button newSaveButton;
    public Button cancelButton;

    private string selectedSlot;
    private List<GameObject> spawnedSlots = new List<GameObject>();
    private FirstPersonController playerController;

    public bool IsOpen => menuRoot != null && menuRoot.activeSelf;

    private void Start()
    {
        BuildDefaultMenuIfNeeded();

        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        if (overwriteButton != null)
        {
            overwriteButton.onClick.AddListener(OverwriteSelectedSlot);
        }

        if (newSaveButton != null)
        {
            newSaveButton.onClick.AddListener(CreateNewSave);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(Close);
        }
    }

    public void Open()
    {
        if (menuRoot == null)
        {
            Debug.LogWarning("SaveMenuUI has no menu root assigned.");
            return;
        }

        if (IsOpen) return;

        menuRoot.SetActive(true);
        PopulateSlots();
        LockPlayer(true);
    }

    public void Close()
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(false);
        }

        LockPlayer(false);
    }

    public void SelectSlot(string slotName)
    {
        selectedSlot = slotName;
    }

    private void PopulateSlots()
    {
        foreach (GameObject go in spawnedSlots)
        {
            Destroy(go);
        }
        spawnedSlots.Clear();

        List<SaveSlotInfo> slots = SaveManager.Instance != null ? SaveManager.Instance.GetSaveSlots() : new List<SaveSlotInfo>();

        if (slots.Count == 0 && slotButtonPrefab != null)
        {
            InstantiateSlotButton(slotContainer, "No save file found", null);
            overwriteButton.interactable = false;
            return;
        }

        foreach (SaveSlotInfo info in slots)
        {
            InstantiateSlotButton(slotContainer, $"{info.slotName} ({info.lastSaved})", info.slotName);
        }

        overwriteButton.interactable = true;
    }

    private void OverwriteSelectedSlot()
    {
        if (string.IsNullOrEmpty(selectedSlot))
        {
            InteractionSystem.Instance?.ShowNotification("Select a save slot first.");
            return;
        }

        SaveManager.Instance?.SaveToSlot(selectedSlot);
        Close();
    }

    private void CreateNewSave()
    {
        SaveManager.Instance?.SaveToSlot(null);
        Close();
    }

    private void LockPlayer(bool locked)
    {
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<FirstPersonController>();
        }

        if (playerController != null)
        {
            playerController.playerCanMove = !locked;
            playerController.cameraCanMove = !locked;
        }

        Cursor.visible = locked;
        Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void BuildDefaultMenuIfNeeded()
    {
        if (menuRoot != null && slotContainer != null && slotButtonPrefab != null &&
            overwriteButton != null && newSaveButton != null && cancelButton != null)
        {
            return;
        }

        GameObject canvasObj = new GameObject("GeneratedSaveMenu");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();
        EnsureEventSystemExists();

        menuRoot = new GameObject("SaveMenuPanel");
        menuRoot.transform.SetParent(canvas.transform, false);
        var image = menuRoot.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.8f);
        var rect = menuRoot.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.25f, 0.2f);
        rect.anchorMax = new Vector2(0.75f, 0.8f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject title = new GameObject("Title");
        title.transform.SetParent(menuRoot.transform, false);
        var titleText = title.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 15;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "Save Game";
        var titleRect = titleText.rectTransform;
        titleRect.anchorMin = new Vector2(0.1f, 0.8f);
        titleRect.anchorMax = new Vector2(0.9f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        GameObject slotsGO = new GameObject("SlotContainer");
        slotsGO.transform.SetParent(menuRoot.transform, false);
        slotContainer = slotsGO.transform;
        var slotsRect = slotsGO.AddComponent<RectTransform>();
        slotsRect.anchorMin = new Vector2(0.1f, 0.3f);
        slotsRect.anchorMax = new Vector2(0.9f, 0.75f);
        slotsRect.offsetMin = Vector2.zero;
        slotsRect.offsetMax = Vector2.zero;

        var layout = slotsGO.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8f;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        slotButtonPrefab = CreateSlotButtonPrefab();

        overwriteButton = CreateMenuButton("Overwrite", new Vector2(0.08f, 0.15f), new Vector2(0.32f, 0.25f));
        newSaveButton = CreateMenuButton("New Save", new Vector2(0.36f, 0.15f), new Vector2(0.64f, 0.25f));
        cancelButton = CreateMenuButton("Cancel", new Vector2(0.68f, 0.15f), new Vector2(0.92f, 0.25f));

        DontDestroyOnLoad(canvasObj);
        Canvas.ForceUpdateCanvases();
    }

    private GameObject CreateSlotButtonPrefab()
    {
        GameObject prefab = new GameObject("SlotButtonPrefab");
        prefab.transform.SetParent(menuRoot.transform, false);
        var image = prefab.AddComponent<Image>();
        image.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        var button = prefab.AddComponent<Button>();
        var slotScript = prefab.AddComponent<SaveSlotButton>();

        GameObject labelGO = new GameObject("Label");
        labelGO.transform.SetParent(prefab.transform, false);
        var label = labelGO.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = 10;
        label.color = Color.white;
        label.alignment = TextAnchor.MiddleLeft;
        label.rectTransform.anchorMin = new Vector2(0.05f, 0.1f);
        label.rectTransform.anchorMax = new Vector2(0.95f, 0.9f);
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;

        slotScript.label = label;
        prefab.SetActive(false);
        return prefab;
    }
    private GameObject InstantiateSlotButton(Transform parent, string text, string slotName)
    {
        GameObject slot = Instantiate(slotButtonPrefab, parent, false);
        slot.SetActive(true);

        var button = slot.GetComponent<SaveSlotButton>();
        if (button == null)
        {
            button = slot.AddComponent<SaveSlotButton>();
        }

        button.Setup(this, slotName, text);
        button.SetInteractable(!string.IsNullOrEmpty(slotName));

        if (button.label != null)
        {
            button.label.text = text;
        }

        if (slotName != null)
        {
            slot.GetComponent<Button>().onClick.AddListener(() => SelectSlot(slotName));
        }

        spawnedSlots.Add(slot);
        return slot;
    }

    private Button CreateMenuButton(string label, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonGO = new GameObject(label + "Button");
        buttonGO.transform.SetParent(menuRoot.transform, false);
        var image = buttonGO.AddComponent<Image>();
        image.color = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        var button = buttonGO.AddComponent<Button>();
        var rect = (RectTransform)button.transform;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        var text = textGO.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        var textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    private void EnsureEventSystemExists()
    {
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        DontDestroyOnLoad(eventSystemGO);
    }
}

