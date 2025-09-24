using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Multiplayer : MonoBehaviour, IConnectionCallbacks, IMatchmakingCallbacks
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
        Debug.Log("[Multiplayer] === STARTING PHOTON CONNECTION ===");

        // Manual callback registration (reliable method)
        PhotonNetwork.AddCallbackTarget(this);

        // Unity Editor only - cleanup previous connections (not needed in builds)
#if UNITY_EDITOR
        if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            Debug.LogWarning($"[Multiplayer] Editor - Current state: {PhotonNetwork.NetworkClientState}");
            Debug.LogWarning("[Multiplayer] Editor - Disconnecting from previous connection...");
            PhotonNetwork.Disconnect();

            // Retry connection after disconnect
            Invoke("RetryConnection", 2f);
            return;
        }
#endif

        StartConnection();
    }

    void RetryConnection()
    {
        Debug.Log("[Multiplayer] Retrying connection after disconnect...");
        StartConnection();
    }

    void StartConnection()
    {
        if (showDebugLogs)
        {
            Debug.Log("[Multiplayer] Internet Reachability: " + Application.internetReachability);
            Debug.Log("[Multiplayer] PhotonNetwork.IsConnected: " + PhotonNetwork.IsConnected);
            Debug.Log("[Multiplayer] PhotonNetwork.NetworkClientState: " + PhotonNetwork.NetworkClientState);
        }

        // Basic settings
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        // Connection attempt
        bool result = PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[Multiplayer] ConnectUsingSettings result: " + result);

        // Start status checking
        InvokeRepeating("CheckConnectionStatus", 2f, 2f);
    }

    void CheckConnectionStatus()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[Status] State: {PhotonNetwork.NetworkClientState} | Connected: {PhotonNetwork.IsConnected} | InLobby: {PhotonNetwork.InLobby}");
        }

        // Stop status checking when connected to lobby
        if (PhotonNetwork.InLobby && isConnectedToPhoton && !isInLobby)
        {
            isInLobby = true;
            Debug.Log("[Multiplayer] Successfully joined lobby - Ready for gameplay!");
            CancelInvoke("CheckConnectionStatus");
        }
    }

    void OnDestroy()
    {
        // Manual callback removal
        PhotonNetwork.RemoveCallbackTarget(this);
        CancelInvoke("CheckConnectionStatus");
    }

    #region IConnectionCallbacks

    public void OnConnected()
    {
        Debug.Log("*** OnConnected CALLED! ***");
    }

    public void OnConnectedToMaster()
    {
        isConnectedToPhoton = true;
        Debug.Log("*** OnConnectedToMaster CALLED! ***");
        Debug.Log("[Multiplayer] Connected to Master Server! Joining lobby...");

        PhotonNetwork.JoinLobby();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        isConnectedToPhoton = false;
        isInLobby = false;
        Debug.LogError($"[Multiplayer] Disconnected! Cause: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("[Multiplayer] Region list received");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log("[Multiplayer] Custom auth response");
    }

    public void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError($"[Multiplayer] Custom auth failed: {debugMessage}");
    }

    #endregion

    #region IMatchmakingCallbacks

    public void OnJoinedLobby()
    {
        isInLobby = true;
        Debug.Log("*** OnJoinedLobby CALLED! ***");
        Debug.Log("[Multiplayer] Successfully joined lobby!");

        // Ready for gameplay - no auto test
    }

    public void OnLeftLobby()
    {
        isInLobby = false;
        Debug.Log("[Multiplayer] Left lobby");
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log($"[Multiplayer] Room list updated: {roomList.Count} rooms");
    }

    public void OnJoinedRoom()
    {
        Debug.Log($"[Multiplayer] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
    }

    public void OnLeftRoom()
    {
        Debug.Log("[Multiplayer] Left room");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Multiplayer] Create room failed: {returnCode} - {message}");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Multiplayer] Join room failed: {returnCode} - {message}");
    }

    public void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[Multiplayer] Join random failed (not used)");
    }

    public void OnCreatedRoom()
    {
        Debug.Log($"[Multiplayer] Created room: {PhotonNetwork.CurrentRoom.Name}");
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Multiplayer] Player joined: {newPlayer.NickName}");
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[Multiplayer] Player left: {otherPlayer.NickName}");
    }

    public void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log($"[Multiplayer] Master client: {newMasterClient.NickName}");
    }

    public void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("[Multiplayer] Friend list updated");
    }

    #endregion

    #region Room Functions

    public string CreateRoomWithCode()
    {
        if (!IsReadyToCreateRoom())
        {
            Debug.LogError("[Multiplayer] Not ready to create room!");
            return null;
        }

        string roomCode = GenerateRoomCode();
        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayersPerRoom;
        options.IsVisible = false;
        options.IsOpen = true;

        Debug.Log($"[Multiplayer] Creating room: {roomCode}");
        PhotonNetwork.CreateRoom(roomCode, options);
        return roomCode;
    }

    public bool JoinRoomWithCode(string roomCode)
    {
        if (!IsReadyToJoinRoom())
        {
            Debug.LogError("[Multiplayer] Not ready to join room!");
            return false;
        }

        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            Debug.LogError("[Multiplayer] Invalid room code!");
            return false;
        }

        roomCode = roomCode.ToUpper();
        Debug.Log($"[Multiplayer] Joining room: {roomCode}");
        PhotonNetwork.JoinRoom(roomCode);
        return true;
    }

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

    #region Status Methods

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

    #endregion
}