using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSequenceTrigger : MonoBehaviour
{
    [Header("Movement")]
    public float walkDuration = 5f;
    public float walkSpeed = 2f;
    public Transform lookDirection;

    [Header("UI / Fade")]
    public CanvasGroup fadeCanvas;
    public Text endMessageText;
    public string finalMessage = "Thanks for playing!";
    public float fadeDuration = 2f;
    public float messageHoldTime = 3f;

    [Header("Flow")]
    public string mainMenuScene = "MainMenu";

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        StartCoroutine(PlayEndingSequence(other.gameObject));
    }

    private IEnumerator PlayEndingSequence(GameObject player)
    {
        EnsureUI();

        var controller = player.GetComponent<FirstPersonController>();
        var rigidbody = player.GetComponent<Rigidbody>();

        if (controller != null)
        {
            controller.playerCanMove = false;
            controller.cameraCanMove = false;
        }

        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        if (lookDirection != null)
        {
            player.transform.rotation = Quaternion.LookRotation(lookDirection.forward, Vector3.up);
        }

        float elapsed = 0f;
        Vector3 forward = player.transform.forward;

        while (elapsed < walkDuration)
        {
            elapsed += Time.deltaTime;
            player.transform.position += forward * walkSpeed * Time.deltaTime;
            yield return null;
        }

        yield return FadeCanvas(1f);

        if (endMessageText != null)
        {
            endMessageText.text = finalMessage;
            endMessageText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(messageHoldTime);

        if (!string.IsNullOrEmpty(mainMenuScene))
        {
            SceneManager.LoadScene(mainMenuScene);
        }
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

    private void EnsureUI()
    {
        if (fadeCanvas != null && endMessageText != null)
        {
            return;
        }

        GameObject canvasObj = new GameObject("EndingCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
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

        GameObject textObj = new GameObject("EndingMessage");
        textObj.transform.SetParent(canvas.transform, false);
        endMessageText = textObj.AddComponent<Text>();
        endMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        endMessageText.fontSize = 36;
        endMessageText.alignment = TextAnchor.MiddleCenter;
        endMessageText.color = Color.white;
        endMessageText.gameObject.SetActive(false);

        RectTransform textRect = endMessageText.rectTransform;
        textRect.anchorMin = new Vector2(0.1f, 0.4f);
        textRect.anchorMax = new Vector2(0.9f, 0.6f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
}

