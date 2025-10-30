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

    [Header("States")]
    public float sightRange = 15f;
    
    private bool playerInSightRange, playerInAttackRange;

    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
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

    private void AttackPlayer()
    {
        // Make sure enemy stops moving
        m_Agent.SetDestination(transform.position);
        transform.LookAt(Target);

        if (!alreadyAttacked)
        {
            Health playerHealth = Target.GetComponent<Health>();
            if (playerHealth != null) playerHealth.TakeDamage(damage);

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
