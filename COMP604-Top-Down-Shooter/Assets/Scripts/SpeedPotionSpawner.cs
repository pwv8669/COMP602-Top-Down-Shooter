using UnityEngine;
using System.Collections;
using Photon.Pun;

public class SpeedPotionSpawner : MonoBehaviourPunCallbacks
{
    public GameObject speedPotionPrefab;
    public float spawnRadius = 5f;
    public int maxPotions = 3;
    public float spawnInterval = 10f;
    public Transform playerTransform; // Reference to player
    public Vector3 mapCenter = new Vector3(100f, 1.1f, 100f);

    private int currentPotionCount = 0;
    private const string SPEED_POTION_NAME = "SpeedPotion";

    void Start()
    {
        Debug.Log($"[SpeedPotionSpawner] IsMasterClient: {PhotonNetwork.IsMasterClient}");
        Debug.Log($"[SpeedPotionSpawner] IsConnected: {PhotonNetwork.IsConnected}");

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[SpeedPotionSpawner] Client: Not spawning potions (host handles this)");
            return;
        }

        /*
        // If player transform not set, try to find it automatically
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }*/

        StartCoroutine(SpawnPotionRoutine());
    }

    IEnumerator SpawnPotionRoutine()
    {
        while (true)
        {
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            if (currentPotionCount < maxPotions)
            {
                SpawnPotion();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPotion()
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        Debug.Log($"[SpeedPotionSpawner] Spawning potion at: {spawnPosition}");

        GameObject newPotion;

        if (PhotonNetwork.IsConnected)
        {
            newPotion = PhotonNetwork.Instantiate(SPEED_POTION_NAME, spawnPosition, Quaternion.identity);
            Debug.Log($"[MULTIPLAYER] Spawned speed potion via PhotonNetwork at {spawnPosition}");
        }
        else
        {
            if (speedPotionPrefab == null)
            {
                Debug.LogError("[SpeedPotionSpawner] speedPotionPrefab is null!");
                return;
            }

            newPotion = Instantiate(speedPotionPrefab, spawnPosition, Quaternion.identity);
        }

        SpeedPotion potionScript = newPotion.GetComponent<SpeedPotion>();
        if (potionScript != null)
        {
            potionScript.SetSpawner(this);
        }

        currentPotionCount++;
        Debug.Log($"[SpeedPotionSpawner] Potion spawned. Count: {currentPotionCount}/{maxPotions}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPosition = new Vector3(
            mapCenter.x + randomCircle.x,
            mapCenter.y,
            mapCenter.z + randomCircle.y
        );

        return spawnPosition;
    }

    public void PotionCollected()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            return;

        currentPotionCount--;
        Debug.Log($"[SpeedPotionSpawner] Potion collected. Count: {currentPotionCount}/{maxPotions}");
    }
}