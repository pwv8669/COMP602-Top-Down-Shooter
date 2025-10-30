using UnityEngine;
using Photon.Pun;

public class SpeedPotion : MonoBehaviourPunCallbacks
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
        PhotonView playerPV = other.GetComponent<PhotonView>();

        if (playerPV != null && playerPV.IsMine)
        {
            Character playerController = other.GetComponent<Character>();

            if (playerController != null)
            {
                playerController.ActivateSpeedBoost(speedMultiplier, effectDuration);

                if (spawner != null)
                {
                    spawner.PotionCollected();
                }

                DestroyPotion();
            }
        }
    }

    private void DestroyPotion()
    {
        PhotonView pv = GetComponent<PhotonView>();

        if (PhotonNetwork.IsConnected && pv != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("[SpeedPotion] Host destroying potion");
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.Log("[SpeedPotion] Client requesting host to destroy");
                pv.RPC("RPC_CollectPotion", RpcTarget.MasterClient);
            }
        }
        else
        {
            Debug.Log("[SpeedPotion] Singleplayer - normal destroy");
            Destroy(gameObject);
        }
    }

    [PunRPC]
    private void RPC_CollectPotion()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[SpeedPotion] RPC called on non-host!");
            return;
        }

        Debug.Log("[SpeedPotion] Host received collection request - destroying");
        PhotonNetwork.Destroy(gameObject);
    }
}