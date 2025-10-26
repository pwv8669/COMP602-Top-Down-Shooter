using UnityEngine;
using Photon.Pun;

public abstract class Potion : MonoBehaviour
{
    [SerializeField] public int healAmount;
    [SerializeField] protected string potionName;
    public bool isLargePotion = false;

    private PhotonView photonView;

    protected virtual void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

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

        // Notify spawner before destroying
        PotionSpawner spawner = FindFirstObjectByType<PotionSpawner>();
        if (spawner != null)
        {
            spawner.OnPotionCollected(isLargePotion);
        }

        // Destroy properly based on multiplayer or singleplayer
        DestroyPotion();
    }

    private void DestroyPotion()
    {
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            // Multiplayer: Use PhotonNetwork.Destroy
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            // Singleplayer: Normal destroy
            Destroy(gameObject);
        }
    }
}