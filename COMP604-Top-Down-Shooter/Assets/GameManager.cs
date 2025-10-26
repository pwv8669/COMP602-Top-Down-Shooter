using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Vector3 player1SpawnPos = new Vector3(100f, 1.1f, 100f);
    public Vector3 player2SpawnPos = new Vector3(103f, 1.1f, 100f);

    void Start()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("[GameManager] Photon Network is not ready!");
            return;
        }

        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        // Determine spawn position based on player number
        Vector3 spawnPos = PhotonNetwork.IsMasterClient ? player1SpawnPos : player2SpawnPos;

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

        Debug.Log("[GameManager] Local player setup complete");
    }

    void SetupRemotePlayer(GameObject player)
    {
        // Disable input components for remote players
        Character character = player.GetComponent<Character>();
        if (character != null) character.enabled = false;

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