using UnityEngine;
using UnityEngine.InputSystem;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    
    void Start()
    {
        Debug.Log("EnemySpawner started");
        SpawnEnemyAtPlayerPosition();
    }
    
    void SpawnEnemyAtPlayerPosition()
    {
        if (enemyPrefab != null)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, transform.position + Vector3.right * 3f, Quaternion.identity);
            Debug.Log("Enemy spawned at position: " + newEnemy.transform.position);
        }
        else
        {
            Debug.LogError("Enemy prefab is not assigned!");
        }
    }
    
    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("T key pressed - spawning enemy");
            SpawnEnemyAtPlayerPosition();
        }
    }
}