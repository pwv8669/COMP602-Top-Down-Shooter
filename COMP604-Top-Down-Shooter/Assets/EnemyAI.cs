using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    public Transform Target;
    public float AttackRange = 2f;

    private NavMeshAgent m_Agent;
    private float m_Distance;
    private PhotonView photonView;

    [Header("Attack")]
    public int damage = 10;
    public float timeBetweenAttacks = 1f;
    private bool alreadyAttacked;

    // FIXED: Removed unused variables (playerInSightRange, playerInAttackRange, sightRange)

    private void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        photonView = GetComponent<PhotonView>();

        // Subscribe to health death event
        Health enemyHealth = GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDied.AddListener(OnEnemyDied);
        }
    }

    private void Update()
    {
        // In multiplayer, only master client controls AI
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            return;

        // Find closest player (multiplayer support)
        Target = FindClosestPlayer();

        if (Target == null) return;

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

    private Transform FindClosestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        if (players.Length == 0) return null;
        if (players.Length == 1) return players[0].transform;

        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player.transform;
            }
        }

        return closest;
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

    private void OnEnemyDied()
    {
        // Destroy enemy when it dies
        if (PhotonNetwork.IsConnected)
        {
            if (photonView != null && PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}