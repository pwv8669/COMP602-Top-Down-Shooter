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
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is a potion
        Potion potion = other.GetComponent<Potion>();
        if (potion != null && playerHealth != null)
        {
            potion.Collect(playerHealth);
        }
    }
}