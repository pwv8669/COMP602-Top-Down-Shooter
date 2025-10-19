using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun; // Soyun  edit this part

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    
    void Start()
    {
        Debug.Log("EnemySpawner started");

        //Soyun edit this part
        if (!PhotonNetwork.IsMasterClient)
            return;

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
        // Soyun edit this part
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("T key pressed - spawning enemy");
            SpawnEnemyAtPlayerPosition();
        }
    }
}