using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int maxEnemies = 5;
    public float respawnDelay = 3f;

    private List<GameObject> aliveEnemies = new List<GameObject>();
    private bool isRespawning = false;
    private bool isMultiplayer = false;

    void Start()
    {
        isMultiplayer = PhotonNetwork.IsConnected;

        // Only host spawns enemies in multiplayer
        if (!isMultiplayer || PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < maxEnemies; i++)
            {
                SpawnEnemy();
            }
        }
    }

    private void Update()
    {
        // Only host manages enemy spawning in multiplayer
        if (isMultiplayer && !PhotonNetwork.IsMasterClient)
            return;

        aliveEnemies.RemoveAll(e => e == null);

        if (aliveEnemies.Count < maxEnemies && !isRespawning)
        {
            StartCoroutine(RespawnWithDelay());
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy;

        if (isMultiplayer)
        {
            // Multiplayer: Use PhotonNetwork.Instantiate
            enemy = PhotonNetwork.Instantiate(enemyPrefab.name, transform.position, Quaternion.identity);
        }
        else
        {
            // Singleplayer: Use normal Instantiate
            enemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
        }

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