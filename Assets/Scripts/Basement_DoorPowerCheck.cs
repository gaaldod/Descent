using UnityEngine;

public class DoorIndicatorLight : MonoBehaviour
{
    public bool powered = false;        // set this from your generator
    public float blinkSpeed = 3f;       // speed of red blinking
    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;  // get material instance
    }

    void Update()
    {
        if (powered)
        {
            // Solid green emission
            Color green = Color.green * 3f;
            mat.SetColor("_EmissionColor", green);
        }
        else
        {
            // Blinking red emission
            float blink = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color redBlink = Color.red * 3f * blink;
            mat.SetColor("_EmissionColor", redBlink);
        }
    }
}
