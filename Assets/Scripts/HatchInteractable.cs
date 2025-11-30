using System.Collections;
using UnityEngine;

/// <summary>
/// Hatch that teleports player to first floor and starts enemy AI after delay.
/// </summary>
public class HatchInteractable : Interactable
{
    [Header("Key Requirement")]
    public string requiredKeyName = "Attic Key";

    [Header("Teleport")]
    public Transform teleportDestination; // First floor at bottom of stairs

    [Header("Enemy AI")]
    public EnemyAI enemyAI;
    public float aiStartDelay = 10f; // Seconds before enemy AI starts

    [Header("Audio")]
    public AudioClip lockedClip;
    public AudioClip openClip;

    private bool hasActivated;
    private AudioSource audioSource;

    private void Awake()
    {
        if (string.IsNullOrEmpty(interactionText))
        {
            interactionText = "open hatch";
        }
    }

    public override string GetInteractionText()
    {
        if (hasActivated)
        {
            return "";
        }

        if (DevionGamesInventoryWrapper.HasItem(requiredKeyName))
        {
            return "open hatch";
        }
        else
        {
            return "hatch locked";
        }
    }

    public override void Interact()
    {
        if (hasActivated)
        {
            return;
        }

        // Check if player has the key
        if (!DevionGamesInventoryWrapper.HasItem(requiredKeyName))
        {
            PlayClip(lockedClip);
            if (InteractionSystem.Instance != null)
            {
                InteractionSystem.Instance.ShowNotification("The hatch is locked.");
            }
            return;
        }

        // Player has the key - proceed with teleport and AI activation
        StartCoroutine(ActivateHatchRoutine());
    }

    private IEnumerator ActivateHatchRoutine()
    {
        hasActivated = true;
        canInteract = false;

        // Play open sound
        PlayClip(openClip);

        // Find the player
        FirstPersonController player = FindFirstObjectByType<FirstPersonController>();
        if (player == null)
        {
            Debug.LogError("[HatchInteractable] Player not found!");
            yield break;
        }

        // Teleport player to destination
        if (teleportDestination != null)
        {
            player.transform.position = teleportDestination.position;
            player.transform.rotation = teleportDestination.rotation;
            
            Debug.Log($"[HatchInteractable] Player teleported to {teleportDestination.position}");
        }
        else
        {
            Debug.LogWarning("[HatchInteractable] Teleport destination not set!");
        }

        // Wait for the delay before starting enemy AI
        yield return new WaitForSeconds(aiStartDelay);

        // Enable/Start the enemy AI
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
            Debug.Log("[HatchInteractable] Enemy AI enabled after delay.");
        }
        else
        {
            Debug.LogWarning("[HatchInteractable] Enemy AI reference not set!");
        }
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
            audioSource.maxDistance = 12f;
        }

        audioSource.clip = clip;
        audioSource.Play();
    }
}

