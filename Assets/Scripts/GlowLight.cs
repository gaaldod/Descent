using UnityEngine;

[RequireComponent(typeof(Light))]
public class GlowLight : MonoBehaviour
{
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.4f;
    public float pulseSpeed = 2f;
    public bool activeOnStart = true;

    private Light cachedLight;
    private bool activeState;
    private float baseIntensity;

    private void Awake()
    {
        cachedLight = GetComponent<Light>();
        baseIntensity = cachedLight.intensity;
        activeState = activeOnStart;
    }

    private void Update()
    {
        if (!activeState)
        {
            cachedLight.intensity = baseIntensity;
            return;
        }

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        cachedLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, pulse);
    }

    public void SetActive(bool active)
    {
        activeState = active;
        if (!active)
        {
            cachedLight.intensity = baseIntensity;
        }
    }
}

