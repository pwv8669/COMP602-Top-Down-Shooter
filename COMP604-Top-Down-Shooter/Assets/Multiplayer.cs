using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Multiplayer : MonoBehaviourPunPV, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Room Settings")]
    public int maxPlayersPerRoom = 4;
    public string gameVersion = "1.0";

    [Header("Debug Info")]
    public bool showDebugLogs = true;

    // Current room status
    private bool isConnectedToPhoton = false;
    private bool isInLobby = false;

    void Start()
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Starting connection to Photon...");

        // Set game version for matchmaking
        PhotonNetwork.GameVersion = gameVersion;

        // Set player nickname
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        // Connect to Photon servers
        PhotonNetwork.ConnectUsingSettings();
    }

    #region IConnectionCallbacks Implementation

    public void OnConnected()
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Connected to internet!");
    }

    public void OnConnectedToMaster()
    {
        isConnectedToPhoton = true;

        if (showDebugLogs)
        {
            Debug.Log("[Multiplayer] Connected to Photon Master Server!");
            Debug.Log("[Multiplayer] Player nickname: " + PhotonNetwork.NickName);
            Debug.Log("[Multiplayer] Ready to create or join rooms with codes!");
        }

        // Join lobby to receive room lists
        PhotonNetwork.JoinLobby();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        isConnectedToPhoton = false;
        isInLobby = false;

        if (showDebugLogs)
            Debug.LogError($"[Multiplayer] Disconnected from Photon! Cause: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Region list received");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Custom authentication response");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        if (showDebugLogs)
            Debug.LogError($"[Multiplayer] Custom authentication failed: {debugMessage}");
    }

    #endregion

    #region IMatchmakingCallbacks Implementation

    public void OnJoinedLobby()
    {
        isInLobby = true;

        if (showDebugLogs)
        {
            Debug.Log("[Multiplayer] Joined lobby successfully!");
            Debug.Log("[Multiplayer] Ready to create or join rooms!");
        }
    }

    public void OnLeftLobby()
    {
        isInLobby = false;
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Left lobby");
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Room list update (can be used by UI if needed)
        if (showDebugLogs)
        {
            Debug.Log($"[Multiplayer] Room list updated! Found {roomList.Count} rooms");
        }
    }

    public void OnJoinedRoom()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[Multiplayer] Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
            Debug.Log($"Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
            Debug.Log($"I am Master Client: {PhotonNetwork.IsMasterClient}");

            // Print all players in the room
            foreach (var player in PhotonNetwork.PlayerList)
            {
                Debug.Log($"Player in room: {player.NickName} (ID: {player.ActorNumber})");
            }
        }
    }

    public void OnLeftRoom()
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Left room");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        if (showDebugLogs)
            Debug.LogError($"[Multiplayer] Failed to create room! Code: {returnCode}, Message: {message}");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        if (showDebugLogs)
            Debug.LogError($"[Multiplayer] Failed to join room! Code: {returnCode}, Message: {message}");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        // Random room joining is not used in this system
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Random room joining not used in this system");
    }

    public void OnCreatedRoom()
    {
        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Successfully created room with code: {PhotonNetwork.CurrentRoom.Name}");
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Player {newPlayer.NickName} joined the room! Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Player {otherPlayer.NickName} left the room! Remaining players: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Master client switched to {newMasterClient.NickName}");
    }

    #endregion

    #region Room Code System

    /// <summary>
    /// Creates a new room and returns a 6-digit code
    /// </summary>
    public string CreateRoomWithCode()
    {
        if (!IsReadyToCreateRoom())
        {
            if (showDebugLogs)
                Debug.LogError("[Multiplayer] Not ready to create room! Make sure you're connected to Photon.");
            return null;
        }

        // Generate 6-digit room code
        string roomCode = GenerateRoomCode();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)maxPlayersPerRoom;
        roomOptions.IsVisible = false; // Hidden from public list, only accessible via code
        roomOptions.IsOpen = true;

        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Creating room with code: {roomCode}");

        PhotonNetwork.CreateRoom(roomCode, roomOptions);

        return roomCode;
    }

    /// <summary>
    /// Join a room using the provided room code
    /// </summary>
    public bool JoinRoomWithCode(string roomCode)
    {
        if (!IsReadyToJoinRoom())
        {
            if (showDebugLogs)
                Debug.LogError("[Multiplayer] Not ready to join room! Make sure you're connected to Photon.");
            return false;
        }

        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            if (showDebugLogs)
                Debug.LogError("[Multiplayer] Invalid room code! Code must be 6 characters.");
            return false;
        }

        roomCode = roomCode.ToUpper(); // Convert to uppercase

        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Trying to join room with code: {roomCode}");

        PhotonNetwork.JoinRoom(roomCode);
        return true;
    }

    /// <summary>
    /// Generates a random 6-digit room code (numbers + uppercase letters)
    /// </summary>
    private string GenerateRoomCode()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string result = "";

        for (int i = 0; i < 6; i++)
        {
            result += chars[Random.Range(0, chars.Length)];
        }

        return result;
    }

    #endregion

    #region Game Start (Host Only)

    /// <summary>
    /// Start the game (only the host can call this, signals all players)
    /// </summary>
    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (showDebugLogs)
                Debug.LogError("[Multiplayer] Only the host can start the game!");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < 1)
        {
            if (showDebugLogs)
                Debug.LogError("[Multiplayer] Need at least 1 player to start the game!");
            return;
        }

        if (showDebugLogs)
            Debug.Log($"[Multiplayer] Host starting game with {PhotonNetwork.CurrentRoom.PlayerCount} players!");

        // Send game start signal to all players
        photonView.RPC("OnGameStarted", RpcTarget.All);
    }

    [PunRPC]
    void OnGameStarted()
    {
        if (showDebugLogs)
            Debug.Log("[Multiplayer] Game started! Loading actual game scene...");

        // Here you can load the actual game scene or
        // activate teammates' Character, MapGenerator, etc.
        // Example: SceneManager.LoadScene("GameScene");
    }

    #endregion

    #region Leave Room

    /// <summary>
    /// Leave the current room
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            if (showDebugLogs)
                Debug.Log("[Multiplayer] Leaving current room...");
            PhotonNetwork.LeaveRoom();
        }
    }

    #endregion

    #region Status Check Methods

    public bool IsReadyToCreateRoom()
    {
        return isConnectedToPhoton && isInLobby && !PhotonNetwork.InRoom;
    }

    public bool IsReadyToJoinRoom()
    {
        return isConnectedToPhoton && isInLobby && !PhotonNetwork.InRoom;
    }

    public bool IsConnectedToPhoton()
    {
        return isConnectedToPhoton;
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public int GetPlayerCount()
    {
        return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    }

    public int GetMaxPlayers()
    {
        return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.MaxPlayers : maxPlayersPerRoom;
    }

    public string GetRoomCode()
    {
        return PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "";
    }

    public string GetPlayerNickname()
    {
        return PhotonNetwork.NickName;
    }

    public Player[] GetAllPlayers()
    {
        return PhotonNetwork.PlayerList;
    }

    #endregion
}
