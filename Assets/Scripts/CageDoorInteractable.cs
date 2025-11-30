using System.Collections;
using UnityEngine;

public class CageDoorInteractable : Interactable
{
    [Header("Door Requirements")]
    public string requiredKeyName = "Cage Key";
    public bool consumeKeyOnUse = true;

    [Header("Animation")]
    public Vector3 openOffset = new Vector3(1.2f, 0f, 0f);
    public float openDuration = 1.5f;

    [Header("Audio")]
    public AudioClip lockedClip;
    public AudioClip openClip;

    private Vector3 closedPosition;
    private bool isOpen;
    private bool isAnimating;
    private AudioSource audioSource;

    private void Awake()
    {
        closedPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();

        if (string.IsNullOrEmpty(interactionText))
        {
            interactionText = "open cage door";
        }
    }

    public override void Interact()
    {
        if (isAnimating || isOpen)
        {
            return;
        }

        if (DevionGamesInventoryWrapper.HasItem(requiredKeyName))
        {
            if (consumeKeyOnUse)
            {
                DevionGamesInventoryWrapper.RemoveItem(requiredKeyName);
            }

            StartCoroutine(OpenDoorRoutine());
        }
        else
        {
            PlayClip(lockedClip);
        }
    }

    private IEnumerator OpenDoorRoutine()
    {
        isAnimating = true;
        Vector3 openTarget = closedPosition + openOffset;

        float elapsed = 0f;
        PlayClip(openClip);

        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openDuration);
            transform.localPosition = Vector3.Lerp(closedPosition, openTarget, t);
            yield return null;
        }

        transform.localPosition = openTarget;
        isOpen = true;
        isAnimating = false;
        canInteract = false;
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

