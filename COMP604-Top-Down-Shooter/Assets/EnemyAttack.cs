using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    
    private Transform player;
    private float lastAttackTime;
    private Health playerHealth;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<Health>();
        }
        else
        {
            Debug.LogWarning("Player not found with tag 'Player'. Make sure your player has the 'Player' tag.");
        }
    }

    void Update()
    {
        if (player == null || playerHealth == null) return;
        
        // Check if player is in range
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
        }
    }

    void AttackPlayer()
    {
        playerHealth.TakeDamage(damage);
        lastAttackTime = Time.time;
        Debug.Log($"Enemy attacked player for {damage} damage. Player health: {playerHealth.CurrentHealth}");
    }
}