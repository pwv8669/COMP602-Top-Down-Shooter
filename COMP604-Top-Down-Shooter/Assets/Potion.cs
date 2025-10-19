using UnityEngine;

public abstract class Potion : MonoBehaviour
{
    [SerializeField] public int healAmount; 
    [SerializeField] protected string potionName;
    
    public abstract void ApplyEffect(Health playerHealth);
    
    // This will be called when player touches the potion
    public void Collect(Health playerHealth)
    {
        ApplyEffect(playerHealth);
        Debug.Log($"{potionName} collected! Healing for {healAmount} health.");
        Destroy(gameObject);
    }
}