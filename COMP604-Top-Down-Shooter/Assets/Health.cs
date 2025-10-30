using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

/// <summary>
/// Simplified Health system for multiplayer
/// Rule: All damage/heal is processed by MasterClient for authority
/// </summary>
public class Health : MonoBehaviourPunCallbacks, IPunObservable
{
    // Public events for UI and other systems
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDied;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private bool isPlayer = false;

    [Header("Effects")]
    [SerializeField] private GameObject bloodEffectPrefab; // Reference to your blood particle prefab

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    private bool isDead = false;

    private void Start()
    {
        CurrentHealth = maxHealth;
        Debug.Log($"{gameObject.name} health initialized: {CurrentHealth}/{maxHealth}");

        // Tiny delay to ensure HealthBar is ready
        Invoke(nameof(InitializeHealth), 0.01f);
    }

    private void InitializeHealth()
    {
        // Trigger the event to update any UI that might be listening
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    // ===== PUBLIC API: Damage =====
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        // MULTIPLAYER: Route all damage through MasterClient
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Host: Apply damage directly
                ApplyDamage(damageAmount);
            }
            else
            {
                // Client: Request damage from host
                photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, damageAmount);
            }
        }
        else
        {
            // SINGLEPLAYER: Apply directly
            ApplyDamage(damageAmount);
        }
    }

    // ===== PUBLIC API: Heal =====
    public void Heal(int healAmount)
    {
        if (isDead) return;

        // MULTIPLAYER: Route all healing through MasterClient
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Host: Apply heal directly
                ApplyHeal(healAmount);
            }
            else
            {
                // Client: Request heal from host
                photonView.RPC(nameof(RPC_RequestHeal), RpcTarget.MasterClient, healAmount);
            }
        }
        else
        {
            // SINGLEPLAYER: Apply directly
            ApplyHeal(healAmount);
        }
    }

    // ===== RPC: Damage Request (Client -> Host) =====
    [PunRPC]
    private void RPC_RequestDamage(int damageAmount)
    {
        // Only host processes damage requests
        if (PhotonNetwork.IsMasterClient && !isDead)
        {
            ApplyDamage(damageAmount);
        }
    }

    // ===== RPC: Heal Request (Client -> Host) =====
    [PunRPC]
    private void RPC_RequestHeal(int healAmount)
    {
        // Only host processes heal requests
        if (PhotonNetwork.IsMasterClient && !isDead)
        {
            ApplyHeal(healAmount);
        }
    }

    // ===== RPC: Apply Damage (Host -> All) =====
    [PunRPC]
    private void RPC_ApplyDamage(int damageAmount)
    {
        if (isDead) return;

        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, 0, maxHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage: {oldHealth} -> {CurrentHealth}");

        // Notify UI
        OnHealthChanged?.Invoke(CurrentHealth);

        // Check death
        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    // ===== RPC: Apply Heal (Host -> All) =====
    [PunRPC]
    private void RPC_ApplyHeal(int healAmount)
    {
        if (isDead) return;

        int oldHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(CurrentHealth + healAmount, 0, maxHealth);

        // Only invoke event if health actually changed
        if (CurrentHealth != oldHealth)
        {
            Debug.Log($"{gameObject.name} healed: {oldHealth} -> {CurrentHealth} (+{healAmount})");
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }

    // ===== INTERNAL: Damage logic (Host only) =====
    private void ApplyDamage(int damageAmount)
    {
        // Host applies damage and syncs to all clients
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.All, damageAmount);
        }
        else
        {
            // Singleplayer: Direct call
            RPC_ApplyDamage(damageAmount);
        }
    }

    // ===== INTERNAL: Heal logic (Host only) =====
    private void ApplyHeal(int healAmount)
    {
        // Host applies heal and syncs to all clients
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_ApplyHeal), RpcTarget.All, healAmount);
        }
        else
        {
            // Singleplayer: Direct call
            RPC_ApplyHeal(healAmount);
        }
    }

    // ===== Death handling =====
    private void Die()
    {
        if (isDead) return;
        isDead = true;

        PhotonView pv = GetComponent<PhotonView>();

        if (pv != null && PhotonNetwork.IsConnected)
        {
            if (CompareTag("Player"))
            {
                // PLAYER: Only owner destroys their own player
                if (pv.IsMine)
                {
                    // Show death screen for local player only
                    if (DeathManager.Instance != null)
                    {
                        DeathManager.Instance.ShowDeathScreen();
                    }

                    photonView.RPC(nameof(RPC_Die), RpcTarget.AllBuffered);
                    Invoke(nameof(DestroyPlayer), 0.1f);
                }
            }
            else
            {
                // ENEMY: Only host destroys enemies
                if (PhotonNetwork.IsMasterClient)
                {
                    photonView.RPC(nameof(RPC_Die), RpcTarget.AllBuffered);
                    Invoke(nameof(DestroyEnemy), 0.1f);
                }
            }
        }
        else
        {
            // SINGLEPLAYER MODE: Direct death
            if (CompareTag("Player"))
            {
                // Show death screen in singleplayer
                if (DeathManager.Instance != null)
                {
                    DeathManager.Instance.ShowDeathScreen();
                }
            }

            RPC_Die();

            // Delay destruction so death screen can show
            if (CompareTag("Player"))
            {
                Invoke(nameof(DestroySingleplayer), 0.1f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    [PunRPC]
    private void RPC_Die()
    {
        // This runs on ALL clients (including the one who died)
        OnDied?.Invoke();
        Debug.Log($"{gameObject.name} died!");
    }

    private void DestroyPlayer()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void DestroyEnemy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void DestroySingleplayer()
    {
        Destroy(gameObject);
    }

    // ===== PHOTON: Continuous sync for health value =====
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send current health to other clients
            stream.SendNext(CurrentHealth);
            stream.SendNext(isDead);
        }
        else
        {
            // Receive health from other clients
            CurrentHealth = (int)stream.ReceiveNext();
            isDead = (bool)stream.ReceiveNext();

            // Update UI on remote clients
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
}