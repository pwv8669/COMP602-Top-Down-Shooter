using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class MultiplayerUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Connection Panel")]
    [SerializeField] private TextMeshProUGUI connectionStatusText;

    [Header("Menu Panel")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TextMeshProUGUI menuStatusText;

    [Header("Room Panel")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button leaveRoomButton;

    private Multiplayer multiplayerManager;
    private bool isInitialized = false;

    void Start()
    {
        // Find Multiplayer manager
        multiplayerManager = FindFirstObjectByType<Multiplayer>();

        if (multiplayerManager == null)
        {
            Debug.LogError("[MultiplayerUI] Multiplayer manager not found!");
            return;
        }

        // Subscribe to events
        multiplayerManager.OnReadyForRoomOperations += OnReadyForRoomOperations;

        // Setup button listeners
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);

        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(OnJoinRoomClicked);

        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);

        // CRITICAL: Show only connection panel at start
        ShowConnectionPanel();

        UpdateStatus("Connecting to server...");

        isInitialized = true;
        Debug.Log("[MultiplayerUI] UI initialized - showing connection panel only");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (multiplayerManager != null)
        {
            multiplayerManager.OnReadyForRoomOperations -= OnReadyForRoomOperations;
        }

        // Remove button listeners
        if (createRoomButton != null)
            createRoomButton.onClick.RemoveListener(OnCreateRoomClicked);

        if (joinRoomButton != null)
            joinRoomButton.onClick.RemoveListener(OnJoinRoomClicked);

        if (leaveRoomButton != null)
            leaveRoomButton.onClick.RemoveListener(OnLeaveRoomClicked);
    }

    void Update()
    {
        if (!isInitialized || multiplayerManager == null)
            return;

        // Update room info if in a room
        if (PhotonNetwork.InRoom)
        {
            UpdateRoomInfo();
        }
    }

    #region Panel Management

    private void ShowConnectionPanel()
    {
        SetPanelActive(connectionPanel, true);
        SetPanelActive(menuPanel, false);
        SetPanelActive(roomPanel, false);

        Debug.Log("[MultiplayerUI] Showing connection panel");
    }

    private void ShowMenuPanel()
    {
        SetPanelActive(connectionPanel, false);
        SetPanelActive(menuPanel, true);
        SetPanelActive(roomPanel, false);

        Debug.Log("[MultiplayerUI] Showing menu panel");
    }

    private void ShowRoomPanel()
    {
        SetPanelActive(connectionPanel, false);
        SetPanelActive(menuPanel, false);
        SetPanelActive(roomPanel, true);

        Debug.Log("[MultiplayerUI] Showing room panel");
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    #endregion

    #region Multiplayer Callbacks

    private void OnReadyForRoomOperations()
    {
        Debug.Log("[MultiplayerUI] Ready! Showing menu panel.");

        // Hide connection panel and show menu
        ShowMenuPanel();

        UpdateStatus("Ready! Create or join a room.");
        UpdateButtonStates();
    }

    public void OnJoinedRoom()
    {
        Debug.Log("[MultiplayerUI] Joined room - showing room panel");

        // Show room panel
        ShowRoomPanel();

        UpdateRoomInfo();
        UpdatePlayerList();
    }

    public void OnLeftRoom()
    {
        Debug.Log("[MultiplayerUI] Left room - returning to menu");

        // Return to menu panel
        ShowMenuPanel();

        UpdateStatus("Left room. Create or join another room.");
        UpdateButtonStates();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[MultiplayerUI] Player joined: {newPlayer.NickName}");
        UpdatePlayerList();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[MultiplayerUI] Player left: {otherPlayer.NickName}");
        UpdatePlayerList();
    }

    #endregion

    #region Button Handlers

    private void OnCreateRoomClicked()
    {
        if (multiplayerManager == null || !multiplayerManager.IsConnectedToPhoton())
        {
            UpdateStatus("Not connected to server!");
            return;
        }

        Debug.Log("[MultiplayerUI] Create room button clicked");

        UpdateStatus("Creating room...");
        createRoomButton.interactable = false;

        multiplayerManager.CreateRoom();
    }

    private void OnJoinRoomClicked()
    {
        if (multiplayerManager == null || !multiplayerManager.IsConnectedToPhoton())
        {
            UpdateStatus("Not connected to server!");
            return;
        }

        if (roomCodeInput == null || string.IsNullOrEmpty(roomCodeInput.text))
        {
            UpdateStatus("Please enter a room code!");
            return;
        }

        string roomCode = roomCodeInput.text.Trim().ToUpper();

        Debug.Log($"[MultiplayerUI] Join room button clicked: {roomCode}");

        UpdateStatus($"Joining room {roomCode}...");
        joinRoomButton.interactable = false;

        multiplayerManager.JoinRoom(roomCode);
    }

    private void OnLeaveRoomClicked()
    {
        if (multiplayerManager == null)
            return;

        Debug.Log("[MultiplayerUI] Leave room button clicked");

        leaveRoomButton.interactable = false;
        multiplayerManager.LeaveRoom();
    }

    #endregion

    #region UI Updates

    private void UpdateButtonStates()
    {
        bool isConnected = multiplayerManager != null && multiplayerManager.IsConnectedToPhoton();
        bool isInRoom = PhotonNetwork.InRoom;

        if (createRoomButton != null)
            createRoomButton.interactable = isConnected && !isInRoom;

        if (joinRoomButton != null)
            joinRoomButton.interactable = isConnected && !isInRoom;

        if (leaveRoomButton != null)
            leaveRoomButton.interactable = isInRoom;
    }

    private void UpdateRoomInfo()
    {
        if (!PhotonNetwork.InRoom)
            return;

        // Update room code
        if (roomCodeText != null)
        {
            string roomName = PhotonNetwork.CurrentRoom.Name;
            roomCodeText.text = $"Room Code: {roomName}";
        }

        // Update player count
        if (playerCountText != null)
        {
            int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
            playerCountText.text = $"Players: {currentPlayers}/{maxPlayers}";
        }
    }

    private void UpdatePlayerList()
    {
        if (!PhotonNetwork.InRoom || playerListText == null)
            return;

        string playerList = "Players:\n";

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            string indicator = player.IsMasterClient ? "★" : "•";
            playerList += $"{indicator} {player.NickName}\n";
        }

        playerListText.text = playerList;

        Debug.Log($"[MultiplayerUI] Updated player list:\n{playerList}");
    }

    private void UpdateStatus(string message)
    {
        // Update connection status text
        if (connectionStatusText != null && connectionPanel.activeSelf)
        {
            connectionStatusText.text = message;
        }

        // Update menu status text
        if (menuStatusText != null && menuPanel.activeSelf)
        {
            menuStatusText.text = message;
        }

        Debug.Log($"[MultiplayerUI] Status: {message}");
    }

    #endregion

    #region Public Methods

    public void OnConnectionFailed(string reason)
    {
        UpdateStatus($"Connection failed: {reason}");

        // Stay on connection panel and show error
        ShowConnectionPanel();

        if (createRoomButton != null)
            createRoomButton.interactable = false;

        if (joinRoomButton != null)
            joinRoomButton.interactable = false;
    }

    public void OnJoinRoomFailed(string reason)
    {
        UpdateStatus($"Failed to join room: {reason}");

        // Return to menu panel
        ShowMenuPanel();

        UpdateButtonStates();
    }

    public void OnCreateRoomFailed(string reason)
    {
        UpdateStatus($"Failed to create room: {reason}");

        // Return to menu panel
        ShowMenuPanel();

        UpdateButtonStates();
    }

    #endregion
}