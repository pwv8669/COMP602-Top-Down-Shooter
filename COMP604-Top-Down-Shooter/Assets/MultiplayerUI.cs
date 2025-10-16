using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultiplayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Multiplayer multiplayer;

    [Header("UI Panels")]
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject roomPanel;

    [Header("Menu Panel Elements")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_InputField roomCodeInput;

    [Header("Room Panel Elements")]
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private TextMeshProUGUI playerListText;

    [Header("Status Elements")]
    [SerializeField] private TextMeshProUGUI statusText;

    void Start()
    {
        // Find Multiplayer if not assigned
        if (multiplayer == null)
        {
            multiplayer = FindFirstObjectByType<Multiplayer>();
        }

        if (multiplayer == null)
        {
            Debug.LogError("[MultiplayerUI] Multiplayer script not found!");
            return;
        }

        // Subscribe to events
        multiplayer.OnReadyForRoomOperations += OnReadyForRoomOperations;
        multiplayer.OnRoomCreated += OnRoomCreated;
        multiplayer.OnRoomJoinedSuccess += OnRoomJoined;
        multiplayer.OnRoomJoinedFailed += OnRoomJoinFailed;
        multiplayer.OnRoomLeft += OnRoomLeft;
        multiplayer.OnPlayerJoined += OnPlayerJoined;
        multiplayer.OnPlayerLeft += OnPlayerLeft;

        // Setup button listeners
        createRoomButton?.onClick.AddListener(OnCreateRoomClicked);
        joinRoomButton?.onClick.AddListener(OnJoinRoomClicked);
        leaveRoomButton?.onClick.AddListener(OnLeaveRoomClicked);

        // Initial UI state
        ShowConnectionPanel();
        UpdateStatus("Connecting to server...");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (multiplayer != null)
        {
            multiplayer.OnReadyForRoomOperations -= OnReadyForRoomOperations;
            multiplayer.OnRoomCreated -= OnRoomCreated;
            multiplayer.OnRoomJoinedSuccess -= OnRoomJoined;
            multiplayer.OnRoomJoinedFailed -= OnRoomJoinFailed;
            multiplayer.OnRoomLeft -= OnRoomLeft;
            multiplayer.OnPlayerJoined -= OnPlayerJoined;
            multiplayer.OnPlayerLeft -= OnPlayerLeft;
        }

        // Remove button listeners
        createRoomButton?.onClick.RemoveListener(OnCreateRoomClicked);
        joinRoomButton?.onClick.RemoveListener(OnJoinRoomClicked);
        leaveRoomButton?.onClick.RemoveListener(OnLeaveRoomClicked);
    }

    #region Button Handlers

    private void OnCreateRoomClicked()
    {
        UpdateStatus("Creating room...");
        createRoomButton.interactable = false;
        multiplayer.CreateRoomWithCode();
    }

    private void OnJoinRoomClicked()
    {
        string code = roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code) || code.Length != 6)
        {
            UpdateStatus("Please enter a valid 6-character room code!");
            return;
        }

        UpdateStatus($"Joining room {code}...");
        joinRoomButton.interactable = false;
        multiplayer.JoinRoomWithCode(code);
    }

    private void OnLeaveRoomClicked()
    {
        UpdateStatus("Leaving room...");
        leaveRoomButton.interactable = false;
        multiplayer.LeaveRoom();
    }

    #endregion

    #region Multiplayer Event Handlers

    private void OnReadyForRoomOperations()
    {
        ShowMenuPanel();
        UpdateStatus("Ready! Create or join a room.");
    }

    private void OnRoomCreated(string roomCode)
    {
        UpdateStatus($"Room created: {roomCode}");
        // Room joined callback will handle UI transition
    }

    private void OnRoomJoined()
    {
        ShowRoomPanel();
        UpdateRoomInfo();
        UpdateStatus("Connected to room!");
    }

    private void OnRoomJoinFailed(string errorMessage)
    {
        UpdateStatus($"Failed: {errorMessage}");
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
    }

    private void OnRoomLeft()
    {
        ShowMenuPanel();
        UpdateStatus("Left the room");
        leaveRoomButton.interactable = true;
    }

    private void OnPlayerJoined(Photon.Realtime.Player player)
    {
        UpdateRoomInfo();
        UpdateStatus($"{player.NickName} joined!");
    }

    private void OnPlayerLeft(Photon.Realtime.Player player)
    {
        UpdateRoomInfo();
        UpdateStatus($"{player.NickName} left");
    }

    #endregion

    #region UI Update Methods

    private void ShowConnectionPanel()
    {
        connectionPanel?.SetActive(true);
        menuPanel?.SetActive(false);
        roomPanel?.SetActive(false);
    }

    private void ShowMenuPanel()
    {
        connectionPanel?.SetActive(false);
        menuPanel?.SetActive(true);
        roomPanel?.SetActive(false);

        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        if (roomCodeInput != null)
        {
            roomCodeInput.text = "";
        }
    }

    private void ShowRoomPanel()
    {
        connectionPanel?.SetActive(false);
        menuPanel?.SetActive(false);
        roomPanel?.SetActive(true);

        leaveRoomButton.interactable = true;
    }

    private void UpdateRoomInfo()
    {
        if (!multiplayer.IsInRoom) return;

        // Update room code
        if (roomCodeText != null)
        {
            roomCodeText.text = $"Room Code: {multiplayer.CurrentRoomCode}";
        }

        // Update player count
        if (playerCountText != null)
        {
            playerCountText.text = $"Players: {multiplayer.CurrentPlayerCount}/{multiplayer.MaxPlayers}";
        }

        // Update player list
        if (playerListText != null)
        {
            string playerList = "Players:\n";
            foreach (var player in Photon.Pun.PhotonNetwork.PlayerList)
            {
                string role = player.IsMasterClient ? " (Host)" : "";
                playerList += $"• {player.NickName}{role}\n";
            }
            playerListText.text = playerList;
        }
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[MultiplayerUI] {message}");
    }

    #endregion

    #region Optional: Keyboard Shortcuts for Testing
#if UNITY_EDITOR
    void Update()
    {
        // Optional: Keep keyboard shortcuts for editor testing
        if (Input.GetKeyDown(KeyCode.Alpha1) && multiplayer.IsInLobby && !multiplayer.IsInRoom)
        {
            OnCreateRoomClicked();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && multiplayer.IsInLobby && !multiplayer.IsInRoom)
        {
            OnJoinRoomClicked();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3) && multiplayer.IsInRoom)
        {
            OnLeaveRoomClicked();
        }
    }
#endif
    #endregion
}