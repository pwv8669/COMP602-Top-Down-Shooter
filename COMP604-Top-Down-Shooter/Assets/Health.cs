using UnityEngine;
using UnityEngine.Events;
using Photon.Pun; // Soyun add it for multiplay

public class Health : MonoBehaviourPun // Soyun modified it MonoBehaviour -> MonoBehaviourPun
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
        /*
        // Clamp the health so it never goes below 0 or above maxHealth
        CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, 0, maxHealth);

        // Let's player know the health has changed 
        OnHealthChanged?.Invoke(CurrentHealth);

        // Check if player is out of health
        if (CurrentHealth <= 0)
        {
            Die();
        }*/

        // Soyun modified that if there is photonview
        if (photonView != null)
        {
            // Only my character will get damage
            if (photonView.IsMine)
            {
                //  Sychronizing by RPC
                photonView.RPC("RPC_TakeDamage", RpcTarget.AllBuffered, damageAmount);
            }
            // Ignore other player
        }
        else
        {
            // No PhotonView
            ApplyDamage(damageAmount);
        }
    }

    // Soyun add: Real damage application via RPC
    [PunRPC]
    private void RPC_TakeDamage(int damageAmount)
    {
        ApplyDamage(damageAmount);
    }

    // Soyun add for seperate RPC method
    private void ApplyDamage(int damageAmount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth - damageAmount, 0, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage. Health: {CurrentHealth}/{maxHealth}");

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

        // Soyun add for networked death handling
        if (photonView != null && photonView.IsMine)
        {
            // Player dead
            gameObject.SetActive(false);
        }
        else if (photonView == null)
        {
            // Destroy enemy object after short delay
            Destroy(gameObject, 0.5f);
        }
    }

}