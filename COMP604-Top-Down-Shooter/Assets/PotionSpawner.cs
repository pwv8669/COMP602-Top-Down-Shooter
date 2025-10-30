using UnityEngine;
using System.Collections;
using Photon.Pun;

/// <summary>
/// Spawns health potions around a random position
/// Host-only spawning with proper multiplayer synchronization
/// FIXED: Removed SetParent to prevent position desync
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
    [SerializeField] private Vector3 mapCenter = new Vector3(100f, 1.1f, 100f);

    private int currentSmallPotions = 0;
    private int currentLargePotions = 0;

    private const string SMALL_POTION_NAME = "SmallHealthPotion";
    private const string LARGE_POTION_NAME = "LargeHealthPotion";

    private void Start()
    {
        Debug.Log($"[PotionSpawner] === INSPECTOR SETTINGS ===");
        Debug.Log($"[PotionSpawner] mapCenter = {mapCenter}");
        Debug.Log($"[PotionSpawner] spawnRadius = {spawnRadius}");
        Debug.Log($"[PotionSpawner] IsMasterClient = {PhotonNetwork.IsMasterClient}");
        Debug.Log($"[PotionSpawner] IsConnected = {PhotonNetwork.IsConnected}");

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PotionSpawner] Client: Not spawning potions (host handles this)");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            SpawnPotion(SMALL_POTION_NAME, false);
        }

        StartCoroutine(SpawnPotionsRoutine());
    }

    private IEnumerator SpawnPotionsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
                continue;

            float randomValue = Random.value;

            if (randomValue < 0.7f && currentSmallPotions < maxSmallPotions)
            {
                SpawnPotion(SMALL_POTION_NAME, false);
            }
            else if (randomValue < 0.9f && currentLargePotions < maxLargePotions)
            {
                SpawnPotion(LARGE_POTION_NAME, true);
            }
        }
    }

    private void SpawnPotion(string potionPrefabName, bool isLargePotion)
    {
        if (string.IsNullOrEmpty(potionPrefabName))
        {
            Debug.LogError("[PotionSpawner] Potion prefab name is null or empty!");
            return;
        }

        Vector3 spawnPosition = GetRandomSpawnPosition();

        Debug.Log($"[PotionSpawner] Spawning {potionPrefabName} at position: {spawnPosition}");

        GameObject potion;
        int healAmount = isLargePotion ? 50 : 10;

        if (PhotonNetwork.IsConnected)
        {
            object[] instantiationData = new object[] { healAmount, isLargePotion };

            potion = PhotonNetwork.Instantiate(
                potionPrefabName,
                spawnPosition,
                Quaternion.identity,
                0,
                instantiationData
            );

            potion.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            Debug.Log($"[MULTIPLAYER] Spawned potion via PhotonNetwork: {potionPrefabName} at {spawnPosition}");
        }
        else
        {
            GameObject prefab = isLargePotion ? largeHealthPotionPrefab : smallHealthPotionPrefab;
            if (prefab == null)
            {
                Debug.LogError($"[PotionSpawner] Prefab is null for {potionPrefabName}!");
                return;
            }
            potion = Instantiate(prefab, spawnPosition, Quaternion.identity);

            potion.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            HealthPotion healthPotion = potion.GetComponent<HealthPotion>();
            if (healthPotion != null)
            {
                healthPotion.healAmount = healAmount;
                healthPotion.isLargePotion = isLargePotion;
            }
        }

        if (isLargePotion)
            currentLargePotions++;
        else
            currentSmallPotions++;

        // DO NOT parent networked objects!
        // This causes position desync because PhotonTransformView syncs local position
        // potion.transform.SetParent(transform);  // REMOVED

        Debug.Log($"[PotionSpawner] Successfully spawned {(isLargePotion ? "Large" : "Small")} health potion at {spawnPosition}");
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

    public void OnPotionCollected(bool isLargePotion)
    {
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