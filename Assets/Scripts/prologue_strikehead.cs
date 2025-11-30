using UnityEngine;
using System.Collections;

public class PlankSwing : MonoBehaviour
{
    public float swingAngle = 90f;
    public float swingSpeed = 3f;

    private bool swinging = false;
    private Quaternion startRot;
    private Quaternion endRot;

    void Start()
    {
        startRot = transform.localRotation;
        endRot = startRot * Quaternion.Euler(swingAngle, 0, 0) * Quaternion.Euler(0, 0, -swingAngle-40f);

    }

    public void StartSwing()
    {
        if (!swinging)
            StartCoroutine(Swing());
    }

    private IEnumerator Swing()
    {
        swinging = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * swingSpeed;
            transform.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        // optional: stop here or bounce back
    }
}
