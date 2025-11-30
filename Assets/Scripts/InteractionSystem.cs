using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance { get; private set; }

    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public LayerMask interactionLayer = -1;
    
    [Header("UI References")]
    public Text interactionText;
    public GameObject crosshair;
    [Header("Notifications")]
    public Text notificationText;
    public float notificationDuration = 2f;
    
    [SerializeField] private Camera playerCamera;
    private Interactable currentInteractable;
    private Coroutine notificationRoutine;

    private void Awake()
    {
        Instance = this;
    }
    
    void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (playerCamera == null)
        {
            Debug.LogError("InteractionSystem: No camera assigned or found in children.", this);
            enabled = false;
            return;
        }
        
        if (interactionText == null)
        {
            // Create UI text if not assigned
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas == null)
            {
                canvas = new GameObject("Canvas");
                canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.AddComponent<CanvasScaler>();
                canvas.AddComponent<GraphicRaycaster>();
            }
            
            GameObject textObj = new GameObject("InteractionText");
            textObj.transform.SetParent(canvas.transform);
            interactionText = textObj.AddComponent<Text>();
            interactionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            interactionText.fontSize = 24;
            interactionText.color = Color.white;
            interactionText.alignment = TextAnchor.MiddleCenter;
            RectTransform rect = interactionText.rectTransform;
            rect.anchorMin = new Vector2(0.25f, 0.1f);
            rect.anchorMax = new Vector2(0.75f, 0.2f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

            if (notificationText == null)
            {
                GameObject notifObj = new GameObject("NotificationText");
                notifObj.transform.SetParent(canvas.transform);
                notificationText = notifObj.AddComponent<Text>();
                notificationText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                notificationText.fontSize = 22;
                notificationText.color = Color.white;
                notificationText.alignment = TextAnchor.MiddleCenter;
                RectTransform notifRect = notificationText.rectTransform;
                notifRect.anchorMin = new Vector2(0.2f, 0.05f);
                notifRect.anchorMax = new Vector2(0.8f, 0.12f);
                notifRect.offsetMin = Vector2.zero;
                notifRect.offsetMax = Vector2.zero;
                notificationText.gameObject.SetActive(false);
            }
        }
    }
    
    void Update()
    {
        CheckForInteractables();
        HandleInteraction();
    }
    
    void CheckForInteractables()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green);
            // Debug.Log("Hit " + hit.collider.name);
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            
            if (interactable != null && interactable.CanInteract())
            {
                SetCurrentInteractable(interactable);
                ShowInteractionPrompt(interactable.GetInteractionText());
                return;
            }
        }

        SetCurrentInteractable(null);
        HideInteractionPrompt();
    }
    
    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }
    
    void ShowInteractionPrompt(string text)
    {
        if (interactionText != null)
        {
            interactionText.text = "Press E to " + text;
            interactionText.gameObject.SetActive(true);
        }
    }
    
    void HideInteractionPrompt()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationText == null)
        {
            return;
        }

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine = StartCoroutine(HideNotificationAfterDelay());
    }

    private IEnumerator HideNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
        notificationRoutine = null;
    }

    private void SetCurrentInteractable(Interactable interactable)
    {
        if (currentInteractable == interactable)
        {
            return;
        }

        currentInteractable?.OnFocusExit();
        currentInteractable = interactable;
        currentInteractable?.OnFocus();
    }
}
