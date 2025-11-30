using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interactable generator: requires a fuel item, consumes it, and toggles power once activated.
/// </summary>
public class GeneratorInteractable : Interactable
{
    [Header("Fuel Requirement")]
    public string requiredItemName = "Gas Can";
    public string emptyFuelMessage = "Out of fuel.";

    [Header("Feedback")]
    public AudioSource runningLoop;
    public AudioSource feedbackAudio;
    public AudioClip startClip;
    public AudioClip noFuelClip;
    public ParticleSystem activationFx;

    [Header("Power Targets")]
    public DoorIndicatorLight[] indicatorLights;
    public GameObject[] enableOnPower;
    public Door[] doorsToUnlock;

    public UnityEvent onGeneratorStarted;
    public UnityEvent onGeneratorNoFuel;

    private bool isRunning;

    private void Reset()
    {
        interactionText = "refuel generator";
    }

    public override void Interact()
    {
        if (isRunning)
        {
            Debug.Log("Generator already running.");
            return;
        }

        if (!DevionGamesInventoryWrapper.HasItem(requiredItemName))
        {
            HandleNoFuel();
            return;
        }

        DevionGamesInventoryWrapper.RemoveItem(requiredItemName);
        ActivateGenerator();
    }

    private void ActivateGenerator()
    {
        isRunning = true;
        interactionText = "generator humming";
        canInteract = false;

        if (feedbackAudio != null && startClip != null)
        {
            feedbackAudio.PlayOneShot(startClip);
        }

        if (runningLoop != null)
        {
            runningLoop.loop = true;
            runningLoop.Play();
        }

        if (activationFx != null)
        {
            activationFx.Play();
        }

        if (indicatorLights != null)
        {
            foreach (DoorIndicatorLight indicator in indicatorLights)
            {
                if (indicator != null)
                {
                    indicator.powered = true;
                }
            }
        }

        if (enableOnPower != null)
        {
            foreach (GameObject go in enableOnPower)
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }
        }

        if (doorsToUnlock != null)
        {
            foreach (Door door in doorsToUnlock)
            {
                if (door != null)
                {
                    door.canInteract = true;
                    door.Interact();
                }
            }
        }

        onGeneratorStarted?.Invoke();
    }

    private void HandleNoFuel()
    {
        if (feedbackAudio != null && noFuelClip != null)
        {
            feedbackAudio.PlayOneShot(noFuelClip);
        }

        InteractionSystem.Instance?.ShowNotification(emptyFuelMessage);

        onGeneratorNoFuel?.Invoke();
    }
}

