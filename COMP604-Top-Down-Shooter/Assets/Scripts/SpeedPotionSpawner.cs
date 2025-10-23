using UnityEngine;
using System.Collections;

public class SpeedPotionSpawner : MonoBehaviour
{
    public GameObject speedPotionPrefab;
    public float spawnRadius = 5f;
    public int maxPotions = 3;
    public float spawnInterval = 10f;

    private int currentPotionCount = 0;

    void Start()
    {
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
        Vector2 randomPos = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = new Vector3(randomPos.x, 0, randomPos.y); // Changed for 3D

        GameObject newPotion = Instantiate(speedPotionPrefab, spawnPosition, Quaternion.identity);
        
        // Add the speed potion component and set up the reference
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
    }
}