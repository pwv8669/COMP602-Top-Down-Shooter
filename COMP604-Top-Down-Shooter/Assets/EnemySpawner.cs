using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class EnemySpawner : MonoBehaviourPunCallbacks
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float respawnDelay = 3f;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isRespawning = false;

    void Start()
    {
        // MULTIPLAYER: Only host spawns enemies
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[EnemySpawner] Client: Not spawning enemies (host handles this)");
            return;
        }

        // Spawn initial enemies
        for (int i = 0; i < maxEnemies; i++)
        {
            SpawnEnemy();
        }
    }

    private void Update()
    {
        // MULTIPLAYER: Only host manages enemy spawning
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            return;

        // Clean up null references (destroyed enemies)
        aliveEnemies.RemoveAll(e => e == null);

        // Respawn if needed
        if (aliveEnemies.Count < maxEnemies && !isRespawning)
        {
            StartCoroutine(RespawnWithDelay());
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy;

        if (PhotonNetwork.IsConnected)
        {
            // MULTIPLAYER: Use PhotonNetwork.Instantiate
            enemy = PhotonNetwork.Instantiate(enemyPrefab.name, transform.position, Quaternion.identity);
            Debug.Log($"[MULTIPLAYER] Spawned enemy via PhotonNetwork at {transform.position}");
        }
        else
        {
            // SINGLEPLAYER: Use normal Instantiate
            enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }

        enemy.layer = LayerMask.NameToLayer("Enemy");

        Debug.Log($"[EnemySpawner] Enemy spawned - Layer: {enemy.layer} ({LayerMask.LayerToName(enemy.layer)})");

        // Enemy AI will find closest player automatically
        aliveEnemies.Add(enemy);
    }

    private System.Collections.IEnumerator RespawnWithDelay()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        if (aliveEnemies.Count < maxEnemies)
        {
            SpawnEnemy();
        }
        isRespawning = false;
    }
}