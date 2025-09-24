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
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
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