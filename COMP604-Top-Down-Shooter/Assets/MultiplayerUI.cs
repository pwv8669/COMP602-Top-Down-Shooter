using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class MultiplayerUI : MonoBehaviour
{
    // Removed connectionPanel and menuPanel
    [Header("Panel References")]
    [SerializeField] private GameObject roomPanel;

    // Removed Connection/Menu related UI

    [Header("Room Panel")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button startGameButton;

    // Added status text for displaying connection and room status
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;

    private Multiplayer multiplayerManager;
    private bool isInitialized = false;

    // Cache variables for change detection
    private int lastPlayerCount = -1;
    private bool lastIsMasterClient = false;

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
        if (leaveRoomButton != null)
            leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);

        // Hide roomPanel at start while waiting for connection
        if (roomPanel != null)
            roomPanel.SetActive(false);

        UpdateStatus("Connecting to server...");

        isInitialized = true;
        Debug.Log("[MultiplayerUI] UI initialized - waiting for connection");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (multiplayerManager != null)
        {
            multiplayerManager.OnReadyForRoomOperations -= OnReadyForRoomOperations;
        }

        // Remove button listeners
        if (leaveRoomButton != null)
            leaveRoomButton.onClick.RemoveListener(OnLeaveRoomClicked);

        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);
    }

    void Update()
    {
        if (!isInitialized || multiplayerManager == null)
            return;

        // Update room info only when in a room and values changed
        if (PhotonNetwork.InRoom)
        {
            int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            if (currentPlayerCount != lastPlayerCount)
            {
                lastPlayerCount = currentPlayerCount;
                UpdateRoomInfo();
            }

            bool currentIsMasterClient = PhotonNetwork.IsMasterClient;
            if (currentIsMasterClient != lastIsMasterClient)
            {
                lastIsMasterClient = currentIsMasterClient;
                UpdateStartButtonVisibility();
            }
        }
        else
        {
            lastPlayerCount = -1;
            lastIsMasterClient = false;
        }
    }

    #region Multiplayer Callbacks

    // Automatically create or join room when connected
    private void OnReadyForRoomOperations()
    {
        Debug.Log("[MultiplayerUI] Connected! Checking multiplayer mode...");

        string mode = PlayerPrefs.GetString("MultiplayerMode", "");

        if (mode == "Host")
        {
            PlayerPrefs.DeleteKey("MultiplayerMode");
            UpdateStatus("Creating room...");
            StartCoroutine(AutoCreateRoom());
        }
        else if (mode == "Join")
        {
            string roomCode = PlayerPrefs.GetString("RoomCode", "");
            PlayerPrefs.DeleteKey("MultiplayerMode");
            PlayerPrefs.DeleteKey("RoomCode");

            if (!string.IsNullOrEmpty(roomCode))
            {
                UpdateStatus($"Joining room {roomCode}...");
                multiplayerManager.JoinRoom(roomCode);
            }
            else
            {
                UpdateStatus("Error: No room code!");
            }
        }
    }

    // Auto create room with delay
    private IEnumerator AutoCreateRoom()
    {
        yield return new WaitForSeconds(0.5f);
        multiplayerManager.CreateRoom();
    }

    public void OnJoinedRoom()
    {
        Debug.Log("[MultiplayerUI] Joined room - showing room panel");

        // Show room panel
        if (roomPanel != null)
            roomPanel.SetActive(true);

        lastPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        lastIsMasterClient = PhotonNetwork.IsMasterClient;

        UpdateRoomInfo();
        UpdatePlayerList();
        UpdateStartButtonVisibility();
        UpdateStatus("In room!");

        Debug.Log($"[MultiplayerUI] Room Code: {PhotonNetwork.CurrentRoom.Name}");
        Debug.Log($"[MultiplayerUI] Player Count: {PhotonNetwork.CurrentRoom.PlayerCount}");
    }

    public void OnLeftRoom()
    {
        Debug.Log("[MultiplayerUI] Left room - returning to main menu");

        // Return to main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[MultiplayerUI] Player joined: {newPlayer.NickName}");
        lastPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        UpdatePlayerList();
        UpdateStartButtonVisibility();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[MultiplayerUI] Player left: {otherPlayer.NickName}");
        lastPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        UpdatePlayerList();
        UpdateStartButtonVisibility();
    }

    #endregion

    #region Button Handlers

    private void OnLeaveRoomClicked()
    {
        if (multiplayerManager == null)
            return;

        Debug.Log("[MultiplayerUI] Leave room button clicked");
        leaveRoomButton.interactable = false;
        multiplayerManager.LeaveRoom();
    }

    private void OnStartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[MultiplayerUI] Only host can start the game!");
            return;
        }

        Debug.Log("[MultiplayerUI] Host starting game...");
        UpdateStatus("Starting game...");

        // Add game start logic here
        // Example: PhotonNetwork.LoadLevel("GameScene");
    }

    #endregion

    #region UI Updates

    private void UpdateRoomInfo()
    {
        if (!PhotonNetwork.InRoom)
            return;

        if (roomCodeText != null)
        {
            string roomName = PhotonNetwork.CurrentRoom.Name;
            roomCodeText.text = $"Room Code: {roomName}";
        }

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
            string indicator = player.IsMasterClient ? "[HOST]" : "-";
            playerList += $"{indicator} {player.NickName}\n";
        }

        playerListText.text = playerList;
    }

    private void UpdateStartButtonVisibility()
    {
        if (startGameButton == null)
            return;

        bool isMasterClient = PhotonNetwork.IsMasterClient;
        startGameButton.gameObject.SetActive(isMasterClient);
        startGameButton.interactable = isMasterClient;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[MultiplayerUI] Status: {message}");
    }

    #endregion

    #region Public Methods

    public void OnConnectionFailed(string reason)
    {
        UpdateStatus($"Connection failed: {reason}");

        // Return to main menu on connection failure
        StartCoroutine(ReturnToMenuAfterDelay(3f));
    }

    public void OnJoinRoomFailed(string reason)
    {
        UpdateStatus($"Failed to join room: {reason}");
        StartCoroutine(ReturnToMenuAfterDelay(3f));
    }

    public void OnCreateRoomFailed(string reason)
    {
        UpdateStatus($"Failed to create room: {reason}");
        StartCoroutine(ReturnToMenuAfterDelay(3f));
    }

    private IEnumerator ReturnToMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    #endregion
}