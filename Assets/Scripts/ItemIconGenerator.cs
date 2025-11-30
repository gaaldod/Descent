using UnityEngine;
using System.Collections;

/// <summary>
/// Utility to generate item icons from 3D models by rendering them to a texture.
/// This can be used at runtime or in editor to create sprites from 3D prefabs.
/// </summary>
public class ItemIconGenerator : MonoBehaviour
{
    [Header("Render Settings")]
    [Tooltip("Size of the generated icon texture")]
    public int iconSize = 256;
    
    [Tooltip("Background color for the icon")]
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0f);
    
    [Tooltip("Camera distance from model")]
    public float cameraDistance = 2f;
    
    [Tooltip("Camera field of view")]
    public float fieldOfView = 30f;
    
    [Tooltip("Light intensity for rendering")]
    public float lightIntensity = 1.5f;
    
    private Camera renderCamera;
    private Light renderLight;
    private RenderTexture renderTexture;
    private GameObject currentModel;
    
    /// <summary>
    /// Generates a Sprite icon from a 3D GameObject prefab
    /// </summary>
    public Sprite GenerateIconFromPrefab(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[ItemIconGenerator] Prefab is null!");
            return null;
        }
        
        SetupRenderEnvironment();
        
        // Instantiate the model
        currentModel = Instantiate(prefab);
        currentModel.transform.position = Vector3.zero;
        currentModel.transform.rotation = Quaternion.identity;
        
        // Ensure model is active and visible
        currentModel.SetActive(true);
        
        // Position camera to frame the model
        Bounds bounds = GetBounds(currentModel);
        Vector3 center = bounds.center;
        float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        
        if (size <= 0.01f) size = 1f; // Fallback if bounds are invalid
        
        renderCamera.transform.position = center + Vector3.back * (size * cameraDistance);
        renderCamera.transform.LookAt(center);
        renderCamera.fieldOfView = fieldOfView;
        
        // Ensure model is on a visible layer
        SetLayerRecursively(currentModel, 0); // Default layer
        
        // Render to texture
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
        
        // Convert RenderTexture to Texture2D
        Texture2D texture = RenderTextureToTexture2D(renderTexture);
        
        // Create Sprite from texture
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, iconSize, iconSize),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        // Cleanup
        Cleanup();
        
        return sprite;
    }
    
    /// <summary>
    /// Generates a Sprite icon from a 3D GameObject in the scene
    /// </summary>
    public Sprite GenerateIconFromGameObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("[ItemIconGenerator] GameObject is null!");
            return null;
        }
        
        SetupRenderEnvironment();
        
        // Use the existing GameObject (don't instantiate)
        currentModel = gameObject;
        Vector3 originalPosition = currentModel.transform.position;
        Quaternion originalRotation = currentModel.transform.rotation;
        
        // Temporarily move to origin for rendering
        currentModel.transform.position = Vector3.zero;
        currentModel.transform.rotation = Quaternion.identity;
        
        // Position camera to frame the model
        Bounds bounds = GetBounds(currentModel);
        Vector3 center = bounds.center;
        float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        
        renderCamera.transform.position = center + Vector3.back * (size * cameraDistance);
        renderCamera.transform.LookAt(center);
        renderCamera.fieldOfView = fieldOfView;
        
        // Render to texture
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
        
        // Convert RenderTexture to Texture2D
        Texture2D texture = RenderTextureToTexture2D(renderTexture);
        
        // Create Sprite from texture
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, iconSize, iconSize),
            new Vector2(0.5f, 0.5f),
            100f
        );
        
        // Restore original transform
        currentModel.transform.position = originalPosition;
        currentModel.transform.rotation = originalRotation;
        
        // Cleanup (but don't destroy the model since it's from scene)
        CleanupRenderEnvironment();
        currentModel = null;
        
        return sprite;
    }
    
    private void SetupRenderEnvironment()
    {
        // Create render texture
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(iconSize, iconSize, 24);
            renderTexture.antiAliasing = 4;
        }
        
        // Create camera if needed
        if (renderCamera == null)
        {
            GameObject cameraObj = new GameObject("IconRenderCamera");
            cameraObj.transform.SetParent(transform);
            renderCamera = cameraObj.AddComponent<Camera>();
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = backgroundColor;
            renderCamera.orthographic = false;
            renderCamera.fieldOfView = fieldOfView;
            renderCamera.cullingMask = -1; // Render all layers
            renderCamera.enabled = false; // We'll call Render() manually
            renderCamera.useOcclusionCulling = false;
        }
        
        // Create light if needed
        if (renderLight == null)
        {
            GameObject lightObj = new GameObject("IconRenderLight");
            lightObj.transform.SetParent(transform);
            renderLight = lightObj.AddComponent<Light>();
            renderLight.type = LightType.Directional;
            renderLight.intensity = lightIntensity;
            renderLight.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
            renderLight.shadows = LightShadows.None; // No shadows for icon rendering
        }
    }
    
    private Bounds GetBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.one);
        }
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        
        return bounds;
    }
    
    private Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTexture;
        
        Texture2D texture = new Texture2D(iconSize, iconSize, TextureFormat.RGBA32, false);
        texture.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
        texture.Apply();
        
        RenderTexture.active = previous;
        
        return texture;
    }
    
    private void Cleanup()
    {
        if (currentModel != null)
        {
            Destroy(currentModel);
            currentModel = null;
        }
        CleanupRenderEnvironment();
    }
    
    private void CleanupRenderEnvironment()
    {
        // Keep camera and light for reuse, just clear the model
    }
    
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    
    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (renderCamera != null)
        {
            Destroy(renderCamera.gameObject);
        }
        if (renderLight != null)
        {
            Destroy(renderLight.gameObject);
        }
    }
}

