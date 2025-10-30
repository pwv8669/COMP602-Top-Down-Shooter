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
            return;
        }

        PhotonView pv = GetComponent<PhotonView>();

        if (PhotonNetwork.IsConnected && pv != null)
        {
            // MULTIPLAYER: Handle destruction based on who collected it
            if (PhotonNetwork.IsMasterClient)
            {
                // Host collected the potion - destroy directly
                Debug.Log("[Potion] Host collected potion - destroying directly");
                NotifySpawner();
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                // Client collected the potion - request host to destroy it
                Debug.Log("[Potion] Client collected potion - requesting host to destroy");
                pv.RPC("RPC_CollectPotion", RpcTarget.MasterClient, isLargePotion);
            }
        }
        else
        {
            // SINGLEPLAYER: Normal destroy
            NotifySpawner();
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// RPC called by client to tell host to destroy the potion
    /// Only host can destroy networked objects that host created
    /// </summary>
    [PunRPC]
    private void RPC_CollectPotion(bool isLarge)
    {
        // Only host should execute this
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[Potion] RPC_CollectPotion called on non-host client!");
            return;
        }

        Debug.Log($"[Potion] Host received collection request - destroying potion");
        NotifySpawner();
        PhotonNetwork.Destroy(gameObject);
    }

    private void NotifySpawner()
    {
        // Tell spawner to decrement the counter
        PotionSpawner spawner = FindFirstObjectByType<PotionSpawner>();
        if (spawner != null)
        {
            spawner.OnPotionCollected(isLargePotion);
        }
    }
}