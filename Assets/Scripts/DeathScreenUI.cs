using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Death screen UI that appears when the player is killed by the enemy.
/// </summary>
public class DeathScreenUI : MonoBehaviour
{
    private static DeathScreenUI instance;
    
    private CanvasGroup canvasGroup;
    private Image darkRedOverlay;
    private Text deathText;
    private Button loadSaveButton;
    private Button mainMenuButton;
    private bool isShowing = false;
    
    public static DeathScreenUI Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("DeathScreenUI");
                instance = go.AddComponent<DeathScreenUI>();
                DontDestroyOnLoad(go);
                instance.BuildUI();
            }
            return instance;
        }
    }
    
    private void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20000; // Very high to ensure it's on top of everything
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
        
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        // Dark red overlay
        GameObject overlayObj = new GameObject("DarkRedOverlay");
        overlayObj.transform.SetParent(transform, false);
        darkRedOverlay = overlayObj.AddComponent<Image>();
        darkRedOverlay.color = new Color(0.3f, 0f, 0f, 0.95f); // Dark red with high alpha
        darkRedOverlay.raycastTarget = false; // Don't block clicks on buttons
        var overlayRect = darkRedOverlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        // Death message
        GameObject deathTextObj = new GameObject("DeathText");
        deathTextObj.transform.SetParent(transform, false);
        deathText = deathTextObj.AddComponent<Text>();
        deathText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        deathText.fontSize = 48;
        deathText.color = Color.white;
        deathText.alignment = TextAnchor.MiddleCenter;
        deathText.text = "YOU DIED";
        var deathTextRect = deathText.rectTransform;
        deathTextRect.anchorMin = new Vector2(0.1f, 0.6f);
        deathTextRect.anchorMax = new Vector2(0.9f, 0.85f);
        deathTextRect.offsetMin = Vector2.zero;
        deathTextRect.offsetMax = Vector2.zero;
        
        // Ensure EventSystem exists for button interaction
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("[DeathScreenUI] Created EventSystem for button interaction");
        }
        
        // Button container (create AFTER overlay so it appears on top)
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(transform, false);
        var containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.25f, 0.15f);
        containerRect.anchorMax = new Vector2(0.75f, 0.45f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        // Load Save Button - Simple approach with visible background
        GameObject loadSaveBtnObj = new GameObject("LoadSaveButton");
        loadSaveBtnObj.transform.SetParent(buttonContainer.transform, false);
        var loadSaveBtnRect = loadSaveBtnObj.AddComponent<RectTransform>();
        loadSaveBtnRect.anchorMin = new Vector2(0f, 0.5f);
        loadSaveBtnRect.anchorMax = new Vector2(1f, 0.5f);
        loadSaveBtnRect.anchoredPosition = new Vector2(0f, 20f);
        loadSaveBtnRect.sizeDelta = new Vector2(0f, 50f);
        
        // Background image first
        Image loadSaveImage = loadSaveBtnObj.AddComponent<Image>();
        loadSaveImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Brighter gray
        loadSaveImage.raycastTarget = true;
        
        // Button component
        loadSaveButton = loadSaveBtnObj.AddComponent<Button>();
        var loadSaveColors = loadSaveButton.colors;
        loadSaveColors.normalColor = Color.white;
        loadSaveColors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        loadSaveColors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        loadSaveColors.selectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        loadSaveButton.colors = loadSaveColors;
        loadSaveButton.targetGraphic = loadSaveImage;
        
        // Text as child
        GameObject loadSaveTextObj = new GameObject("Text");
        loadSaveTextObj.transform.SetParent(loadSaveBtnObj.transform, false);
        Text loadSaveText = loadSaveTextObj.AddComponent<Text>();
        loadSaveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadSaveText.fontSize = 32;
        loadSaveText.color = Color.white;
        loadSaveText.alignment = TextAnchor.MiddleCenter;
        loadSaveText.text = "Load Last Save";
        loadSaveText.raycastTarget = false;
        
        var loadSaveTextRect = loadSaveText.rectTransform;
        loadSaveTextRect.anchorMin = Vector2.zero;
        loadSaveTextRect.anchorMax = Vector2.one;
        loadSaveTextRect.sizeDelta = Vector2.zero;
        loadSaveTextRect.anchoredPosition = Vector2.zero;
        
        loadSaveButton.onClick.AddListener(OnLoadSaveClicked);
        
        // Main Menu Button
        GameObject mainMenuBtnObj = new GameObject("MainMenuButton");
        mainMenuBtnObj.transform.SetParent(buttonContainer.transform, false);
        var mainMenuBtnRect = mainMenuBtnObj.AddComponent<RectTransform>();
        mainMenuBtnRect.anchorMin = new Vector2(0f, 0.5f);
        mainMenuBtnRect.anchorMax = new Vector2(1f, 0.5f);
        mainMenuBtnRect.anchoredPosition = new Vector2(0f, -30f);
        mainMenuBtnRect.sizeDelta = new Vector2(0f, 50f);
        
        // Background image first
        Image mainMenuImage = mainMenuBtnObj.AddComponent<Image>();
        mainMenuImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Brighter gray
        mainMenuImage.raycastTarget = true;
        
        // Button component
        mainMenuButton = mainMenuBtnObj.AddComponent<Button>();
        var mainMenuColors = mainMenuButton.colors;
        mainMenuColors.normalColor = Color.white;
        mainMenuColors.highlightedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
        mainMenuColors.pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        mainMenuColors.selectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
        mainMenuButton.colors = mainMenuColors;
        mainMenuButton.targetGraphic = mainMenuImage;
        
        // Text as child
        GameObject mainMenuTextObj = new GameObject("Text");
        mainMenuTextObj.transform.SetParent(mainMenuBtnObj.transform, false);
        Text mainMenuText = mainMenuTextObj.AddComponent<Text>();
        mainMenuText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mainMenuText.fontSize = 32;
        mainMenuText.color = Color.white;
        mainMenuText.alignment = TextAnchor.MiddleCenter;
        mainMenuText.text = "Main Menu";
        mainMenuText.raycastTarget = false;
        
        var mainMenuTextRect = mainMenuText.rectTransform;
        mainMenuTextRect.anchorMin = Vector2.zero;
        mainMenuTextRect.anchorMax = Vector2.one;
        mainMenuTextRect.sizeDelta = Vector2.zero;
        mainMenuTextRect.anchoredPosition = Vector2.zero;
        
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        
        // Ensure buttons are active
        loadSaveBtnObj.SetActive(true);
        mainMenuBtnObj.SetActive(true);
        buttonContainer.SetActive(true);
        
        Debug.Log("[DeathScreenUI] UI built - Buttons created with EventSystem");
    }
    
    public void Show()
    {
        if (isShowing) return;
        
        isShowing = true;
        
        // Lock player controls immediately
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null)
        {
            player.playerCanMove = false;
            player.cameraCanMove = false;
        }
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Check if save exists and enable/disable button accordingly
        bool hasSave = SaveManager.Instance != null && SaveManager.Instance.GetSaveSlots().Count > 0;
        if (loadSaveButton != null)
        {
            loadSaveButton.interactable = hasSave;
            if (!hasSave)
            {
                Text buttonText = loadSaveButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.text = "No Save Available";
                    buttonText.color = new Color(0.5f, 0.5f, 0.5f, 1f); // Gray out
                }
            }
        }
        
        // Start fade-in coroutine
        StartCoroutine(FadeInDeathScreen());
    }
    
    private IEnumerator FadeInDeathScreen()
    {
        // Fade in the dark red overlay over 1 second
        float fadeDuration = 1f;
        float elapsed = 0f;
        
        // Start with overlay at 0 alpha
        if (darkRedOverlay != null)
        {
            Color startColor = darkRedOverlay.color;
            startColor.a = 0f;
            darkRedOverlay.color = startColor;
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        // Gradually fade in while game is still running
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works even if timeScale changes
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            
            if (darkRedOverlay != null)
            {
                Color currentColor = darkRedOverlay.color;
                currentColor.a = Mathf.Lerp(0f, 0.95f, t);
                darkRedOverlay.color = currentColor;
            }
            
            canvasGroup.alpha = t;
            yield return null;
        }
        
        // Ensure full opacity
        if (darkRedOverlay != null)
        {
            Color finalColor = darkRedOverlay.color;
            finalColor.a = 0.95f;
            darkRedOverlay.color = finalColor;
        }
        canvasGroup.alpha = 1f;
        
        // Ensure buttons are visible and interactable
        if (loadSaveButton != null)
        {
            loadSaveButton.gameObject.SetActive(true);
            loadSaveButton.interactable = true;
            // Force button to be enabled
            var buttonImage = loadSaveButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = true;
                buttonImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
            var buttonText = loadSaveButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.enabled = true;
                buttonText.color = Color.white;
            }
            Debug.Log($"[DeathScreenUI] Load Save Button active: {loadSaveButton.gameObject.activeSelf}, interactable: {loadSaveButton.interactable}, Image: {buttonImage != null}, Text: {buttonText != null}");
        }
        if (mainMenuButton != null)
        {
            mainMenuButton.gameObject.SetActive(true);
            mainMenuButton.interactable = true;
            // Force button to be enabled
            var buttonImage = mainMenuButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = true;
                buttonImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            }
            var buttonText = mainMenuButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.enabled = true;
                buttonText.color = Color.white;
            }
            Debug.Log($"[DeathScreenUI] Main Menu Button active: {mainMenuButton.gameObject.activeSelf}, interactable: {mainMenuButton.interactable}, Image: {buttonImage != null}, Text: {buttonText != null}");
        }
        
        // Add instruction text for keyboard shortcuts
        if (deathText != null)
        {
            deathText.text = "YOU DIED\n\nPress 1 to Load Last Save\nPress 2 for Main Menu";
        }
        
        // Pause the game after fade-in completes
        Time.timeScale = 0f;
    }
    
    private void OnLoadSaveClicked()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("[DeathScreenUI] SaveManager not found!");
            return;
        }
        
        var saveSlots = SaveManager.Instance.GetSaveSlots();
        if (saveSlots.Count == 0)
        {
            Debug.LogWarning("[DeathScreenUI] No save slots available!");
            return;
        }
        
        // Get the most recent save
        var mostRecentSave = saveSlots.OrderByDescending(s => s.lastSaved).FirstOrDefault();
        if (mostRecentSave != null)
        {
            // Resume time before loading
            Time.timeScale = 1f;
            
            // Load the save
            SaveManager.Instance.LoadSlot(mostRecentSave.slotName);
        }
    }
    
    private void OnMainMenuClicked()
    {
        // Resume time before loading scene
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene("mainMenuScene");
    }
    
    private void Update()
    {
        // Allow keyboard input as backup if buttons don't work
        if (isShowing && Time.timeScale == 0f)
        {
            // Check for keyboard input
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                // Load save
                OnLoadSaveClicked();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                // Main menu
                OnMainMenuClicked();
            }
        }
    }
    
    public void Hide()
    {
        if (!isShowing) return;
        
        isShowing = false;
        
        // Resume the game
        Time.timeScale = 1f;
        
        // Unlock player controls
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null)
        {
            player.playerCanMove = true;
            player.cameraCanMove = true;
        }
        
        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Hide UI
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        canvasGroup.alpha = 0f;
    }
}

