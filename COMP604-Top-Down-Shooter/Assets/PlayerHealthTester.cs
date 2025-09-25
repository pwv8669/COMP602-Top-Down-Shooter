using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHealthTester : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private int damageAmount = 10;

    void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Player took {damageAmount} damage. Current health: {playerHealth.CurrentHealth}");
            }
        }
    }
}