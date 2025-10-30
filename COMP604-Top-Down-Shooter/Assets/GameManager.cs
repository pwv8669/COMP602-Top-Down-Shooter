using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Vector3 player1SpawnPos = new Vector3(100f, 1.1f, 100f);
    public Vector3 player2SpawnPos = new Vector3(103f, 1.1f, 100f);

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("[GameManager] Photon Network is not connected!");
            return;
        }

        // Wait for Photon to be fully ready before spawning
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("[GameManager] Waiting for Photon to be ready...");
            StartCoroutine(WaitForPhotonReady());
            return;
        }

        SpawnPlayer();
    }

    /// <summary>
    /// Wait for Photon to be fully ready before spawning player
    /// This prevents timing issues when loading scenes
    /// </summary>
    IEnumerator WaitForPhotonReady()
    {
        float timeout = 10f;
        float elapsed = 0f;

        while (!PhotonNetwork.IsConnectedAndReady && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("[GameManager] Photon ready! Spawning player...");
            SpawnPlayer();
        }
        else
        {
            Debug.LogError("[GameManager] Timeout waiting for Photon to be ready!");
        }
    }

    void SpawnPlayer()
    {
        // Determine spawn position based on player number
        Vector3 spawnPos = PhotonNetwork.IsMasterClient ? player1SpawnPos : player2SpawnPos;

        Debug.Log($"[GameManager] Spawning player at {spawnPos} (IsMasterClient: {PhotonNetwork.IsMasterClient})");

        // Instantiate player
        GameObject player = PhotonNetwork.Instantiate("Character", spawnPos, Quaternion.identity);

        if (player == null)
        {
            Debug.LogError("[GameManager] Failed to instantiate player!");
            return;
        }

        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView == null)
        {
            Debug.LogError("[GameManager] PhotonView component is missing on Character prefab!");
            return;
        }

        if (photonView.IsMine)
        {
            player.name = "Player";
            player.tag = "Player";

            SetupLocalPlayer(player);
        }
        else
        {
            SetupRemotePlayer(player);
        }
    }

    void SetupLocalPlayer(GameObject player)
    {
        Debug.Log($"[GameManager] Setting up local player at {player.transform.position}");

        // Setup camera to follow this player
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Debug.Log($"[GameManager] Main camera found at {mainCam.transform.position}");

            CameraMovement camMove = mainCam.GetComponent<CameraMovement>();
            if (camMove != null)
            {
                camMove.playerTarget = player.transform;
                Debug.Log("[GameManager] Camera target set successfully!");
            }
            else
            {
                Debug.LogError("[GameManager] CameraMovement component NOT FOUND on Main Camera!");
            }
        }
        else
        {
            Debug.LogError("[GameManager] Main Camera NOT FOUND!");
        }

        // Setup player look at mouse
        PlayerLookAtMouse lookAtMouse = player.GetComponent<PlayerLookAtMouse>();
        if (lookAtMouse != null)
        {
            lookAtMouse.mainCamera = mainCam;
            Debug.Log("[GameManager] PlayerLookAtMouse configured");
        }

        // Setup gun system
        Transform gun = player.transform.Find("Gun");
        if (gun != null)
        {
            GunSystem gunSystem = gun.GetComponent<GunSystem>();
            if (gunSystem != null)
            {
                gunSystem.mainCamera = mainCam;
                Debug.Log("[GameManager] GunSystem configured");
            }
        }

        // Setup player health bar connection
        GameObject healthBarObj = GameObject.Find("PlayerHealthBar");
        if (healthBarObj != null)
        {
            Health playerHealth = player.GetComponent<Health>();
            HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();

            if (playerHealth != null && healthBar != null)
            {
                healthBar.SetHealth(playerHealth);
                Debug.Log("[GameManager] PlayerHealthBar connected successfully!");
            }
            else
            {
                Debug.LogError("[GameManager] Health or HealthBar component not found!");
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] PlayerHealthBar not found in scene.");
        }

        Debug.Log("[GameManager] Local player setup complete");
    }

    void SetupRemotePlayer(GameObject player)
    {
        PlayerLookAtMouse lookAtMouse = player.GetComponent<PlayerLookAtMouse>();
        if (lookAtMouse != null) lookAtMouse.enabled = false;

        // Disable gun for remote player
        Transform gun = player.transform.Find("Gun");
        if (gun != null)
        {
            GunSystem gunSystem = gun.GetComponent<GunSystem>();
            if (gunSystem != null) gunSystem.enabled = false;
        }

        Debug.Log("[GameManager] Remote player setup complete");
    }
}