using System.Collections;
using UnityEngine;

public class ReadableNote : Interactable
{
    [TextArea(4, 10)]
    public string contents;

    [Tooltip("Seconds before the UI hides automatically. Set to zero to keep it open.")]
    public float autoHideDelay = 8f;

    private Coroutine hideRoutine;

    private void Reset()
    {
        interactionText = "read note";
    }

    public override void Interact()
    {
        if (string.IsNullOrEmpty(contents))
        {
            contents = "The ink is too faded to read.";
        }

        NoteDisplayUI.Instance.Show(contents);

        if (autoHideDelay > 0f)
        {
            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(autoHideDelay);
        NoteDisplayUI.Instance.Hide();
        hideRoutine = null;
    }
}

