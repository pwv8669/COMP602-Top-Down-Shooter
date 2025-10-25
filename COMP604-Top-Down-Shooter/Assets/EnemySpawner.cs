using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemiesPerPlayer = 5;
    public float respawnDelay = 3f;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isRespawning = false;

    void Start()
    {
        for (int i = 0; i < maxEnemiesPerPlayer; i++)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        // Remove destroyed references
        aliveEnemies.RemoveAll(e => e == null);

        // Top up enemies if under the limit
        if (aliveEnemies.Count < maxEnemiesPerPlayer && !isRespawning)
        {
            StartCoroutine(RespawnWithDelay());
        }
    }
    
    void SpawnEnemy()
    {
        GameObject enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        if(ai != null)
        {
            ai.Target = GameObject.FindWithTag("Player").transform;
        }
        aliveEnemies.Add(enemy);
    }

    private System.Collections.IEnumerator RespawnWithDelay()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        if (aliveEnemies.Count < maxEnemiesPerPlayer)
        {
            SpawnEnemy();
        }
        isRespawning = false;
    }
}