using UnityEngine;
using UnityEngine.Events; 

public class Health : MonoBehaviour
{
    // public events so other scripts can listen for them without direct reference
    public UnityEvent<int> OnHealthChanged; // Int event for passing current health
    public UnityEvent OnDied; // Simple event for death

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    public int CurrentHealth { get; private set; }

    public int MaxHealth => maxHealth;

    private void Start()
    {
        CurrentHealth = maxHealth;
        Debug.Log($"{gameObject.name} health initialized: {CurrentHealth}/{maxHealth}");
        
        // tiny delay to ensure HealthBar is ready
        Invoke(nameof(InitializeHealth), 0.01f);
    }

private void InitializeHealth()
{
    // Trigger the event to update any UI that might be listening
    OnHealthChanged?.Invoke(CurrentHealth);
}

    public void TakeDamage(int damageAmount)
    {
        // Clamp the health so it never goes below 0 or above maxHealth
        CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, 0, maxHealth);

        // Let's player know the health has changed 
        OnHealthChanged?.Invoke(CurrentHealth);

        // Check if player is out of health
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDied?.Invoke();

        // Log and disable the object
        Debug.Log(gameObject.name + " died!");
    }
}