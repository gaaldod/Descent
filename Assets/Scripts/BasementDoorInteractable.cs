using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DevionGames.InventorySystem;

/// <summary>
/// Basement door that requires 3 Basement Keys to open and loads MainMap scene.
/// </summary>
public class BasementDoorInteractable : Interactable
{
    [Header("Door Requirements")]
    public string requiredKeyName = "Basement Key";
    public int requiredKeyCount = 3;
    public bool consumeKeysOnUse = true;
    public string lockedMessage = "Locked... I can see 3 keyholes";

    [Header("Scene Transition")]
    public string targetSceneName = "MainMap";
    public float fadeDuration = 1f;

    [Header("UI / Fade")]
    public CanvasGroup fadeCanvas;

    [Header("Audio")]
    public AudioClip lockedClip;
    public AudioClip openClip;

    private bool isOpen;
    private bool isAnimating;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (string.IsNullOrEmpty(interactionText))
        {
            interactionText = "open basement door";
        }
    }

    public override string GetInteractionText()
    {
        if (isOpen || isAnimating)
        {
            return "";
        }

        int keyCount = ItemContainer.GetItemAmount("Inventory", requiredKeyName);
        
        if (keyCount >= requiredKeyCount)
        {
            return "open basement door";
        }
        else
        {
            return $"open basement door ({keyCount}/{requiredKeyCount} keys)";
        }
    }

    public override void Interact()
    {
        if (isAnimating || isOpen)
        {
            return;
        }

        // Check if player has enough keys
        int keyCount = ItemContainer.GetItemAmount("Inventory", requiredKeyName);
        
        if (keyCount >= requiredKeyCount)
        {
            // Consume all required keys
            if (consumeKeysOnUse)
            {
                for (int i = 0; i < requiredKeyCount; i++)
                {
                    DevionGamesInventoryWrapper.RemoveItem(requiredKeyName);
                }
            }

            StartCoroutine(TransitionToScene());
        }
        else
        {
            // Show locked message
            if (InteractionSystem.Instance != null)
            {
                InteractionSystem.Instance.ShowNotification(lockedMessage);
            }
            else
            {
                Debug.Log(lockedMessage);
            }
            
            PlayClip(lockedClip);
        }
    }

    private IEnumerator TransitionToScene()
    {
        isAnimating = true;
        canInteract = false;

        // Get or create the scene transition manager
        SceneTransitionManager transitionManager = SceneTransitionManager.Instance;
        fadeCanvas = transitionManager.FadeCanvas;

        // Play open sound
        PlayClip(openClip);

        // Fade to black
        yield return FadeCanvas(1f);

        // Consume all required keys
        if (consumeKeysOnUse)
        {
            for (int i = 0; i < requiredKeyCount; i++)
            {
                DevionGamesInventoryWrapper.RemoveItem(requiredKeyName);
            }
        }

        // Load the scene
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            // Load scene asynchronously
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
            
            // Wait until the scene is fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogError("[BasementDoorInteractable] Target scene name is empty!");
        }

        // Note: Fade-out is handled automatically by SceneTransitionManager when the new scene loads
        isOpen = true;
        isAnimating = false;
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        if (fadeCanvas == null)
        {
            yield break;
        }

        float startAlpha = fadeCanvas.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            fadeCanvas.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        fadeCanvas.alpha = targetAlpha;
    }


    private void PlayClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 10f;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }
}

