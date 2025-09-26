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

    [Header("Manual Testing - Set Room Code Here")]
    public string manualRoomCode = "";  // Inspector에서 설정 가능

    // Current room status
    private bool isConnectedToPhoton = false;
    private bool isInLobby = false;
    private string lastCreatedRoomCode = "";

    void Start()
    {
        Debug.Log("[Multiplayer] === STARTING PHOTON CONNECTION ===");
        PhotonNetwork.AddCallbackTarget(this);

#if UNITY_EDITOR
        if (PhotonNetwork.IsConnected || PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            Debug.LogWarning($"[Multiplayer] Editor - Disconnecting previous connection...");
            PhotonNetwork.Disconnect();
            Invoke("RetryConnection", 2f);
            return;
        }
#endif

        StartConnection();
    }

    void Update()
    {
        // TESTING CONTROLS - Remove for production
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[TEST] Creating room...");
            CreateRoomWithCode();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (!string.IsNullOrEmpty(manualRoomCode))
            {
                Debug.Log($"[TEST] Joining room: {manualRoomCode}");
                JoinRoomWithCode(manualRoomCode);
            }
            else if (!string.IsNullOrEmpty(lastCreatedRoomCode))
            {
                Debug.Log($"[TEST] Joining last created room: {lastCreatedRoomCode}");
                JoinRoomWithCode(lastCreatedRoomCode);
            }
            else
            {
                Debug.LogWarning("[TEST] No room code available! Set manualRoomCode in inspector or create a room first.");
            }
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.Log("[TEST] Leaving room...");
                PhotonNetwork.LeaveRoom();
            }
        }

        // Display current status every few seconds
        if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
        {
            Debug.Log($"[STATUS] Connected: {isConnectedToPhoton} | InLobby: {isInLobby} | InRoom: {PhotonNetwork.InRoom} | Players: {(PhotonNetwork.CurrentRoom?.PlayerCount ?? 0)}");
        }
    }

    void RetryConnection()
    {
        Debug.Log("[Multiplayer] Retrying connection...");
        StartConnection();
    }

    void StartConnection()
    {
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        Debug.Log($"[Multiplayer] Connecting as: {PhotonNetwork.NickName}");
        PhotonNetwork.ConnectUsingSettings();
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    #region IConnectionCallbacks

    public void OnConnected()
    {
        Debug.Log("*** OnConnected! ***");
    }

    public void OnConnectedToMaster()
    {
        isConnectedToPhoton = true;
        isInLobby = true; // Skip lobby, directly ready for room operations
        Debug.Log("*** OnConnectedToMaster! READY FOR ROOMS! ***");
        Debug.Log("[Multiplayer] Press '1' to CREATE room, '2' to JOIN room, '3' to LEAVE room");
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        isConnectedToPhoton = false;
        isInLobby = false;
        Debug.LogError($"[Multiplayer] Disconnected: {cause}");
    }

    public void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("[Multiplayer] Region list received");
    }

    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }

    #endregion

    #region IMatchmakingCallbacks

    public void OnJoinedLobby()
    {
        isInLobby = true;
        Debug.Log("*** OnJoinedLobby! ***");
    }

    public void OnLeftLobby()
    {
        isInLobby = false;
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList) { }

    public void OnJoinedRoom()
    {
        Debug.Log("*** JOINED ROOM SUCCESSFULLY! ***");
        Debug.Log($"*** Room: {PhotonNetwork.CurrentRoom.Name} ***");
        Debug.Log($"*** Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers} ***");

        foreach (var player in PhotonNetwork.PlayerList)
        {
            Debug.Log($"*** Player: {player.NickName} (ID: {player.ActorNumber}) ***");
        }

        Debug.Log("*** VOICE CHAT SHOULD INITIALIZE NOW! ***");
    }

    public void OnLeftRoom()
    {
        Debug.Log("*** LEFT ROOM ***");
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"*** CREATE ROOM FAILED: {message} ({returnCode}) ***");
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"*** JOIN ROOM FAILED: {message} ({returnCode}) ***");
        Debug.LogError($"*** Make sure the room code '{manualRoomCode}' is correct! ***");
    }

    public void OnJoinRandomFailed(short returnCode, string message) { }

    public void OnCreatedRoom()
    {
        Debug.Log("*** ROOM CREATED SUCCESSFULLY! ***");
        Debug.Log($"*** Room Code: {PhotonNetwork.CurrentRoom.Name} ***");
        lastCreatedRoomCode = PhotonNetwork.CurrentRoom.Name;
        Debug.Log($"*** SHARE THIS CODE: {lastCreatedRoomCode} ***");
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"*** PLAYER JOINED: {newPlayer.NickName} ***");
        Debug.Log($"*** Total Players: {PhotonNetwork.CurrentRoom.PlayerCount} ***");
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"*** PLAYER LEFT: {otherPlayer.NickName} ***");
    }

    public void OnMasterClientSwitched(Player newMasterClient) { }
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }

    #endregion

    #region Room Functions

    public string CreateRoomWithCode()
    {
        if (!IsReadyToCreateRoom())
        {
            Debug.LogError("*** NOT READY TO CREATE ROOM! ***");
            Debug.LogError($"Connected: {isConnectedToPhoton}, InLobby: {isInLobby}, InRoom: {PhotonNetwork.InRoom}");
            return null;
        }

        string roomCode = GenerateRoomCode();
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = false,
            IsOpen = true
        };

        Debug.Log($"*** CREATING ROOM: {roomCode} ***");
        PhotonNetwork.CreateRoom(roomCode, options);
        return roomCode;
    }

    public bool JoinRoomWithCode(string roomCode)
    {
        if (!IsReadyToJoinRoom())
        {
            Debug.LogError("*** NOT READY TO JOIN ROOM! ***");
            return false;
        }

        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
        {
            Debug.LogError("*** INVALID ROOM CODE! Must be 6 characters. ***");
            return false;
        }

        roomCode = roomCode.ToUpper();
        Debug.Log($"*** JOINING ROOM: {roomCode} ***");
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