using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight singleton UI used to display lore notes and small bits of text.
/// The canvas is spawned automatically on first use so no manual scene wiring is required.
/// </summary>
public class NoteDisplayUI : MonoBehaviour
{
    private static NoteDisplayUI instance;

    private CanvasGroup canvasGroup;
    private Text noteText;
    private Coroutine fadeRoutine;

    public static NoteDisplayUI Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("NoteDisplayUI");
                instance = go.AddComponent<NoteDisplayUI>();
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
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        GameObject background = new GameObject("NoteBackground");
        background.transform.SetParent(transform, false);
        var bgImage = background.AddComponent<Image>();
        bgImage.color = Color.white; // White background
        var bgRect = bgImage.rectTransform;
        bgRect.anchorMin = new Vector2(0.2f, 0.2f);
        bgRect.anchorMax = new Vector2(0.8f, 0.65f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        GameObject textObj = new GameObject("NoteText");
        textObj.transform.SetParent(background.transform, false);
        noteText = textObj.AddComponent<Text>();
        noteText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        noteText.fontSize = 24;
        noteText.color = Color.black; // Black text on white background
        noteText.alignment = TextAnchor.UpperLeft;
        noteText.supportRichText = true;
        var textRect = noteText.rectTransform;
        textRect.anchorMin = new Vector2(0.05f, 0.08f);
        textRect.anchorMax = new Vector2(0.95f, 0.92f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }

    public void Show(string text)
    {
        if (noteText == null)
        {
            return;
        }

        noteText.text = text;
        canvasGroup.blocksRaycasts = true;
        StartFade(1f, 0.2f);
    }
    
    private void Update()
    {
        // Allow player to close note by pressing E or Escape
        if (canvasGroup != null && canvasGroup.alpha > 0.5f && canvasGroup.blocksRaycasts)
        {
            if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }
    }

    public void Hide()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.blocksRaycasts = false;
        StartFade(0f, 0.25f);
    }

    private void StartFade(float target, float duration)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(target, duration));
    }

    private IEnumerator FadeRoutine(float target, float duration)
    {
        float elapsed = 0f;
        float start = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = duration > 0f ? elapsed / duration : 1f;
            canvasGroup.alpha = Mathf.Lerp(start, target, t);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}

