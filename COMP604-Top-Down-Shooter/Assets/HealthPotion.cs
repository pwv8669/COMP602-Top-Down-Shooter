using UnityEngine;

public class HealthPotion : Potion
{
    private void Start()
    {
        potionName = "Health Potion";
        Setup3DAppearance();
    }
    
    private void Setup3DAppearance()
    {
        // Scale down the potion
        transform.localScale = Vector3.one * 0.5f; 
        
        // Rotate to lay flat or at an angle so it's visible from top-down view
        // For a bottle lying on its side:
        transform.rotation = Quaternion.Euler(90f, 0f, 45f);
    }
    
    public override void ApplyEffect(Health playerHealth)
    {
        playerHealth.Heal(healAmount);
    }
}