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
        
        // Button container
        GameObject buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(transform, false);
        var containerRect = buttonContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.3f, 0.2f);
        containerRect.anchorMax = new Vector2(0.7f, 0.5f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        
        var verticalLayout = buttonContainer.AddComponent<VerticalLayoutGroup>();
        verticalLayout.spacing = 20f;
        verticalLayout.childAlignment = TextAnchor.MiddleCenter;
        verticalLayout.childControlHeight = true;
        verticalLayout.childControlWidth = true;
        verticalLayout.childForceExpandHeight = false;
        verticalLayout.childForceExpandWidth = true;
        
        // Load Save Button
        GameObject loadSaveBtnObj = new GameObject("LoadSaveButton");
        loadSaveBtnObj.transform.SetParent(buttonContainer.transform, false);
        loadSaveButton = loadSaveBtnObj.AddComponent<Button>();
        var loadSaveColors = loadSaveButton.colors;
        loadSaveColors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        loadSaveColors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        loadSaveColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        loadSaveButton.colors = loadSaveColors;
        
        Image loadSaveImage = loadSaveBtnObj.AddComponent<Image>();
        loadSaveImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        GameObject loadSaveTextObj = new GameObject("Text");
        loadSaveTextObj.transform.SetParent(loadSaveBtnObj.transform, false);
        Text loadSaveText = loadSaveTextObj.AddComponent<Text>();
        loadSaveText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loadSaveText.fontSize = 24;
        loadSaveText.color = Color.white;
        loadSaveText.alignment = TextAnchor.MiddleCenter;
        loadSaveText.text = "Load Last Save";
        
        var loadSaveTextRect = loadSaveText.rectTransform;
        loadSaveTextRect.anchorMin = Vector2.zero;
        loadSaveTextRect.anchorMax = Vector2.one;
        loadSaveTextRect.offsetMin = Vector2.zero;
        loadSaveTextRect.offsetMax = Vector2.zero;
        
        loadSaveButton.onClick.AddListener(OnLoadSaveClicked);
        
        // Main Menu Button
        GameObject mainMenuBtnObj = new GameObject("MainMenuButton");
        mainMenuBtnObj.transform.SetParent(buttonContainer.transform, false);
        mainMenuButton = mainMenuBtnObj.AddComponent<Button>();
        var mainMenuColors = mainMenuButton.colors;
        mainMenuColors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        mainMenuColors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        mainMenuColors.pressedColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        mainMenuButton.colors = mainMenuColors;
        
        Image mainMenuImage = mainMenuBtnObj.AddComponent<Image>();
        mainMenuImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        GameObject mainMenuTextObj = new GameObject("Text");
        mainMenuTextObj.transform.SetParent(mainMenuBtnObj.transform, false);
        Text mainMenuText = mainMenuTextObj.AddComponent<Text>();
        mainMenuText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mainMenuText.fontSize = 24;
        mainMenuText.color = Color.white;
        mainMenuText.alignment = TextAnchor.MiddleCenter;
        mainMenuText.text = "Main Menu";
        
        var mainMenuTextRect = mainMenuText.rectTransform;
        mainMenuTextRect.anchorMin = Vector2.zero;
        mainMenuTextRect.anchorMax = Vector2.one;
        mainMenuTextRect.offsetMin = Vector2.zero;
        mainMenuTextRect.offsetMax = Vector2.zero;
        
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
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
        SceneManager.LoadScene("MainMenu");
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

