using UnityEngine;

public class CandleAutoSetup : MonoBehaviour
{
    public Material flameMaterial;   // Drag your firecandle material here
    public Vector3 flameOffset = new Vector3(0, 0.2f, 0);

    void Start()
    {
        // Create flame quad
        GameObject flame = GameObject.CreatePrimitive(PrimitiveType.Quad);
        flame.transform.SetParent(transform, false);
        flame.transform.localPosition = flameOffset;
        flame.transform.localRotation = Quaternion.identity;
        flame.transform.localScale = new Vector3(0.15f, 0.25f, 1);

        // Assign material
        flame.GetComponent<MeshRenderer>().material = flameMaterial;

        // Add billboard behavior
        flame.AddComponent<Billboard>();

        // Add point light
        Light l = flame.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = 2f;
        l.intensity = 2f;
        l.color = new Color(1.0f, 0.75f, 0.45f);

        // Add flicker script
        var flicker = flame.AddComponent<LightFlicker>();
    }
}

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform);
    }
}

public class LightFlicker : MonoBehaviour
{
    Light l;
    void Awake() { l = GetComponent<Light>(); }

    void Update()
    {
        float n = Mathf.PerlinNoise(Time.time * 3f, 0f);
        l.intensity = Mathf.Lerp(1.5f, 2.5f, n);
    }
}
