using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class IntroDescriptionController : MonoBehaviour
{
    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public TextMeshProUGUI bodyText;

    [Header("Narration")]
    [TextArea(3, 8)]
    public string[] paragraphs;
    public float paragraphDuration = 7f;
    public float paragraphFadeDuration = 1f;
    public float fadeDuration = 1f;
    public KeyCode continueKey = KeyCode.Space;

    [Header("Flow")]
    public string nextSceneName = "IntroScene";

    private bool sequenceRunning;
    private Coroutine playbackRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
        }

        if (bodyText == null && canvasGroup != null)
        {
            bodyText = canvasGroup != null ? canvasGroup.GetComponentInChildren<TextMeshProUGUI>() : null;
        }
    }

    private void Start()
    {
        SetCursorState(false);
        playbackRoutine = StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        if (sequenceRunning && Input.GetKeyDown(continueKey))
        {
            SkipToEnd();
        }
    }

    private IEnumerator PlaySequence()
    {
        sequenceRunning = true;
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        if (bodyText != null)
        {
            Color textColor = bodyText.color;
            textColor.a = 0f;
            bodyText.color = textColor;
        }

        // Fade in
        yield return FadeCanvas(startAlpha, 1f);

        if (paragraphs == null || paragraphs.Length == 0)
        {
            paragraphs = new[]
            {
                "After a long shift, Dan usually goes for a quick run in the forest behind his house",
                "Even the thick fog of the night didn't stop him",
                "But on this evening, something is different..."
            };
        }

        foreach (string paragraph in paragraphs)
        {
            if (bodyText != null)
            {
                if (!string.IsNullOrEmpty(bodyText.text))
                {
                    yield return FadeTextAlpha(0f);
                }

                bodyText.text = paragraph;
                yield return FadeTextAlpha(1f);
            }

            yield return new WaitForSeconds(paragraphDuration);
        }

        yield return FadeTextAlpha(0f);

        // Fade out and continue
        yield return FadeCanvas(1f, 0f);
        sequenceRunning = false;
        LoadNextScene();
    }

    private void SkipToEnd()
    {
        if (playbackRoutine != null)
        {
            StopCoroutine(playbackRoutine);
        }

        sequenceRunning = false;
        LoadNextScene();
    }

    private IEnumerator FadeCanvas(float from, float to)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private IEnumerator FadeTextAlpha(float targetAlpha)
    {
        if (bodyText == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, paragraphFadeDuration);
        Color color = bodyText.color;
        float startAlpha = color.a;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            bodyText.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        bodyText.color = color;
    }

    private void LoadNextScene()
    {
        SetCursorState(true);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

