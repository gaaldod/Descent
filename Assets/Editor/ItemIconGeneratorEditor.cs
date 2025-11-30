using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using DevionGames.InventorySystem;
using DevionGames;
using DGItem = DevionGames.InventorySystem.Item;

/// <summary>
/// Editor tool to generate item icons from 3D models and assign them to items in the database.
/// </summary>
public class ItemIconGeneratorEditor : EditorWindow
{
    private GameObject modelPrefab;
    private ItemDatabase database;
    private DGItem targetItem;
    private ItemIconGenerator generator;
    private Vector2 scrollPosition;
    private string itemSearchString = "Search...";
    
    [MenuItem("Tools/Generate Item Icon from 3D Model")]
    public static void ShowWindow()
    {
        GetWindow<ItemIconGeneratorEditor>("Item Icon Generator");
    }
    
    private void OnEnable()
    {
        // Create a temporary generator if needed
        if (generator == null)
        {
            GameObject generatorObj = new GameObject("ItemIconGenerator");
            generator = generatorObj.AddComponent<ItemIconGenerator>();
            generator.hideFlags = HideFlags.HideAndDontSave;
        }
    }
    
    private void OnDisable()
    {
        if (generator != null)
        {
            DestroyImmediate(generator.gameObject);
        }
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("Generate Item Icon from 3D Model", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("This tool generates a 2D icon sprite from a 3D model and assigns it to an item in your database.", MessageType.Info);
        EditorGUILayout.Space();
        
        // Model prefab selection
        EditorGUILayout.LabelField("3D Model", EditorStyles.boldLabel);
        modelPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Model Prefab/GameObject",
            modelPrefab,
            typeof(GameObject),
            false
        );
        
        EditorGUILayout.Space();
        
        // Database selection
        EditorGUILayout.LabelField("Database", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Item Database", GUILayout.Width(100f));
        if (GUILayout.Button(database != null ? database.name : "Select Database", EditorStyles.objectField))
        {
            SelectDatabase();
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Item selection
        EditorGUILayout.LabelField("Target Item", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Item", GUILayout.Width(100f));
        string itemName = targetItem != null ? targetItem.Name : "None";
        if (GUILayout.Button(itemName, EditorStyles.objectField))
        {
            if (database != null)
            {
                SelectItem();
            }
            else
            {
                EditorUtility.DisplayDialog("No Database", "Please select a database first.", "OK");
            }
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Generator settings
        EditorGUILayout.LabelField("Render Settings", EditorStyles.boldLabel);
        if (generator != null)
        {
            generator.iconSize = EditorGUILayout.IntField("Icon Size", generator.iconSize);
            generator.cameraDistance = EditorGUILayout.FloatField("Camera Distance", generator.cameraDistance);
            generator.fieldOfView = EditorGUILayout.Slider("Field of View", generator.fieldOfView, 10f, 90f);
            generator.lightIntensity = EditorGUILayout.FloatField("Light Intensity", generator.lightIntensity);
            generator.backgroundColor = EditorGUILayout.ColorField("Background Color", generator.backgroundColor);
        }
        
        EditorGUILayout.Space();
        
        // Generate button
        GUI.enabled = modelPrefab != null && targetItem != null;
        if (GUILayout.Button("Generate Icon and Assign to Item", GUILayout.Height(30)))
        {
            GenerateAndAssignIcon();
        }
        GUI.enabled = true;
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Note: The icon will be generated at runtime. For permanent icons, save the generated sprite as an asset.", MessageType.Warning);
        
        EditorGUILayout.EndScrollView();
    }
    
    private void SelectDatabase()
    {
        string searchString = "Search...";
        ItemDatabase[] databases = EditorTools.FindAssets<ItemDatabase>();
        
        UtilityInstanceWindow.ShowWindow("Select Database", delegate ()
        {
            searchString = EditorTools.SearchField(searchString);
            
            for (int i = 0; i < databases.Length; i++)
            {
                if (!string.IsNullOrEmpty(searchString) && !searchString.Equals("Search...") && !databases[i].name.Contains(searchString))
                {
                    continue;
                }
                GUIStyle style = new GUIStyle("button");
                style.wordWrap = true;
                if (GUILayout.Button(AssetDatabase.GetAssetPath(databases[i]), style))
                {
                    this.database = databases[i];
                    targetItem = null; // Clear item selection when database changes
                    UtilityInstanceWindow.CloseWindow();
                }
            }
        });
    }
    
    private void SelectItem()
    {
        if (database == null || database.items == null) return;
        
        itemSearchString = "Search...";
        List<DGItem> items = database.items.ToList();
        
        UtilityInstanceWindow.ShowWindow("Select Item", delegate ()
        {
            itemSearchString = EditorTools.SearchField(itemSearchString);
            
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                
                if (!string.IsNullOrEmpty(itemSearchString) && !itemSearchString.Equals("Search...") && !items[i].Name.ToLower().Contains(itemSearchString.ToLower()))
                {
                    continue;
                }
                
                GUILayout.BeginHorizontal();
                
                // Show icon if available
                if (items[i].Icon != null)
                {
                    GUILayout.Label(items[i].Icon.texture, GUILayout.Width(20), GUILayout.Height(20));
                }
                else
                {
                    GUILayout.Label("", GUILayout.Width(20), GUILayout.Height(20));
                }
                
                GUIStyle style = new GUIStyle("button");
                style.alignment = TextAnchor.MiddleLeft;
                style.wordWrap = true;
                if (GUILayout.Button(items[i].Name, style))
                {
                    this.targetItem = items[i];
                    UtilityInstanceWindow.CloseWindow();
                    Repaint();
                }
                GUILayout.EndHorizontal();
            }
        });
    }
    
    private void GenerateAndAssignIcon()
    {
        if (generator == null || modelPrefab == null || targetItem == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign both a Model and an Item.", "OK");
            return;
        }
        
        try
        {
            // Generate the icon
            Sprite icon = generator.GenerateIconFromPrefab(modelPrefab);
            
            if (icon != null)
            {
                // Assign to item
                targetItem.Icon = icon;
                
                // Mark item as dirty so changes are saved
                EditorUtility.SetDirty(targetItem);
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                
                EditorUtility.DisplayDialog("Success", $"Icon generated and assigned to '{targetItem.Name}'!", "OK");
                Repaint();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Failed to generate icon. Check console for details.", "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"Failed to generate icon: {e.Message}", "OK");
            Debug.LogException(e);
        }
    }
}

