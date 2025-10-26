using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public Vector3 player1SpawnPos = new Vector3(100f, 1.1f, 100f);
    public Vector3 player2SpawnPos = new Vector3(103f, 1.1f, 100f);

    void Start()
    {
        // Check if in multiplayer mode
        if (PhotonNetwork.IsConnected)
        {
            if (!PhotonNetwork.IsConnectedAndReady)
            {
                Debug.LogError("[GameManager] Photon Network is not ready!");
                return;
            }

            // Multiplayer: Spawn new players
            SpawnPlayer();
        }
        else
        {
            // Singleplayer: Find existing player in scene
            SetupExistingPlayer();
        }
    }

    void SpawnPlayer()
    {
        // Only for multiplayer - spawn networked players
        Vector3 spawnPos = PhotonNetwork.IsMasterClient ? player1SpawnPos : player2SpawnPos;
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

    void SetupExistingPlayer()
    {
        // Singleplayer: Find player already in the scene
        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogError("[GameManager] No player found in scene! Make sure player has 'Player' tag.");
            return;
        }

        Debug.Log("[GameManager] Found existing player in scene");
        SetupLocalPlayer(player);
    }

    void SetupLocalPlayer(GameObject player)
    {
        Debug.Log($"[GameManager] Setting up local player at {player.transform.position}");

        // Setup camera to follow this player
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            CameraMovement camMove = mainCam.GetComponent<CameraMovement>();
            if (camMove != null)
            {
                camMove.playerTarget = player.transform;
                Debug.Log("[GameManager] Camera target set successfully!");
            }
        }

        // Setup player look at mouse
        PlayerLookAtMouse lookAtMouse = player.GetComponent<PlayerLookAtMouse>();
        if (lookAtMouse != null)
        {
            lookAtMouse.mainCamera = mainCam;
        }

        // Setup gun system
        Transform gun = player.transform.Find("Gun");
        if (gun != null)
        {
            GunSystem gunSystem = gun.GetComponent<GunSystem>();
            if (gunSystem != null)
            {
                gunSystem.mainCamera = mainCam;
            }
        }

        // Connect HealthBar to this player
        ConnectHealthBar(player);

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

    void ConnectHealthBar(GameObject player)
    {
        // PlayerHealthBar is a child Canvas of the player prefab
        GameObject healthBarObj = GameObject.Find("PlayerHealthBar");
        if (healthBarObj == null)
        {
            Debug.LogWarning("[GameManager] PlayerHealthBar not found in scene");
            return;
        }

        HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
        Health playerHealth = player.GetComponent<Health>();

        if (healthBar == null || playerHealth == null)
        {
            Debug.LogWarning("[GameManager] HealthBar or Health component missing");
            return;
        }

        // Use reflection to set the private health field
        System.Reflection.FieldInfo healthField = typeof(HealthBar).GetField("health",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (healthField != null)
        {
            healthField.SetValue(healthBar, playerHealth);
            Debug.Log("[GameManager] HealthBar connected to player!");
        }
        else
        {
            Debug.LogError("[GameManager] Could not find 'health' field in HealthBar.");
        }
    }
}