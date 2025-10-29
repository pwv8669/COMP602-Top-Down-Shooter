using UnityEngine;
using System.Collections;
using Photon.Pun;

/// <summary>
/// Spawns health potions around a random position
/// Host-only spawning with proper multiplayer synchronization
/// FIXED: Potions now spawn at correct Y height
/// </summary>
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
    [SerializeField] private Vector3 mapCenter = new Vector3(100f, 1.1f, 100f); // Default spawn center

    private int currentSmallPotions = 0;
    private int currentLargePotions = 0;

    // Prefab names for PhotonNetwork.Instantiate (must match Resources folder)
    private const string SMALL_POTION_NAME = "SmallHealthPotion";
    private const string LARGE_POTION_NAME = "LargeHealthPotion";

    private void Start()
    {
        // MULTIPLAYER: Only host spawns potions
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PotionSpawner] Client: Not spawning potions (host handles this)");
            return;
        }

        // Spawn some initial potions
        for (int i = 0; i < 3; i++)
        {
            SpawnPotion(SMALL_POTION_NAME, false);
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
                SpawnPotion(SMALL_POTION_NAME, false);
            }
            else if (randomValue < 0.9f && currentLargePotions < maxLargePotions)
            {
                // 20% chance for large potion
                SpawnPotion(LARGE_POTION_NAME, true);
            }
            // 10% chance to spawn nothing
        }
    }

    private void SpawnPotion(string potionPrefabName, bool isLargePotion)
    {
        // Validate prefab name
        if (string.IsNullOrEmpty(potionPrefabName))
        {
            Debug.LogError("[PotionSpawner] Potion prefab name is null or empty!");
            return;
        }

        // Get random position around map center
        Vector3 spawnPosition = GetRandomSpawnPosition();

        Debug.Log($"[PotionSpawner] Spawning {potionPrefabName} at position: {spawnPosition}");

        GameObject potion;

        if (PhotonNetwork.IsConnected)
        {
            // MULTIPLAYER: Use PhotonNetwork.Instantiate with EXACT prefab name from Resources
            potion = PhotonNetwork.Instantiate(potionPrefabName, spawnPosition, Quaternion.identity);
            Debug.Log($"[MULTIPLAYER] Spawned potion via PhotonNetwork: {potionPrefabName}");
        }
        else
        {
            // SINGLEPLAYER: Use normal Instantiate
            GameObject prefab = isLargePotion ? largeHealthPotionPrefab : smallHealthPotionPrefab;
            if (prefab == null)
            {
                Debug.LogError($"[PotionSpawner] Prefab is null for {potionPrefabName}!");
                return;
            }
            potion = Instantiate(prefab, spawnPosition, Quaternion.identity);
        }

        // Set up the potion values
        HealthPotion healthPotion = potion.GetComponent<HealthPotion>();
        if (healthPotion != null)
        {
            if (isLargePotion)
            {
                healthPotion.healAmount = 50;
                healthPotion.isLargePotion = true;
                currentLargePotions++;
            }
            else
            {
                healthPotion.healAmount = 10;
                healthPotion.isLargePotion = false;
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
        else
        {
            Debug.LogError($"[PotionSpawner] HealthPotion component not found on {potionPrefabName}!");
        }

        // Parent to this spawner for organization
        potion.transform.SetParent(transform, true);

        Debug.Log($"[PotionSpawner] Successfully spawned {(isLargePotion ? "Large" : "Small")} health potion at {spawnPosition}");
    }

    private Vector3 GetRandomSpawnPosition()
    {
        // Get random point around map center within spawn radius (X and Z only)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        // FIXED: Keep the Y position from mapCenter, don't override it with 0
        Vector3 spawnPosition = new Vector3(
            mapCenter.x + randomCircle.x,
            mapCenter.y,  // Use mapCenter's Y position
            mapCenter.z + randomCircle.y
        );

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

        Debug.Log($"[PotionSpawner] Potion collected. Small: {currentSmallPotions}/{maxSmallPotions}, Large: {currentLargePotions}/{maxLargePotions}");
    }
}