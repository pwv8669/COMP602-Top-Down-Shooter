using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;

public class Multiplayer : MonoBehaviourPunCallbacks
{
    [Header("Room Settings")]
    [SerializeField] private int maxPlayersPerRoom = 4;
    [SerializeField] private string gameVersion = "1.0";

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    public event Action OnReadyForRoomOperations;

    private const int MAX_RETRY_ATTEMPTS = 3;
    private const float RETRY_DELAY = 2f;
    private const float FALLBACK_READY_DELAY = 5f;

    private int retryCount = 0;
    private MultiplayerUI uiManager;

    void Start()
    {
        // Find UI Manager
        uiManager = FindFirstObjectByType<MultiplayerUI>();

        LogDebug("[Multiplayer] Starting Photon connection...");

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

#if UNITY_EDITOR
        if (PhotonNetwork.IsConnected)
        {
            LogDebug("[Multiplayer] Editor - Disconnecting previous connection...");
            PhotonNetwork.Disconnect();
            StartCoroutine(RetryConnectionCoroutine());
            return;
        }
#endif

        StartConnection();
    }

    #region Connection Methods

    private void StartConnection()
    {
        if (PhotonNetwork.IsConnected)
        {
            LogDebug("[Multiplayer] Already connected");
            OnConnectedToMaster();
            return;
        }

        string playerName = "Player_" + UnityEngine.Random.Range(1000, 9999);
        PhotonNetwork.NickName = playerName;

        LogDebug($"[Multiplayer] Connecting as: {playerName}");

        PhotonNetwork.ConnectUsingSettings();

        StartCoroutine(FallbackReadyStateCoroutine());
    }

    private IEnumerator RetryConnectionCoroutine()
    {
        LogDebug($"[Multiplayer] Waiting {RETRY_DELAY} seconds before retry...");
        yield return new WaitForSeconds(RETRY_DELAY);

        RetryConnection();
    }

    private void RetryConnection()
    {
        LogDebug("[Multiplayer] Retrying connection...");
        StartConnection();
    }

    private IEnumerator FallbackReadyStateCoroutine()
    {
        yield return new WaitForSeconds(FALLBACK_READY_DELAY);

        if (!PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
        {
            LogWarning("[Multiplayer] Lobby join delayed - enabling UI anyway");
            OnReadyForRoomOperations?.Invoke();
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnConnected()
    {
        LogDebug("[Multiplayer] OnConnected");
    }

    public override void OnConnectedToMaster()
    {
        LogDebug("[Multiplayer] Connected to Master Server - Joining Lobby...");

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        else
        {
            OnJoinedLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        LogDebug("[Multiplayer] Joined Lobby - Ready for room operations");
        OnReadyForRoomOperations?.Invoke();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LogDebug($"[Multiplayer] Disconnected: {cause}");

        if (uiManager != null)
        {
            uiManager.OnConnectionFailed(cause.ToString());
        }

        if (retryCount < MAX_RETRY_ATTEMPTS)
        {
            retryCount++;
            LogDebug($"[Multiplayer] Retry attempt {retryCount}/{MAX_RETRY_ATTEMPTS}");
            StartCoroutine(RetryConnectionCoroutine());
        }
    }

    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        LogDebug("[Multiplayer] Region list received");
    }

    public override void OnCreatedRoom()
    {
        LogDebug($"[Multiplayer] Room created: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnJoinedRoom()
    {
        LogDebug($"[Multiplayer] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        LogDebug($"[Multiplayer] Players in room: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnJoinedRoom();
        }
    }

    public override void OnLeftRoom()
    {
        LogDebug("[Multiplayer] Left room");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnLeftRoom();
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        LogDebug($"[Multiplayer] Create room failed: {message}");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnCreateRoomFailed(message);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LogDebug($"[Multiplayer] Join room failed: {message}");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnJoinRoomFailed(message);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LogDebug($"[Multiplayer] Player joined: {newPlayer.NickName}");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnPlayerEnteredRoom(newPlayer);
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LogDebug($"[Multiplayer] Player left: {otherPlayer.NickName}");

        // Notify UI
        if (uiManager != null)
        {
            uiManager.OnPlayerLeftRoom(otherPlayer);
        }
    }

    #endregion

    #region Public Methods

    public void CreateRoom()
    {
        if (!IsConnectedToPhoton())
        {
            LogDebug("[Multiplayer] Not connected - cannot create room");
            return;
        }

        string roomCode = GenerateRoomCode();

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        LogDebug($"[Multiplayer] Creating room: {roomCode}");
        PhotonNetwork.CreateRoom(roomCode, roomOptions);
    }

    public void JoinRoom(string roomCode)
    {
        if (!IsConnectedToPhoton())
        {
            LogDebug("[Multiplayer] Not connected - cannot join room");
            return;
        }

        if (string.IsNullOrEmpty(roomCode))
        {
            LogDebug("[Multiplayer] Room code is empty");
            return;
        }

        LogDebug($"[Multiplayer] Joining room: {roomCode}");
        PhotonNetwork.JoinRoom(roomCode);
    }

    public void LeaveRoom()
    {
        if (!PhotonNetwork.InRoom)
        {
            LogDebug("[Multiplayer] Not in a room");
            return;
        }

        LogDebug("[Multiplayer] Leaving room");
        PhotonNetwork.LeaveRoom();
    }

    public bool IsConnectedToPhoton()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public string GetCurrentRoomCode()
    {
        if (PhotonNetwork.InRoom)
        {
            return PhotonNetwork.CurrentRoom.Name;
        }
        return string.Empty;
    }

    #endregion

    #region Helper Methods

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        char[] code = new char[6];

        for (int i = 0; i < code.Length; i++)
        {
            code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        }

        return new string(code);
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }

    private void LogWarning(string message)
    {
        if (showDebugLogs)
        {
            Debug.LogWarning(message);
        }
    }

    #endregion
}