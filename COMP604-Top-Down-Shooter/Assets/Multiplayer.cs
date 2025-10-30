using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Multiplayer : MonoBehaviourPunCallbacks
{
    public int maxPlayersPerRoom = 2;
    public string gameVersion = "1.0";

    private bool isConnecting = false;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Connect()
    {
        if (isConnecting || PhotonNetwork.IsConnected) return;

        isConnecting = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.NickName = "Player_" + Random.Range(1000, 9999);

        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[Multiplayer] Connecting...");
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("[Multiplayer] Connected to Master");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[Multiplayer] Joined Lobby");
        isConnecting = false;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[Multiplayer] Joined Room: {PhotonNetwork.CurrentRoom.Name}");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[Multiplayer] Player joined: {newPlayer.NickName}");
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Multiplayer] Create room failed: {message}");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[Multiplayer] Join room failed: {message}");
    }

    #endregion

    #region Public Methods

    public void CreateRoomWithCode()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[Multiplayer] Not connected yet, connecting now...");
            Connect();
            return;
        }

        if (!PhotonNetwork.InLobby)
        {
            Debug.LogWarning("[Multiplayer] Not in lobby yet, waiting...");
            return;
        }

        string roomCode = GenerateRoomCode();

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = (byte)maxPlayersPerRoom,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(roomCode, options);
        Debug.Log($"[Multiplayer] Creating room: {roomCode}");
    }

    public void JoinRoomWithCode(string roomCode)
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("[Multiplayer] Not connected yet, connecting now...");
            Connect();
            return;
        }

        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6) return;

        PhotonNetwork.JoinRoom(roomCode.ToUpper());
    }

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        //if (PhotonNetwork.CurrentRoom.PlayerCount < maxPlayersPerRoom)
        //{
        //    Debug.LogWarning($"[Multiplayer] Waiting for more players ({PhotonNetwork.CurrentRoom.PlayerCount}/{maxPlayersPerRoom})");
        //    return;
        //}

        PhotonNetwork.LoadLevel("MultiplayerScene");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    #endregion

    #region Helper Methods

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";

        for (int i = 0; i < 6; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }

        return code;
    }

    #endregion

    #region Getters

    public bool IsConnected()
    {
        return PhotonNetwork.IsConnectedAndReady;
    }

    public bool IsInLobby()
    {
        return PhotonNetwork.InLobby;
    }

    public bool IsInRoom()
    {
        return PhotonNetwork.InRoom;
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }

    public string GetRoomCode()
    {
        return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : "";
    }

    public int GetPlayerCount()
    {
        return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    }

    #endregion
}