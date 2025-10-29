using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    // public events so other scripts can listen for them without direct reference
    public UnityEvent<int> OnHealthChanged; // Int event for passing current health
    public UnityEvent OnDied; // Simple event for death

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool isPlayer = false;

    [Header("Effects")]
    [SerializeField] private GameObject bloodEffectPrefab; // Reference to your blood particle prefab

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

    // This method now accepts a position for where the damage occurred
    public void TakeDamage(int damageAmount, Vector3 hitPosition = default)
    {
        // Instantiate the blood particle effect at the point of impact
        if (bloodEffectPrefab != null && hitPosition != default)
        {
            Instantiate(bloodEffectPrefab, hitPosition, Quaternion.Euler(90, 0, 0));
        }

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

    // Healing functionality
    public void Heal(int healAmount)
    {
        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount, 0, maxHealth);
        
        // Only invoke event if health actually changed
        if (CurrentHealth != oldHealth)
        {
            Debug.Log($"Healing: {oldHealth} -> {CurrentHealth} (+{healAmount})");
            OnHealthChanged?.Invoke(CurrentHealth);
        }
        else
        {
            Debug.Log($"Healing had no effect (already at max health: {CurrentHealth}/{maxHealth})");
        }
    }

    private void Die()
    {
        // Log and disable the object
        Debug.Log(gameObject.name + " died!");

        OnDied?.Invoke();

        if (isPlayer && DeathManager.Instance != null)
        {
            DeathManager.Instance.ShowDeathScreen();
        }
        
        Destroy(gameObject);
    }
}