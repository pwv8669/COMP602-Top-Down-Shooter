using UnityEngine;
using Photon.Pun;

public abstract class Potion : MonoBehaviourPunCallbacks
{
    [SerializeField] public int healAmount;
    [SerializeField] protected string potionName;
    public bool isLargePotion = false;

    public abstract void ApplyEffect(Health playerHealth);

    public void Collect(Health playerHealth)
    {
        if (playerHealth != null)
        {
            ApplyEffect(playerHealth);
            Debug.Log($"{potionName} collected! Healing for {healAmount} health. New health: {playerHealth.CurrentHealth}");
        }
        else
        {
            Debug.LogError("Potion.Collect: playerHealth is null!");
        }

        // MULTIPLAYER: Notify spawner before destroying (only if we're the collector)
        PhotonView pv = GetComponent<PhotonView>();
        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            // Singleplayer OR we are the host in multiplayer
            PotionSpawner spawner = FindFirstObjectByType<PotionSpawner>();
            if (spawner != null)
            {
                spawner.OnPotionCollected(isLargePotion);
            }
        }

        // MULTIPLAYER: Destroy on network
        if (PhotonNetwork.IsConnected && pv != null)
        {
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            // SINGLEPLAYER: Normal destroy
            Destroy(gameObject);
        }
    }
}