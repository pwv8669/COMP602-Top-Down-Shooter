using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;

public class Health : MonoBehaviourPunCallbacks, IPunObservable
{
    // Public events for UI and other systems
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnDied;

    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

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

    // ===== MULTIPLAYER: Call this to deal damage =====
    // attackerIsEnemy: Set to true when enemy attacks player
    public void TakeDamage(int damageAmount, bool attackerIsEnemy = false)
    {
        if (isDead) return;

        PhotonView pv = GetComponent<PhotonView>();

        if (pv != null && PhotonNetwork.IsConnected)
        {
            // MULTIPLAYER MODE
            if (CompareTag("Player"))
            {
                // PLAYER: Check who is dealing damage
                if (attackerIsEnemy)
                {
                    // ENEMY ATTACK: Host sends damage to player owner
                    if (PhotonNetwork.IsMasterClient)
                    {
                        // Host tells the player owner to take damage
                        photonView.RPC(nameof(RPC_TakeDamage), pv.Owner, damageAmount);
                    }
                }
                else
                {
                    // PLAYER DAMAGE (from gun, etc): Only owner can modify their own health
                    if (pv.IsMine)
                    {
                        RPC_TakeDamage(damageAmount);
                        photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.Others, damageAmount);
                    }
                }
            }
            else
            {
                // ENEMY: Anyone can damage, but send to MasterClient for authority
                // Client shoots enemy ¡æ tells host ¡æ host applies damage ¡æ syncs to all
                if (PhotonNetwork.IsMasterClient)
                {
                    // Host directly applies damage
                    RPC_TakeDamage(damageAmount);
                    photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.Others, damageAmount);
                }
                else
                {
                    // Client requests damage from host
                    photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, damageAmount);
                }
            }
        }
        else
        {
            // SINGLEPLAYER MODE: Direct call
            RPC_TakeDamage(damageAmount);
        }
    }

    // ===== RPC: Client requests host to apply damage to enemy =====
    [PunRPC]
    private void RPC_RequestDamage(int damageAmount)
    {
        // Only host processes this request
        if (PhotonNetwork.IsMasterClient && !isDead)
        {
            RPC_TakeDamage(damageAmount);
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.Others, damageAmount);
        }
    }

    // ===== MULTIPLAYER: Call this to heal =====
    public void Heal(int healAmount)
    {
        if (isDead) return;

        PhotonView pv = GetComponent<PhotonView>();

        if (pv != null && PhotonNetwork.IsConnected)
        {
            // MULTIPLAYER MODE
            if (CompareTag("Player"))
            {
                // PLAYER: Only owner can heal themselves
                if (pv.IsMine)
                {
                    RPC_Heal(healAmount);
                    photonView.RPC(nameof(RPC_Heal), RpcTarget.Others, healAmount);
                }
            }
            else
            {
                // ENEMY: Host manages enemy healing (rare case)
                if (PhotonNetwork.IsMasterClient)
                {
                    RPC_Heal(healAmount);
                    photonView.RPC(nameof(RPC_Heal), RpcTarget.Others, healAmount);
                }
                else
                {
                    // Client requests heal from host
                    photonView.RPC(nameof(RPC_RequestHeal), RpcTarget.MasterClient, healAmount);
                }
            }
        }
        else
        {
            // SINGLEPLAYER MODE: Direct call
            RPC_Heal(healAmount);
        }
    }

    // ===== RPC: Client requests host to heal enemy =====
    [PunRPC]
    private void RPC_RequestHeal(int healAmount)
    {
        // Only host processes this request
        if (PhotonNetwork.IsMasterClient && !isDead)
        {
            RPC_Heal(healAmount);
            photonView.RPC(nameof(RPC_Heal), RpcTarget.Others, healAmount);
        }
    }

    // ===== RPC: Actual damage logic =====
    [PunRPC]
    private void RPC_TakeDamage(int damageAmount)
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

    // ===== RPC: Actual heal logic =====
    [PunRPC]
    private void RPC_Heal(int healAmount)
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
        else
        {
            Debug.Log($"Healing had no effect (already at max health: {CurrentHealth}/{maxHealth})");
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
            RPC_Die();
            Destroy(gameObject);
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