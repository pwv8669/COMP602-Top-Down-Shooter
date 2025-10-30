using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Character : MonoBehaviourPunCallbacks
{
    private CharacterController characterController;
    private PhotonView photonView;
    public float Speed = 5f;
    private Vector2 moveInput;
    
    // Speed boost variables
    private float originalSpeed;
    private bool isSpeedBoostActive = false;
    private float speedBoostTimeRemaining = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        photonView = GetComponent<PhotonView>();
        originalSpeed = Speed;
    }

    // Update is called once per frame
    void Update()
    {
        // Only allow input for local player
        if (photonView != null && !photonView.IsMine)
            return;

        Vector3 move = new Vector3 (moveInput.x, 0, moveInput.y);

        if(move.magnitude > 1f) move.Normalize();

        characterController.Move(move * Time.deltaTime * Speed);

        // Handle the countdown for the speed boost
        if (isSpeedBoostActive)
        {
            speedBoostTimeRemaining -= Time.deltaTime;

            if (speedBoostTimeRemaining <= 0)
            {
                // When time runs out, revert the speed
                DeactivateSpeedBoost();
            }
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Only allow input for local player
        if (photonView != null && !photonView.IsMine)
            return;

        moveInput = context.ReadValue<Vector2>();
    }

    // This public method is called by the SpeedPotion script when collected
    /// <summary>
    /// Called by SpeedPotion when collected
    /// This method now triggers an RPC to synchronize speed boost across all clients
    /// </summary>
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        // If in multiplayer, call RPC to sync with all clients
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            Debug.Log($"[Character] Calling RPC_ActivateSpeedBoost for all clients");
            photonView.RPC("RPC_ActivateSpeedBoost", RpcTarget.All, multiplier, duration);
        }
        else
        {
            // Singleplayer: just activate directly
            ApplySpeedBoost(multiplier, duration);
        }
    }

    /// <summary>
    /// RPC method that runs on all clients to synchronize speed boost
    /// This ensures everyone sees the player moving faster
    /// </summary>
    [PunRPC]
    private void RPC_ActivateSpeedBoost(float multiplier, float duration)
    {
        ApplySpeedBoost(multiplier, duration);
        Debug.Log($"[Character] RPC_ActivateSpeedBoost received on {(photonView.IsMine ? "LOCAL" : "REMOTE")} player. Speed: {Speed}");
    }

    /// <summary>
    /// Actually applies the speed boost
    /// </summary>
    private void ApplySpeedBoost(float multiplier, float duration)
    {
        Speed = originalSpeed * multiplier;
        speedBoostTimeRemaining = duration;
        isSpeedBoostActive = true;

        Debug.Log($"Speed Boost Activated! Current speed: {Speed}");
    }

    /// <summary>
    /// Deactivates speed boost and syncs across network
    /// </summary>
    private void DeactivateSpeedBoost()
    {
        // If in multiplayer, call RPC to sync with all clients
        if (PhotonNetwork.IsConnected && photonView != null)
        {
            Debug.Log($"[Character] Calling RPC_DeactivateSpeedBoost for all clients");
            photonView.RPC("RPC_DeactivateSpeedBoost", RpcTarget.All);
        }
        else
        {
            // Singleplayer: just deactivate directly
            ApplySpeedDeactivation();
        }
    }

    /// <summary>
    /// RPC method that runs on all clients to synchronize speed boost deactivation
    /// </summary>
    [PunRPC]
    private void RPC_DeactivateSpeedBoost()
    {
        ApplySpeedDeactivation();
        Debug.Log($"[Character] RPC_DeactivateSpeedBoost received on {(photonView.IsMine ? "LOCAL" : "REMOTE")} player. Speed: {Speed}");
    }

    /// <summary>
    /// Actually deactivates the speed boost
    /// </summary>
    private void ApplySpeedDeactivation()
    {
        Speed = originalSpeed;
        speedBoostTimeRemaining = 0f;
        isSpeedBoostActive = false;

        Debug.Log($"Speed Boost Deactivated. Speed back to: {Speed}");
    }
}