using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SimpleOutline : MonoBehaviour
{
    public Color emissionColor = Color.white;
    public float pulseSpeed = 3f;
    public float intensityMultiplier = 2f;

    private Renderer cachedRenderer;
    private MaterialPropertyBlock propertyBlock;
    private bool highlighted;

    private void Awake()
    {
        cachedRenderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        SetHighlighted(false);
    }

    private void Update()
    {
        if (!highlighted)
        {
            return;
        }

        float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
        Color color = emissionColor * (pulse * intensityMultiplier);
        propertyBlock.SetColor("_EmissionColor", color);
        cachedRenderer.SetPropertyBlock(propertyBlock);
    }

    public void SetHighlighted(bool value)
    {
        highlighted = value;

        if (!highlighted)
        {
            propertyBlock.SetColor("_EmissionColor", Color.black);
            cachedRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}

