using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Persistent manager that handles scene transitions with fade effects.
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager instance;
    private CanvasGroup fadeCanvas;
    private float fadeDuration = 1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureFadeUI();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        // When a new scene loads, check if we need to fade out
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // If fade canvas is visible (alpha > 0), fade it out
        if (fadeCanvas != null && fadeCanvas.alpha > 0.1f)
        {
            StartCoroutine(FadeOut());
        }
    }

    public void FadeIn(float duration = 1f)
    {
        fadeDuration = duration;
        if (fadeCanvas != null)
        {
            StartCoroutine(FadeTo(1f));
        }
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.1f); // Small delay to ensure scene is fully loaded
        yield return FadeTo(0f);
    }

    private IEnumerator FadeTo(float targetAlpha)
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

    private void EnsureFadeUI()
    {
        if (fadeCanvas != null)
        {
            return;
        }

        // Try to find existing fade canvas
        CanvasGroup existingFade = FindFirstObjectByType<CanvasGroup>();
        if (existingFade != null && existingFade.name.Contains("Fade"))
        {
            fadeCanvas = existingFade;
            return;
        }

        // Create new fade canvas
        GameObject canvasObj = new GameObject("SceneTransitionFade");
        canvasObj.transform.SetParent(transform);
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it's on top
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        var fadeImage = new GameObject("Fade").AddComponent<Image>();
        fadeImage.transform.SetParent(canvas.transform, false);
        fadeImage.color = Color.black;
        RectTransform fadeRect = fadeImage.rectTransform;
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.offsetMin = Vector2.zero;
        fadeRect.offsetMax = Vector2.zero;

        fadeCanvas = fadeImage.gameObject.AddComponent<CanvasGroup>();
        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false; // Don't block input
    }

    public static SceneTransitionManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SceneTransitionManager");
                instance = go.AddComponent<SceneTransitionManager>();
            }
            return instance;
        }
    }

    public CanvasGroup FadeCanvas
    {
        get
        {
            if (instance != null && instance.fadeCanvas == null)
            {
                instance.EnsureFadeUI();
            }
            return instance != null ? instance.fadeCanvas : null;
        }
    }
}

