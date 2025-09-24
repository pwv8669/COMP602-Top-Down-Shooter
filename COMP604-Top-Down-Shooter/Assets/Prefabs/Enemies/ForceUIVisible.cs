using UnityEngine;

public class ForceUIVisible : MonoBehaviour
{
    void Start()
    {
        // Force the canvas to be visible
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = true;
        }
        
        // Ensure CanvasGroup is fully visible
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = false; // Ensure it doesn't block clicks
        }
    }
}