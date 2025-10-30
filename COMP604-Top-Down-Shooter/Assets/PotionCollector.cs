using UnityEngine;
using Photon.Pun;

public class PotionCollector : MonoBehaviourPunCallbacks
{
    private Health playerHealth;

    private void Start()
    {
        playerHealth = GetComponent<Health>();
        if (playerHealth == null)
        {
            Debug.LogError("PotionCollector: No Health component found on player!");
        }
    }

    // 3D collision detection
    private void OnTriggerEnter(Collider other)
    {
        // MULTIPLAYER: Only local player can collect potions
        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && PhotonNetwork.IsConnected && !pv.IsMine)
        {
            return; // Not my player, ignore
        }

        // Check if the collided object is a potion
        Potion potion = other.GetComponent<Potion>();
        if (potion != null && playerHealth != null)
        {
            potion.Collect(playerHealth);
            Debug.Log($"Potion collected! Current health: {playerHealth.CurrentHealth}");
        }
    }
}