using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private bool isPlayerHealthBar = false;
    [SerializeField] private bool showOnlyWhenDamaged = false;

    private CanvasGroup canvasGroup;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initialize with correct visibility
        InitializeHealthBarVisibility();
        
        if (health != null)
        {
            health.OnHealthChanged.AddListener(UpdateHealthBar);
            health.OnDied.AddListener(OnEntityDied);
            UpdateHealthBar(health.CurrentHealth);
        }
        else
        {
            Debug.LogError("Health component not found on " + gameObject.name);
        }
    }

    private void InitializeHealthBarVisibility()
    {
        if (showOnlyWhenDamaged && !isPlayerHealthBar)
        {
            // Force hide enemy health bars at start
            canvasGroup.alpha = 0;
            Debug.Log("Enemy health bar hidden at start");
        }
        else
        {
            canvasGroup.alpha = 1;
        }
    }

    private void UpdateHealthBar(int currentHealth)
    {
        Debug.Log($"Health bar updating to: {currentHealth}");
        
        if (healthFillImage != null && health != null)
        {
            float healthPercentage = (float)currentHealth / health.MaxHealth;
            healthFillImage.fillAmount = healthPercentage;
            Debug.Log($"Fill amount set to: {healthPercentage}");
        }

        // Show health bar if entity is damaged and configured to show only when damaged
        if (showOnlyWhenDamaged && !isPlayerHealthBar && currentHealth < health.MaxHealth)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 1;
        }
    }

    // Hides health bar when entity dies
    private void OnEntityDied()
    {
        // Ensure health bar shows empty
        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = 0;
        }
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(UpdateHealthBar);
            health.OnDied.RemoveListener(OnEntityDied);
        }
    }

    private void Update()
    {
        // Additional check to ensure enemy health bars stay hidden when at full health
        if (showOnlyWhenDamaged && !isPlayerHealthBar && health != null)
        {
            if (health.CurrentHealth >= health.MaxHealth && canvasGroup.alpha > 0)
            {
                canvasGroup.alpha = 0;
            }
        }
    }
}