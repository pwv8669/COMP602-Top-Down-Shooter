using UnityEngine;
using Photon.Pun;

public class PotionCollector : MonoBehaviour
{
    private Health playerHealth;
    private PhotonView photonView;

    private void Start()
    {
        playerHealth = GetComponent<Health>();
        photonView = GetComponent<PhotonView>();

        if (playerHealth == null)
        {
            Debug.LogError("PotionCollector: No Health component found on player!");
        }
    }

    // 3D collision detection
    private void OnTriggerEnter(Collider other)
    {
        // In multiplayer, only local player can collect potions
        if (photonView != null && PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            return;
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