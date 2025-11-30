using UnityEngine;

public class StrikeTrigger : MonoBehaviour
{
    public PlankSwing plankSwing;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            plankSwing.StartSwing();
        }
    }
}