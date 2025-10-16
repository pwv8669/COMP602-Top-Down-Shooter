using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Multiplayer : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
{
    [Header("Room Settings")]
    public int maxPlayersPerRoom = 4;
    public string gameVersion = "1.0";

    [Header("Debug Settings")]
    public bool showDebugLogs = false;

    // Events for UI to subscribe to
    public System.Action OnReadyForRoomOperations;
    public System.Action<string> OnRoomCreated;
    public System.Action OnRoomJoinedSuccess;
    public System.Action<string> OnRoomJoinedFailed;
    public System.Action OnRoomLeft;
    public System.Action<Player> OnPlayerJoined;
    public System.Action<Player> OnPlayerLeft;

    // Current room status
    private bool isConnectedToPhoton = false;
    private bool isInLobby = false;

    // Properties
    public bool IsConnectedToPhoton => isConnectedToPhoton;
    public bool IsInLobby => isInLobby;
    public bool IsInRoom => PhotonNetwork.InRoom;
    public bool IsMasterClient => PhotonNetwork.IsMasterClient;
    public string CurrentRoomCode => PhotonNetwork.CurrentRoom?.Name ?? "";
    public int CurrentPlayerCount => PhotonNetwork.CurrentRoom?.PlayerCount ?? 0;
    public int MaxPlayers => PhotonNetwork.CurrentRoom?.MaxPlayers ?? 0;

    void Start()
    {
        LogDebug("[Multiplayer] Starting Photon connection...");
        PhotonNetwork.AddCallbackTarget(this);

#if UNITY_EDITOR
        // Clean up any previous connection in Editor
        if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            LogDebug("[Multiplayer] Editor - Disconnecting previous connection...");
            PhotonNetwork.Disconnect();
            Invoke(nameof(RetryConnection), 2f);
            return;
        }
#endif

        StartConnection();
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void RetryConnection()
    {
        LogDebug("[Multiplayer] Retrying connection...");
        StartConnection();
    }

    private void StartConnection()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        LogDebug($"[Multiplayer] Connecting as: {PhotonNetwork.NickName}");
        PhotonNetwork.ConnectUsingSettings();
    }

    #region IConnectionCallbacks

    public void OnConnected()
    {
        LogDebug("[Multiplayer] OnConnected");
    }

    public void OnConnectedToMaster()
    {
        isConnectedToPhoton = true;
        LogDebug("[Multiplayer] Connected to Master Server - Joining Lobby...");
        PhotonNetwork.JoinLobby();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        isConnectedToPhoton = false;
        isInLobby = false;
        Debug.LogError($"[Multiplayer] Disconnected: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        LogDebug("[Multiplayer] Region list received");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }

    #endregion

    #region IMatchmakingCallbacks

    public void OnJoinedLobby()
    {
        isInLobby = true;
        LogDebug("[Multiplayer] Joined Lobby - Ready for room operations!");
        OnReadyForRoomOperations?.Invoke();
    }

    public void OnLeftLobby()
    {
        isInLobby = false;
        LogDebug("[Multiplayer] Left Lobby");
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList) { }

    public void OnJoinedRoom()
    {
        LogDebug($"[Multiplayer] Joined Room: {PhotonNetwork.CurrentRoom.Name}");
        LogDebug($"[Multiplayer] Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");

        if (showDebugLogs)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                LogDebug($"[Multiplayer] Player: {player.NickName} (ID: {player.ActorNumber})");
            }
        }

        OnRoomJoinedSuccess?.Invoke();
    }

    public void OnLeftRoom()
    {
        LogDebug("[Multiplayer] Left Room");
        OnRoomLeft?.Invoke();
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Multiplayer] Create Room Failed: {message} ({returnCode})");
        OnRoomJoinedFailed?.Invoke($"Failed to create room: {message}");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        string errorMessage = GetJoinErrorMessage(returnCode, message);
        Debug.LogError($"[Multiplayer] Join Room Failed: {errorMessage}");
        OnRoomJoinedFailed?.Invoke(errorMessage);
    }

    public void OnJoinRandomFailed(short returnCode, string message) { }

    public void OnCreatedRoom()
    {
        string roomCode = PhotonNetwork.CurrentRoom.Name;
        LogDebug($"[Multiplayer] Room Created Successfully: {roomCode}");
        OnRoomCreated?.Invoke(roomCode);
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        LogDebug($"[Multiplayer] Player Joined: {newPlayer.NickName}");
        LogDebug($"[Multiplayer] Total Players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        OnPlayerJoined?.Invoke(newPlayer);
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        LogDebug($"[Multiplayer] Player Left: {otherPlayer.NickName}");
        OnPlayerLeft?.Invoke(otherPlayer);
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        LogDebug($"[Multiplayer] Master Client Switched: {newMasterClient.NickName}");
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList) { }

    #endregion

    #region Public Room Functions

    /// <summary>
    /// Creates a new room and returns a 6-character random room code
    /// </summary>
    public string CreateRoomWithCode()
    {
        if (!IsReadyForRoomOperations())
        {
            Debug.LogError("[Multiplayer] Not ready to create room!");
            Debug.LogError($"Connected: {isConnectedToPhoton}, InLobby: {isInLobby}, InRoom: {PhotonNetwork.InRoom}");
            return null;
        }

        string roomCode = GenerateRoomCode();
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true
        };

        LogDebug($"[Multiplayer] Creating Room: {roomCode}");
        PhotonNetwork.CreateRoom(roomCode, options);
        return roomCode;
    }

    /// <summary>
    /// Joins a room using the provided room code
    /// </summary>
    public bool JoinRoomWithCode(string roomCode)
    {
        if (!IsReadyForRoomOperations())
        {
            Debug.LogError("[Multiplayer] Not ready to join room!");
            return false;
        }

        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            Debug.LogError("[Multiplayer] Invalid room code! Must be 6 characters.");
            return false;
        }

        roomCode = roomCode.ToUpper();
        LogDebug($"[Multiplayer] Joining Room: {roomCode}");
        PhotonNetwork.JoinRoom(roomCode);
        return true;
    }

    /// <summary>
    /// Leaves the current room
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            LogDebug("[Multiplayer] Leaving Room...");
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            Debug.LogWarning("[Multiplayer] Not in a room!");
        }
    }

    #endregion

    #region Helper Methods

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string result = "";
        for (int i = 0; i < 6; i++)
        {
            result += chars[Random.Range(0, chars.Length)];
        }
        return result;
    }

    private string GetJoinErrorMessage(short returnCode, string message)
    {
        switch (returnCode)
        {
            case 32758:
                return "Room does not exist! Check the room code.";
            case 32764:
                return "Room is full!";
            default:
                return message;
        }
    }

    private bool IsReadyForRoomOperations()
    {
        return isConnectedToPhoton && isInLobby && !PhotonNetwork.InRoom;
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

    #endregion

    #region Public Status Properties
    // IsConnectedToPhoton - Returns true if connected to Photon server
    // IsInLobby - Returns true if in lobby and ready for room operations
    // IsInRoom - Returns true if currently in a room
    // IsMasterClient - Returns true if this client is the room host
    // CurrentRoomCode - Returns the current room code (6-character string)
    // CurrentPlayerCount - Returns number of players in current room
    // MaxPlayers - Returns maximum allowed players in current room
    #endregion
}