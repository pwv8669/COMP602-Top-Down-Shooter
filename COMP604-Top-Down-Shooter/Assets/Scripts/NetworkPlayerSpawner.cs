using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private string playerPrefabName = "NetworkPlayer";

    private bool hasSpawned = false;

    void Start()
    {
        // Don't spawn immediately, wait for room
        Debug.Log("[NetworkPlayerSpawner] Waiting for room connection...");
    }

    void Update() // Soyun add this function
    {
        // Wait until we're in a room and haven't spawned yet
        if (!hasSpawned && PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            hasSpawned = true;
            SpawnPlayer();
        }
    }

    void SpawnPlayer()
    {
        Vector3 spawnPosition = GetSpawnPosition();

        Debug.Log($"[NetworkPlayerSpawner] Spawning player at {spawnPosition}");

        GameObject player = PhotonNetwork.Instantiate(
            playerPrefabName,
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log($"[NetworkPlayerSpawner] Player spawned: {player.name}");
    }

    Vector3 GetSpawnPosition()
    {
        // If spawn points are defined, use them
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
            return spawnPoints[spawnIndex].position;
        }

        // Otherwise, random position
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-5f, 5f);
        return new Vector3(randomX, 1f, randomZ);
    }
}
