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
        transform.localScale = Vector3.one * 0.5f; 
        transform.rotation = Quaternion.Euler(90f, 0f, 45f);
    }
    
    public override void ApplyEffect(Health playerHealth)
    {
        Debug.Log($"Applying health potion effect: +{healAmount} health");
        Debug.Log($"Health before heal: {playerHealth.CurrentHealth}");
        playerHealth.Heal(healAmount);
        Debug.Log($"Health after heal: {playerHealth.CurrentHealth}");
    }
}