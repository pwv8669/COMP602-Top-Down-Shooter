using UnityEngine;

public class SpeedPotion : MonoBehaviour
{
    public float speedMultiplier = 2.0f;
    public float effectDuration = 10f;
    
    private SpeedPotionSpawner spawner;

    // This method will be called by the spawner when the potion is created
    public void SetSpawner(SpeedPotionSpawner spawnerReference)
    {
        spawner = spawnerReference;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Character playerController = other.GetComponent<Character>();

            if (playerController != null)
            {
                playerController.ActivateSpeedBoost(speedMultiplier, effectDuration);
                
                // Notify the spawner if we have a reference
                if (spawner != null)
                {
                    spawner.PotionCollected();
                }
                
                Destroy(gameObject);
            }
        }
    }
}