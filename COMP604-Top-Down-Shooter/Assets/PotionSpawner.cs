using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PotionSpawner : MonoBehaviourPunCallbacks
{
    [Header("Potion Prefabs")]
    [SerializeField] private GameObject smallHealthPotionPrefab;
    [SerializeField] private GameObject largeHealthPotionPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int maxSmallPotions = 10;
    [SerializeField] private int maxLargePotions = 3;
    [SerializeField] private float spawnRadius = 15f;
    [SerializeField] private float spawnInterval = 5f;

    private int currentSmallPotions = 0;
    private int currentLargePotions = 0;
    private Transform playerTransform;

    private void Start()
    {
        // MULTIPLAYER: Only host spawns potions
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PotionSpawner] Client: Not spawning potions (host handles this)");
            return;
        }

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("PotionSpawner: No player found! Make sure your player has the 'Player' tag.");
        }

        // Spawn some initial potions
        for (int i = 0; i < 3; i++)
        {
            SpawnPotion(smallHealthPotionPrefab, false);
        }

        // Start continuous spawning
        StartCoroutine(SpawnPotionsRoutine());
    }

    private IEnumerator SpawnPotionsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // MULTIPLAYER: Only host spawns
            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
                continue;

            // Random chance to spawn different potions
            float randomValue = Random.value;

            if (randomValue < 0.7f && currentSmallPotions < maxSmallPotions)
            {
                // 70% chance for small potion
                SpawnPotion(smallHealthPotionPrefab, false);
            }
            else if (randomValue < 0.9f && currentLargePotions < maxLargePotions)
            {
                // 20% chance for large potion
                SpawnPotion(largeHealthPotionPrefab, true);
            }
            // 10% chance to spawn nothing
        }
    }

    private void SpawnPotion(GameObject potionPrefab, bool isLargePotion)
    {
        if (potionPrefab == null)
        {
            Debug.LogWarning("Potion prefab is null! Make sure to assign prefabs in the inspector.");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("PotionSpawner: No player transform found!");
            return;
        }

        // Get random position around player
        Vector3 spawnPosition = GetRandomSpawnPosition();

        Debug.Log($"Spawning potion at position: {spawnPosition}");

        GameObject potion;

        if (PhotonNetwork.IsConnected)
        {
            // MULTIPLAYER: Use PhotonNetwork.Instantiate
            potion = PhotonNetwork.Instantiate(potionPrefab.name, spawnPosition, Quaternion.identity);
            Debug.Log($"[MULTIPLAYER] Spawned potion via PhotonNetwork: {potionPrefab.name}");
        }
        else
        {
            // SINGLEPLAYER: Use normal Instantiate
            potion = Instantiate(potionPrefab, spawnPosition, Quaternion.identity);
        }

        // Set up the potion values
        HealthPotion healthPotion = potion.GetComponent<HealthPotion>();
        if (healthPotion != null)
        {
            if (isLargePotion)
            {
                healthPotion.healAmount = 50;
                healthPotion.isLargePotion = true; // Set the flag
                currentLargePotions++;
            }
            else
            {
                healthPotion.healAmount = 10;
                healthPotion.isLargePotion = false; // Set the flag
                currentSmallPotions++;
            }

            // MULTIPLAYER: Sync potion values to all clients
            if (PhotonNetwork.IsConnected)
            {
                PhotonView pv = potion.GetComponent<PhotonView>();
                if (pv != null)
                {
                    pv.RPC(nameof(HealthPotion.RPC_SetPotionValues), RpcTarget.AllBuffered,
                           healthPotion.healAmount, healthPotion.isLargePotion);
                }
            }
        }

        // Parent to this spawner for organization
        potion.transform.SetParent(transform);

        Debug.Log($"Successfully spawned {(isLargePotion ? "Large" : "Small")} health potion at {spawnPosition}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
            return Vector3.zero;
        }

        // Get random point around player within spawn radius (X and Z only)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // Use player's Y position so potions spawn at the same height as player
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        Debug.Log($"Player Y: {playerTransform.position.y}, Spawn Y: {spawnPosition.y}");

        return spawnPosition;
    }

    // Called when a potion is collected
    public void OnPotionCollected(bool isLargePotion)
    {
        // MULTIPLAYER: Only host manages potion counts
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
            return;

        if (isLargePotion)
        {
            currentLargePotions--;
        }
        else
        {
            currentSmallPotions--;
        }
    }
}