using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform Target;
    public float AttackRange = 2f;

    private NavMeshAgent m_Agent;
    private float m_Distance;

    [Header("Attack")]
    public int damage = 10;
    public float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;

    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        
        // Auto-find player if target not assigned
        if (Target == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
            Debug.Log("Auto-assigned player as target: " + player.name);
        }
        else
        {
            Debug.LogError("No player found! Make sure your player has the 'Player' tag.");
            // Disable the script to stop errors
            enabled = false;
        }
    }

    private void Update()
    {
        // Don't run if no target
        if (Target == null) 
        {
            TryFindPlayerAgain();
            return;
        }
        
        m_Distance = Vector3.Distance(m_Agent.transform.position, Target.position);
        if (m_Distance < AttackRange)
        {
            m_Agent.isStopped = true;
            AttackPlayer();
        }
        else
        {
            m_Agent.isStopped = false;
            m_Agent.destination = Target.position;
        }
    }

    private void TryFindPlayerAgain()
    {
        // Try to find player every few seconds if missing
        if (Time.frameCount % 60 == 0) // Every ~1 second
        {
            FindPlayer();
        }
    }

    private void AttackPlayer()
    {
        // Make sure enemy stops moving
        m_Agent.SetDestination(transform.position);
        
        if (Target != null)
        {
            transform.LookAt(Target);

            if (!alreadyAttacked)
            {
                Health playerHealth = Target.GetComponent<Health>();
                if (playerHealth != null) 
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("Enemy attacked player for " + damage + " damage");
                }

                alreadyAttacked = true;
                Invoke(nameof(ResetAttack), timeBetweenAttacks);
            }
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}