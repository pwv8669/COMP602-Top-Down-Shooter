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

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

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
        // In multiplayer, only owner can modify health
        if (photonView != null && PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        // Clamp the health so it never goes below 0 or above maxHealth
        CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, 0, maxHealth);

        // Let's player know the health has changed 
        OnHealthChanged?.Invoke(CurrentHealth);

        // Sync health to other clients in multiplayer
        if (photonView != null && PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_SyncHealth", RpcTarget.OthersBuffered, CurrentHealth);
        }

        // Check if player is out of health
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // Healing functionality
    public void Heal(int healAmount)
    {
        // In multiplayer, only owner can modify health
        if (photonView != null && PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
        }

        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount, 0, maxHealth);

        // Only invoke event if health actually changed
        if (CurrentHealth != oldHealth)
        {
            Debug.Log($"Healing: {oldHealth} -> {CurrentHealth} (+{healAmount})");
            OnHealthChanged?.Invoke(CurrentHealth);

            // Sync health to other clients in multiplayer
            if (photonView != null && PhotonNetwork.IsConnected)
            {
                photonView.RPC("RPC_SyncHealth", RpcTarget.OthersBuffered, CurrentHealth);
            }
        }
        else
        {
            Debug.Log($"Healing had no effect (already at max health: {CurrentHealth}/{maxHealth})");
        }
    }

    [PunRPC]
    private void RPC_SyncHealth(int newHealth)
    {
        CurrentHealth = newHealth;
        OnHealthChanged?.Invoke(CurrentHealth);

        if (CurrentHealth <= 0)
        {
            OnDied?.Invoke();
        }
    }

    private void Die()
    {
        // FIXED: Use existing photonView field instead of GetComponent again
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