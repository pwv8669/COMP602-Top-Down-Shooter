using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleHealthTest : MonoBehaviour
{
    public Health health;
    
    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            Debug.Log("H key pressed!");
            if (health != null)
            {
                Debug.Log($"Health before damage: {health.CurrentHealth}");
                health.TakeDamage(10);
                Debug.Log($"Health after damage: {health.CurrentHealth}");
            }
            else
            {
                Debug.LogError("Health component is null!");
            }
        }
    }
}