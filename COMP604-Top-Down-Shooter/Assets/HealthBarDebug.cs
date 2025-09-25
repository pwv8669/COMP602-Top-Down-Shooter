using UnityEngine;
using UnityEngine.InputSystem;

public class HealthBarDebug : MonoBehaviour
{
    void Update()
    {
        // Press P to print health bar position info
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            Debug.Log($"Health Bar World Position: {transform.position}");
            Debug.Log($"Health Bar Local Position: {transform.localPosition}");
            Debug.Log($"Health Bar Scale: {transform.localScale}");
            Debug.Log($"Health Bar Active: {gameObject.activeInHierarchy}");
            
            // Check if visible to camera
            if (Camera.main != null)
            {
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
                Debug.Log($"Health Bar Viewport Position: {viewportPos}");
                Debug.Log($"Is Health Bar in camera view: {viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1}");
            }
        }
    }
}