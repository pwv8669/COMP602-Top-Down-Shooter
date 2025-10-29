using UnityEngine;
using System.Collections;

public class SpeedPotionSpawner : MonoBehaviour
{
    public GameObject speedPotionPrefab;
    public float spawnRadius = 5f;
    public int maxPotions = 3;
    public float spawnInterval = 10f;
    public Transform playerTransform; // Reference to player

    private int currentPotionCount = 0;

    void Start()
    {
        // If player transform not set, try to find it automatically
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        StartCoroutine(SpawnPotionRoutine());
    }

    IEnumerator SpawnPotionRoutine()
    {
        while (true)
        {
            if (currentPotionCount < maxPotions)
            {
                SpawnPotion();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnPotion()
    {
        if (playerTransform == null) return;

        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = playerTransform.position + new Vector3(randomPos.x, 0f, randomPos.y); // Y = 0f

        GameObject newPotion = Instantiate(speedPotionPrefab, spawnPosition, Quaternion.identity);
        
        // Adjust scale and rotation after spawning
        newPotion.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f); // Make smaller
        newPotion.transform.rotation = Quaternion.Euler(75f, 0f, 45f); // Lay on an angle
        
        Debug.Log("Spawning potion at position: " + spawnPosition);
        
        SpeedPotion potionScript = newPotion.GetComponent<SpeedPotion>();
        if (potionScript != null)
        {
            potionScript.SetSpawner(this);
        }
        
        currentPotionCount++;
    }

    public void PotionCollected()
    {
        currentPotionCount--;
        Debug.Log("Potion collected! Current potions: " + currentPotionCount);
    }
}