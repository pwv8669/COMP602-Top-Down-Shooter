using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Health health;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private bool isPlayerHealthBar = false;
    [SerializeField] private bool showOnlyWhenDamaged = false;

    private CanvasGroup canvasGroup;
    private bool isInitialized = false;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Initialize visibility based on settings
        if (showOnlyWhenDamaged && !isPlayerHealthBar)
        {
            canvasGroup.alpha = 0;
            Debug.Log("Enemy health bar hidden at start");
        }
        else
        {
            canvasGroup.alpha = 1;
            Debug.Log("Player health bar visible at start");
        }

        // Try to initialize if health is already assigned (for enemies)
        if (health != null)
        {
            InitializeHealthBar();
        }
        // For player health bar, health will be assigned later by GameManager via SetHealth()
    }

    // Public method to set health reference and initialize
    public void SetHealth(Health newHealth)
    {
        if (health != null)
        {
            // Remove old listeners
            health.OnHealthChanged.RemoveListener(UpdateHealthBar);
            health.OnDied.RemoveListener(OnEntityDied);
        }

        health = newHealth;

        // MULTIPLAYER: Check if this health bar should be visible
        if (isPlayerHealthBar && PhotonNetwork.IsConnected)
        {
            PhotonView pv = health.GetComponent<PhotonView>();
            if (pv != null && !pv.IsMine)
            {
                // Hide health bar for other players
                canvasGroup.alpha = 0;
                Debug.Log($"HealthBar hidden for remote player {health.gameObject.name}");
                return; // Don't initialize listeners for other players
            }
        }

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (health == null || isInitialized) return;

        health.OnHealthChanged.AddListener(UpdateHealthBar);
        health.OnDied.AddListener(OnEntityDied);
        isInitialized = true;

        // Update to current health
        UpdateHealthBar(health.CurrentHealth);
        Debug.Log($"HealthBar initialized for {health.gameObject.name}");
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