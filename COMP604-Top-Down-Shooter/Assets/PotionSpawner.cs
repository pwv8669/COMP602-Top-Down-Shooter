using UnityEngine;
using System.Collections;

public class PotionSpawner : MonoBehaviour
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
    
    private void Start()
    {
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
        
        // SPAWN AT CAMERA POSITION instead of player position
        Camera mainCamera = Camera.main;
        Vector3 spawnPosition = mainCamera != null ? mainCamera.transform.position : Vector3.zero;
        
        // CRITICAL FIX: Match the Z-position of your game world (player is at Z=100)
        spawnPosition.z = 100f; // Force Z to match your game world
        
        Debug.Log($"Spawning potion at CAMERA position: {spawnPosition}");
        
        GameObject potion = Instantiate(potionPrefab, spawnPosition, Quaternion.identity);
        
        // Set up the potion values
        HealthPotion healthPotion = potion.GetComponent<HealthPotion>();
        if (healthPotion != null)
        {
            if (isLargePotion)
            {
                healthPotion.healAmount = 50;
                currentLargePotions++;
            }
            else
            {
                healthPotion.healAmount = 10;
                currentSmallPotions++;
            }
        }
        
        // Parent to this spawner for organization
        potion.transform.SetParent(transform);
        
        Debug.Log($"Successfully spawned {(isLargePotion ? "Large" : "Small")} health potion at {spawnPosition}");
    }
    
    // REMOVED the duplicate GetRandomSpawnPosition method since we're not using it right now
    
    // Called when a potion is collected (we'll hook this up later)
    public void OnPotionCollected(bool isLargePotion)
    {
        if (isLargePotion)
        {
            currentLargePotions--;
        }
        else
        {
            currentSmallPotions--;
        }
    }

    private void Update()
    {
        // Press Space to manually spawn a potion for testing
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnPotion(smallHealthPotionPrefab, false);
            Debug.Log("Manually spawned potion with Space key");
            
            // TEMPORARY: Count how many potions exist in scene
            int potionCount = GameObject.FindObjectsOfType<HealthPotion>().Length;
            Debug.Log($"Total potions in scene: {potionCount}");
        }
    }
}