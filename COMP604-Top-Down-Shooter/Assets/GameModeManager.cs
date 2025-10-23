using UnityEngine;
using Photon.Pun;

/// <summary>
/// Manages player spawning for single/multiplayer modes
/// Place this in SampleScene
/// </summary>
public class GameModeManager : MonoBehaviour
{
    [Header("Scene Player Reference")]
    [SerializeField] private GameObject scenePlayer;
    [Tooltip("Drag a player object on the scene")]

    void Start()
    {
        HandlePlayerSpawning();
    }
    #region Player Spawning

    /// <summary>
    /// Spawn player character in the game scene
    /// Called automatically when joining room in game scene
    /// </summary>
    private void SpawnPlayer()
    {
        Debug.Log("[Multiplayer] Spawning player...");

        // Random spawn position
        Vector3 spawnPosition = new Vector3(
            100f,
            1.1f,
            100f
        );

        // Instantiate player prefab from Resources folder
        GameObject player = PhotonNetwork.Instantiate(
            "Player",  // Must be in Resources folder
            spawnPosition,
            Quaternion.identity
        );

        Debug.Log($"[Multiplayer] Player spawned at {spawnPosition}");
    }

    #endregion
    private void HandlePlayerSpawning()
    {
        // Check if we're in multiplayer mode
        bool isMultiplayer = PhotonNetwork.IsConnected && PhotonNetwork.InRoom;

        if (isMultiplayer)
        {
            // Multiplayer mode: Disable scene player
            Debug.Log("[GameMode] Multiplayer mode detected");

            if (scenePlayer != null)
            {
                scenePlayer.SetActive(false);
                Debug.Log("[GameMode] Scene player disabled - waiting for Photon spawn");
                // Spawn player only if in game scene
                Debug.Log($"[Multiplayer] Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
                {
                    SpawnPlayer();
                }
            }
            else
            {
                Debug.LogWarning("[GameMode] Scene player reference not set!");
            }
        }
        else
        {
            // Single player mode: Enable scene player
            Debug.Log("[GameMode] Single player mode detected");

            if (scenePlayer != null)
            {
                scenePlayer.SetActive(true);
                Debug.Log("[GameMode] Scene player enabled");
            }
            else
            {
                Debug.LogError("[GameMode] Scene player reference not set! Player won't spawn!");
            }
        }
    }
}