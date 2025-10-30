using UnityEngine;
using Photon.Pun;

public class HealthPotion : Potion
{
    private void Start()
    {
        potionName = "Health Potion";

        // Appearance (rotation, scale, sprite) is already set in the prefab
        // DO NOT modify transform at runtime - it interferes with PhotonTransformView synchronization
        // If you need to change appearance, modify the prefab directly in Unity Editor
    }

    /// <summary>
    /// Called automatically by Photon when this object is instantiated over the network
    /// This receives the InstantiationData sent by PhotonNetwork.Instantiate
    /// </summary>
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;

        if (data != null && data.Length >= 2)
        {
            healAmount = (int)data[0];
            isLargePotion = (bool)data[1];

            Debug.Log($"[HealthPotion] Received instantiation data: heal={healAmount}, isLarge={isLargePotion}, position={transform.position}");
        }
        else
        {
            Debug.LogWarning("[HealthPotion] No instantiation data received! Using default values.");
        }
    }

    public override void ApplyEffect(Health playerHealth)
    {
        Debug.Log($"Applying health potion effect: +{healAmount} health");
        Debug.Log($"Health before heal: {playerHealth.CurrentHealth}");
        playerHealth.Heal(healAmount);
        Debug.Log($"Health after heal: {playerHealth.CurrentHealth}");
    }

    // CRITICAL FIX: RPC must be on the component attached to PhotonView
    // PhotonView can only find RPCs on components it observes
    [PunRPC]
    private void RPC_CollectPotion(bool isLarge)
    {
        // Only host should execute this
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[HealthPotion] RPC_CollectPotion called on non-host client!");
            return;
        }

        Debug.Log($"[HealthPotion] Host received collection request - destroying potion");
        NotifySpawner();
        PhotonNetwork.Destroy(gameObject);
    }

    // Helper method to notify spawner
    private void NotifySpawner()
    {
        PotionSpawner spawner = FindFirstObjectByType<PotionSpawner>();
        if (spawner != null)
        {
            spawner.OnPotionCollected(isLargePotion);
        }
    }
}