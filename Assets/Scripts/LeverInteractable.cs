using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class LeverInteractable : Interactable
{
    [Header("Lever Settings")]
    public Transform handle;
    public Vector3 pulledEuler = new Vector3(-50f, 0f, 0f);
    public float pullDuration = 0.4f;

    [Header("Audio")]
    public AudioClip leverSound;

    [Header("Events")]
    public UnityEvent onLeverPulled = new UnityEvent();

    private Quaternion defaultRotation;
    private bool hasBeenPulled;
    private AudioSource audioSource;

    private void Awake()
    {
        interactionText = "pull lever";
        if (handle == null)
        {
            handle = transform;
        }
        defaultRotation = handle.localRotation;
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    public override void Interact()
    {
        if (hasBeenPulled)
        {
            return;
        }

        hasBeenPulled = true;
        canInteract = false;
        StartCoroutine(AnimateLever());
        onLeverPulled?.Invoke();
    }

    private IEnumerator AnimateLever()
    {
        PlaySound();

        Quaternion target = Quaternion.Euler(pulledEuler);
        float elapsed = 0f;

        while (elapsed < pullDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / pullDuration);
            handle.localRotation = Quaternion.Slerp(defaultRotation, target, t);
            yield return null;
        }

        handle.localRotation = target;
    }

    private void PlaySound()
    {
        if (leverSound == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 0.5f;
            audioSource.maxDistance = 8f;
        }

        audioSource.clip = leverSound;
        audioSource.Play();
    }
}

