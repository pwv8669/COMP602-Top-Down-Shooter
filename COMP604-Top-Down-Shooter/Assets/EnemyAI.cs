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

    // Soyun add: Target update
    private float targetUpdateInterval = 0.5f;
    private float lastTargetUpdateTime;

    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();

        FindClosestPlayer(); ; // Soyun add: Find the player target at start
    }

    private void Update()
    {
        //Soyun add: Update sight and attack range checks
        if (Time.time - lastTargetUpdateTime > targetUpdateInterval)
        {
            FindClosestPlayer();
            lastTargetUpdateTime = Time.time;
        }

        if (Target == null)
        {
            FindClosestPlayer();
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

    // Soyun add: Method to find the closest player target
    private void FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0)
        {
            Target = null;
            return;
        }

        Transform closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        Target = closestPlayer;
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
