using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.SceneManagement;

public class MultiplayerUI : MonoBehaviour
{
    [Header("Panel References")]
    [SerializeField] private Multiplayer multiplayerManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject roomPanel;

    [Header("Room Panel Elements")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI playerListText;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button startGameButton;

    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;

    private bool isInitialized = false;

    // Cache variables for change detection
    private int lastPlayerCount = -1;
    private bool lastIsMasterClient = false;

    void Start()
    {

        if (multiplayerManager == null)
        {
            Debug.LogError("[MultiplayerUI] Multiplayer manager not found!");
            return;
        }

        // Subscribe to events
        multiplayerManager.OnReadyForRoomOperations += OnReadyForRoomOperations;

        // Setup button listeners
        Debug.Log("[DEBUG] Setting up button listeners...");

        // Setup button listeners
        if (leaveRoomButton != null)
        {
            Debug.Log("[DEBUG] Leave button found! Adding listener..."); // ← 추가!
            leaveRoomButton.onClick.AddListener(OnLeaveRoomClicked);
            Debug.Log($"[DEBUG] Leave button interactable: {leaveRoomButton.interactable}"); // ← 추가!
        }
        else
        {
            Debug.LogError("[DEBUG] Leave button is NULL!"); // ← 추가!
        }

        if (startGameButton != null)
        {
            Debug.Log("[DEBUG] Start button found! Adding listener..."); // ← 추가!
            startGameButton.onClick.AddListener(OnStartGameClicked);
            Debug.Log($"[DEBUG] Start button interactable: {startGameButton.interactable}"); // ← 추가!
        }
        else
        {
            Debug.LogError("[DEBUG] Start button is NULL!"); // ← 추가!
        }

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

        // If room panel is active, force unlock cursor
        if (roomPanel != null && roomPanel.activeSelf)
        {
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                Debug.Log("[MultiplayerUI] Force unlocking cursor in Update!");
            }
        }

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

        // Force unlock cursor BEFORE showing UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Show room panel
        if (roomPanel != null)
            roomPanel.SetActive(true);

        // Force unlock again to be sure
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"[DEBUG] Cursor unlocked! lockState: {Cursor.lockState}, visible: {Cursor.visible}");

        // Re-enable leave button when joined room
        if (leaveRoomButton != null)
            leaveRoomButton.interactable = true;

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

        if (roomPanel != null)
            roomPanel.SetActive(false);

        // Keep cursor visible and free for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Return to main menu
        SceneManager.LoadScene("MainMenu");

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
        Debug.Log("=== LEAVE BUTTON CLICKED! ===");
        Debug.Log($"[DEBUG] Time.unscaledTime: {Time.unscaledTime}");
        Debug.Log($"[DEBUG] roomPanel active: {roomPanel != null && roomPanel.activeSelf}");
        Debug.Log($"[DEBUG] Button interactable: {leaveRoomButton != null && leaveRoomButton.interactable}");
        Debug.Log($"[DEBUG] EventSystem: {UnityEngine.EventSystems.EventSystem.current != null}");
        Debug.Log($"[DEBUG] Cursor state: lockState={Cursor.lockState}, visible={Cursor.visible}");

        if (multiplayerManager == null)
            return;

        Debug.Log("[MultiplayerUI] Leave room button clicked");

        // Disable button temporarily to prevent multiple clicks
        if (leaveRoomButton != null)
            leaveRoomButton.interactable = false;

        UpdateStatus("Leaving room...");
        multiplayerManager.LeaveRoom();
    }

    private void OnStartGameClicked()
    {
        Debug.Log("=== START BUTTON CLICKED! ===");

        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[MultiplayerUI] Only host can start the game!");
            return;
        }

        Debug.Log("[MultiplayerUI] Host starting game...");
        UpdateStatus("Starting game...");

        // Disable button to prevent multiple clicks
        if (startGameButton != null)
            startGameButton.interactable = false;

        // Call StartGame from Multiplayer manager
        multiplayerManager.StartGame();
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

        // Only enable if master client and in room
        startGameButton.interactable = isMasterClient && PhotonNetwork.InRoom;
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