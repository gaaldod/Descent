using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using DevionGames.InventorySystem;
using DevionGames.UIWidgets;
using Currency = DevionGames.InventorySystem.Currency;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Save Settings")]
    public string savesFolderName = "saves";
    public string defaultSlotName = "SaveSlot";

    private string SavesPath => Path.Combine(Application.persistentDataPath, savesFolderName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Directory.CreateDirectory(SavesPath);
    }

    public List<SaveSlotInfo> GetSaveSlots()
    {
        var result = new List<SaveSlotInfo>();
        if (!Directory.Exists(SavesPath))
        {
            return result;
        }

        foreach (string file in Directory.GetFiles(SavesPath, "*.json"))
        {
            try
            {
                string json = File.ReadAllText(file);
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                result.Add(new SaveSlotInfo
                {
                    slotName = Path.GetFileNameWithoutExtension(file),
                    lastSaved = data.timestamp,
                    sceneName = data.sceneName
                });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to read save file '{file}': {ex.Message}");
            }
        }

        return result;
    }

    public void SaveToSlot(string slotName)
    {
        if (string.IsNullOrEmpty(slotName))
        {
            slotName = GenerateSlotName();
        }

        var player = FindFirstObjectByType<FirstPersonController>();
        var inventoryContainer = WidgetUtility.Find<ItemContainer>("Inventory");

        if (player == null)
        {
            Debug.LogWarning("Cannot save: missing player.");
            return;
        }

        // Get items from Devion Games inventory
        List<string> itemNames = new List<string>();
        if (inventoryContainer != null)
        {
            // Get items from slots
            foreach (var slot in inventoryContainer.Slots)
            {
                if (slot.ObservedItem != null && !(slot.ObservedItem is Currency))
                {
                    var item = slot.ObservedItem;
                    for (int i = 0; i < item.Stack; i++)
                    {
                        itemNames.Add(item.Name);
                    }
                }
            }
        }

        SaveData data = new SaveData
        {
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            position = player.transform.position,
            rotation = player.transform.rotation,
            inventoryItems = itemNames,
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(SavesPath, slotName + ".json");
        File.WriteAllText(path, json);
        InteractionSystem.Instance?.ShowNotification("Game saved to " + slotName + ".");
    }

    public void LoadSlot(string slotName)
    {
        string path = Path.Combine(SavesPath, slotName + ".json");
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Save slot '{slotName}' not found.");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        UnityEngine.SceneManagement.SceneManager.LoadScene(data.sceneName);
        StartCoroutine(RestoreAfterSceneLoad(data));
    }

    private IEnumerator RestoreAfterSceneLoad(SaveData data)
    {
        yield return null;

        var player = FindFirstObjectByType<FirstPersonController>();

        if (player != null)
        {
            player.transform.position = data.position;
            player.transform.rotation = data.rotation;
        }

        // Restore items to Devion Games inventory
        foreach (string itemName in data.inventoryItems)
        {
            DevionGamesInventoryWrapper.AddItem(itemName);
        }
    }

    private string GenerateSlotName()
    {
        return $"{defaultSlotName}_{DateTime.Now:yyyyMMdd_HHmmss}";
    }
}

[Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 position;
    public Quaternion rotation;
    public List<string> inventoryItems;
    public string timestamp;
}

public class SaveSlotInfo
{
    public string slotName;
    public string lastSaved;
    public string sceneName;
}

