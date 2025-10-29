using UnityEngine;

public class PotionCollector : MonoBehaviour
{
    private Health playerHealth;
    
    private void Start()
    {
        playerHealth = GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError("PotionCollector: No Health component found on player!");
        }
    }
    
    // 3D collision detection
    private void OnTriggerEnter(Collider other) // Collider
    {
        // Check if the collided object is a potion
        Potion potion = other.GetComponent<Potion>();
        if (potion != null && playerHealth != null)
        {
            potion.Collect(playerHealth);
            Debug.Log($"Potion collected! Current health: {playerHealth.CurrentHealth}");
        }
    }
}