using UnityEngine;
using System.Collections;
using DevionGames.InventorySystem;
using DevionGames.UIWidgets;
using DevionGames;
using UnityEngine.Events;
using DGItem = DevionGames.InventorySystem.Item;

/// <summary>
/// Wrapper to integrate Devion Games Inventory System with our game.
/// Handles keyboard input to toggle inventory and provides helper methods.
/// </summary>
public class DevionGamesInventoryWrapper : MonoBehaviour
{
    [Header("Inventory Settings")]
    [Tooltip("Name of the inventory window in Devion Games system")]
    public string inventoryWindowName = "Inventory";
    
    [Tooltip("Key to toggle inventory")]
    public KeyCode toggleKey = KeyCode.I;
    
    private ItemContainer inventoryContainer;
    private FirstPersonController playerController;
    private bool isInventoryOpen = false;
    
    private void Awake()
    {
        // Mark this GameObject to persist across scene loads
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Find components in Start to ensure scene is fully loaded
        StartCoroutine(RefreshReferencesDelayed());
    }
    
    private IEnumerator RefreshReferencesDelayed()
    {
        // Wait a frame to ensure all GameObjects are loaded
        yield return null;
        RefreshReferences();
    }
    
    private void RefreshReferences()
    {
        // Find the inventory container - try multiple methods
        if (inventoryContainer == null)
        {
            // Method 1: Use WidgetUtility.Find (searches by Name property)
            inventoryContainer = WidgetUtility.Find<ItemContainer>(inventoryWindowName);
            
            // Method 2: If that fails, try finding by GameObject name
            if (inventoryContainer == null)
            {
                GameObject inventoryGO = GameObject.Find(inventoryWindowName);
                if (inventoryGO != null)
                {
                    inventoryContainer = inventoryGO.GetComponent<ItemContainer>();
                    if (inventoryContainer != null)
                    {
                        Debug.Log($"[DevionGamesInventoryWrapper] Found ItemContainer by GameObject name '{inventoryWindowName}'. Consider setting the ItemContainer's Name property to '{inventoryWindowName}' for better compatibility.");
                    }
                }
            }
            
            // Method 3: Find any ItemContainer in the scene
            if (inventoryContainer == null)
            {
                ItemContainer[] allContainers = FindObjectsByType<ItemContainer>(FindObjectsSortMode.None);
                if (allContainers.Length > 0)
                {
                    Debug.LogWarning($"[DevionGamesInventoryWrapper] Found {allContainers.Length} ItemContainer(s) but none with name '{inventoryWindowName}'. Available names: {string.Join(", ", System.Array.ConvertAll(allContainers, c => c.Name))}");
                    // Use the first one as fallback
                    inventoryContainer = allContainers[0];
                    Debug.LogWarning($"[DevionGamesInventoryWrapper] Using first ItemContainer found: '{inventoryContainer.Name}'");
                }
            }
            
            if (inventoryContainer == null)
            {
                Debug.LogError($"[DevionGamesInventoryWrapper] Could not find inventory window named '{inventoryWindowName}'. Make sure you have an ItemContainer with this name in your scene. The ItemContainer component's 'Name' field (not GameObject name) must be set to '{inventoryWindowName}'.");
            }
            else
            {
                // Configure the ItemContainer for first-person use
                ConfigureItemContainer();
            }
        }
        
        // Find player controller
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<FirstPersonController>();
        }
    }
    
    private void ConfigureItemContainer()
    {
        if (inventoryContainer == null) return;
        
        // Enable cursor show/hide (this is a UIWidget property)
        // We'll handle it manually, but make sure the widget is set up correctly
        var canvasGroup = inventoryContainer.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogWarning("[DevionGamesInventoryWrapper] ItemContainer missing CanvasGroup component!");
        }
        
        // Check if ItemContainer has a Canvas parent
        Canvas canvas = inventoryContainer.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[DevionGamesInventoryWrapper] ItemContainer must be a child of a Canvas! The inventory UI won't be visible without a Canvas.");
        }
        else
        {
            Debug.Log($"[DevionGamesInventoryWrapper] Found Canvas: {canvas.name}, Render Mode: {canvas.renderMode}");
        }
        
        // Hook into the ItemContainer's close event so exit button works
        inventoryContainer.RegisterListener("OnClose", new UnityAction<CallbackEventData>(OnInventoryClosed));
        
        // Log ItemContainer state
        Debug.Log($"[DevionGamesInventoryWrapper] ItemContainer found: {inventoryContainer.name}, Active: {inventoryContainer.gameObject.activeSelf}, Visible: {inventoryContainer.IsVisible}");
    }
    
    /// <summary>
    /// Called when the inventory UI is closed (via exit button or other means)
    /// </summary>
    private void OnInventoryClosed(CallbackEventData eventData)
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
    }
    
    private void Update()
    {
        // Refresh references if needed (in case scene changed)
        if (inventoryContainer == null || playerController == null)
        {
            RefreshReferences();
        }
        
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }
    
    /// <summary>
    /// Toggles the inventory window visibility
    /// </summary>
    public void ToggleInventory()
    {
        if (inventoryContainer == null)
        {
            Debug.LogWarning("[DevionGamesInventoryWrapper] Inventory container not found!");
            RefreshReferences();
            return;
        }
        
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }
    
    /// <summary>
    /// Opens the inventory window
    /// </summary>
    public void OpenInventory()
    {
        if (inventoryContainer == null) return;
        
        RefreshReferences();
        
        // Lock player movement and camera FIRST
        if (playerController != null)
        {
            playerController.playerCanMove = false;
            playerController.cameraCanMove = false;
        }
        
        // Pause time IMMEDIATELY (before showing UI)
        Time.timeScale = 0f;
        
        // Show cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        // Show the inventory (this triggers animation)
        inventoryContainer.Show();
        
        isInventoryOpen = true;
        
        Debug.Log("[DevionGamesInventoryWrapper] Inventory opened - Game paused, camera locked");
    }
    
    /// <summary>
    /// Closes the inventory window
    /// </summary>
    public void CloseInventory()
    {
        if (inventoryContainer == null) return;
        
        // Only close if we're actually open (prevents double-closing)
        if (!isInventoryOpen) return;
        
        // Resume time FIRST
        Time.timeScale = 1f;
        
        // Hide cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        // Close the inventory UI
        inventoryContainer.Close();
        
        // Unlock player movement and camera
        if (playerController != null)
        {
            playerController.playerCanMove = true;
            playerController.cameraCanMove = true;
        }
        
        isInventoryOpen = false;
        
        Debug.Log("[DevionGamesInventoryWrapper] Inventory closed - Game resumed, camera unlocked");
    }
    
    /// <summary>
    /// Checks if player has an item
    /// </summary>
    public static bool HasItem(string itemName)
    {
        return ItemContainer.GetItemAmount("Inventory", itemName) >= 1;
    }
    
    /// <summary>
    /// Adds an item to inventory
    /// </summary>
    public static bool AddItem(string itemName)
    {
        // Try to find item in database
        if (InventoryManager.Database == null)
        {
            Debug.LogError("[DevionGamesInventoryWrapper] InventoryManager.Database is null! Make sure InventoryManager is initialized.");
            return false;
        }
        
        DGItem item = InventoryManager.Database.items.Find(x => x.Name == itemName);
        if (item == null)
        {
            Debug.LogWarning($"[DevionGamesInventoryWrapper] Item '{itemName}' not found in inventory database!");
            return false;
        }
        
        // If item has no icon, try to generate one from the 3D model in the scene
        if (item.Icon == null)
        {
            TryGenerateIconFromSceneModel(item, itemName);
        }
        
        DGItem instance = Instantiate(item);
        return ItemContainer.AddItem("Inventory", instance);
    }
    
    /// <summary>
    /// Tries to find a 3D model in the scene matching the item name and generate an icon from it
    /// </summary>
    private static void TryGenerateIconFromSceneModel(DGItem item, string itemName)
    {
        // Try to find a GameObject in the scene with matching name
        GameObject model = GameObject.Find(itemName);
        if (model == null)
        {
            // Try common variations
            model = GameObject.Find(itemName + "(Clone)");
        }
        
        if (model == null)
        {
            // Search for objects with Item component that match
            Item[] items = FindObjectsByType<Item>(FindObjectsSortMode.None);
            foreach (Item sceneItem in items)
            {
                if (sceneItem.itemName == itemName)
                {
                    model = sceneItem.gameObject;
                    break;
                }
            }
        }
        
        if (model != null)
        {
            // Create a temporary generator
            GameObject generatorObj = new GameObject("TempIconGenerator");
            ItemIconGenerator generator = generatorObj.AddComponent<ItemIconGenerator>();
            generator.hideFlags = HideFlags.HideAndDontSave;
            
            // Generate icon from the model
            Sprite icon = generator.GenerateIconFromGameObject(model);
            
            if (icon != null)
            {
                item.Icon = icon;
                Debug.Log($"[DevionGamesInventoryWrapper] Generated icon for '{itemName}' from 3D model in scene.");
            }
            
            // Cleanup
            Destroy(generatorObj);
        }
        else
        {
            Debug.LogWarning($"[DevionGamesInventoryWrapper] Could not find 3D model for '{itemName}' to generate icon. Item will appear without icon.");
        }
    }
    
    /// <summary>
    /// Removes an item from inventory
    /// </summary>
    public static bool RemoveItem(string itemName, int amount = 1)
    {
        DGItem item = ItemContainer.GetItem("Inventory", itemName);
        if (item == null)
        {
            return false;
        }
        
        return ItemContainer.RemoveItem("Inventory", item, amount);
    }
}

