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

        FindTarget(); // Soyun add: Find the player target at start
    }

    private void Update()
    {
        //Soyun add: Update sight and attack range checks
        if (Target == null)
        {
            FindTarget();
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

    // Soyun add: Method to find the player target
    private void FindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Target = player.transform;
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
