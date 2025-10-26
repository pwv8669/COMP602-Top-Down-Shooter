using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

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
        PhotonView photonView = GetComponent<PhotonView>();
        if (photonView != null && PhotonNetwork.IsConnected)
        {
            // Only the owner handles the death
            if (photonView.IsMine)
            {
                // Notify all clients that this player died
                photonView.RPC("RPC_Die", RpcTarget.AllBuffered);

                // Owner destroys their own object
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            // Singleplayer
            LocalDie();
        }
    }

    [PunRPC]
    private void RPC_Die()
    {
        // This runs on ALL clients (including the one who died)
        OnDied?.Invoke();
        Debug.Log(gameObject.name + " died!");

    }

    private void LocalDie()
    {
        OnDied?.Invoke();
        Debug.Log(gameObject.name + " died!");
        Destroy(gameObject);
    }
}