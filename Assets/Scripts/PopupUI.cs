using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Singleton UI system for displaying popup messages that pause the game.
/// </summary>
public class PopupUI : MonoBehaviour
{
    private static PopupUI instance;
    
    private CanvasGroup canvasGroup;
    private Text popupText;
    private Image backgroundImage;
    private bool isShowing = false;
    
    public static PopupUI Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("PopupUI");
                instance = go.AddComponent<PopupUI>();
                DontDestroyOnLoad(go);
                instance.BuildUI();
            }
            return instance;
        }
    }
    
    public bool IsShowing
    {
        get { return isShowing; }
    }
    
    private void BuildUI()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10000; // Very high to ensure it's on top
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
        
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        
        // Background
        GameObject background = new GameObject("PopupBackground");
        background.transform.SetParent(transform, false);
        backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
        var bgRect = backgroundImage.rectTransform;
        bgRect.anchorMin = new Vector2(0.15f, 0.3f);
        bgRect.anchorMax = new Vector2(0.85f, 0.7f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Text
        GameObject textObj = new GameObject("PopupText");
        textObj.transform.SetParent(background.transform, false);
        popupText = textObj.AddComponent<Text>();
        popupText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        popupText.fontSize = 24;
        popupText.color = Color.white;
        popupText.alignment = TextAnchor.MiddleCenter;
        popupText.supportRichText = true;
        var textRect = popupText.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.1f);
        textRect.anchorMax = new Vector2(0.95f, 0.9f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Instruction text (press E to close)
        GameObject instructionObj = new GameObject("InstructionText");
        instructionObj.transform.SetParent(background.transform, false);
        Text instructionText = instructionObj.AddComponent<Text>();
        instructionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instructionText.fontSize = 16;
        instructionText.color = new Color(1f, 1f, 1f, 0.7f);
        instructionText.alignment = TextAnchor.LowerCenter;
        instructionText.text = "Press E to continue";
        var instructionRect = instructionText.rectTransform;
        instructionRect.anchorMin = new Vector2(0.05f, 0.02f);
        instructionRect.anchorMax = new Vector2(0.95f, 0.15f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;
    }
    
    public void Show(string message, Color bgColor, Color textColor, int fontSize)
    {
        if (popupText == null || backgroundImage == null)
        {
            return;
        }
        
        // Set message and colors
        popupText.text = message;
        popupText.color = textColor;
        popupText.fontSize = fontSize;
        backgroundImage.color = bgColor;
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Lock player controls
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player != null)
        {
            player.playerCanMove = false;
            player.cameraCanMove = false;
        }
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Show UI
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        canvasGroup.alpha = 1f;
        isShowing = true;
    }
    
    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }
        
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
        isShowing = false;
    }
    
    private void Update()
    {
        // Check for E key to close popup
        if (isShowing && Input.GetKeyDown(KeyCode.E))
        {
            Hide();
        }
    }
}

