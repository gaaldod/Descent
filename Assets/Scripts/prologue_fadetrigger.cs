using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class FadeTrigger : MonoBehaviour
{
    public CinemachineCamera vcam;   // the vcam with noise
    public CanvasGroup fadeCanvas;          // UI panel for fade
    public float fadeDuration = 1.5f;

    public AudioSource runningAudio;        // footsteps loop
    public AudioClip thudClip;              // thud sound file

    private AudioSource thudAudio;          // auto-created source for the thud
    private bool triggered = false;

    private void Start()
    {
        // Create AudioSource for the thud sound
        thudAudio = gameObject.AddComponent<AudioSource>();
        thudAudio.playOnAwake = false;
        thudAudio.spatialBlend = 0f;  // 2D sound
        thudAudio.volume = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;

        // Stop running sound
        if (runningAudio != null)
            runningAudio.Stop();

        // Stop head bob (noise)
        if (vcam != null)
        {
            var noise = vcam.GetComponentInChildren<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                noise.AmplitudeGain = 0f;
                noise.FrequencyGain = 0f;
            }
        }

        // Play thud sound
        if (thudClip != null)
            thudAudio.PlayOneShot(thudClip);

        // Start fade
        StartCoroutine(FadeToBlack());
    }

    private IEnumerator FadeToBlack()
    {
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 1f;
    }
}
