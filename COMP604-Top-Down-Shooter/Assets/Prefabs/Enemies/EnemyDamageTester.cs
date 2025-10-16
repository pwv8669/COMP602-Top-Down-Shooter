using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyDamageTester : MonoBehaviour
{
    private Health enemyHealth;
    
    void Start()
    {
        enemyHealth = GetComponent<Health>();
        if (enemyHealth == null)
        {
            Debug.LogError("Health component not found on " + gameObject.name);
        }
        else
        {
            Debug.Log("EnemyDamageTester ready on " + gameObject.name);
        }
    }
    
    void Update()
    {
        // Press 'E' to damage enemy
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("E key pressed!");
            
            if (enemyHealth != null)
            {
                Debug.Log($"Damaging enemy. Current health: {enemyHealth.CurrentHealth}");
                enemyHealth.TakeDamage(10);
                Debug.Log($"Enemy health after damage: {enemyHealth.CurrentHealth}");
            }
            else
            {
                Debug.LogError("EnemyHealth is null!");
            }
        }
    }
}