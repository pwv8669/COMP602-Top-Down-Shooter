using UnityEngine;

public abstract class Potion : MonoBehaviour
{
    [SerializeField] public int healAmount; 
    [SerializeField] protected string potionName;
    public bool isLargePotion = false; 
    
    public abstract void ApplyEffect(Health playerHealth);
    
    public void Collect(Health playerHealth)
    {
        if (playerHealth != null)
        {
            ApplyEffect(playerHealth);
            Debug.Log($"{potionName} collected! Healing for {healAmount} health. New health: {playerHealth.CurrentHealth}");
        }
        else
        {
            Debug.LogError("Potion.Collect: playerHealth is null!");
        }
        
        // Notify spawner before destroying
        PotionSpawner spawner = FindObjectOfType<PotionSpawner>();
        if (spawner != null)
        {
            spawner.OnPotionCollected(isLargePotion);
        }
        
        Destroy(gameObject);
    }
}