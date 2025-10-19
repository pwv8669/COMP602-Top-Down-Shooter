using UnityEngine;

public class HealthPotion : Potion
{
    private void Start()
    {
        potionName = "Health Potion";
    }
    
    public override void ApplyEffect(Health playerHealth)
    {
        playerHealth.Heal(healAmount);
    }
}