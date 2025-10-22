using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// Bridge for MenuManager UI to Multiplayer with Photon features
/// Add photon features to MenuManager UI
/// </summary>
public class MenuMultiplayerBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Multiplayer multiplayerManager;

    [Header("Host Panel UI")]
    [SerializeField] private GameObject hostMultiplayerPanel;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI membersList;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButtonHost;

    [Header("Join Panel UI")]
    [SerializeField] private GameObject joinMultiplayerPanel;
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private Button joinButtonConfirm;
    [SerializeField] private Button backButtonJoin;

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;

    private MenuManager menuManager;
    private bool isInitialized = false;
    private bool isCreatingRoom = false;

    void Start()
    {
        // Find MenuManager
        menuManager = FindFirstObjectByType<MenuManager>();

        // Find Multiplayer manager if not assigned
        if (multiplayerManager == null)
        {
            multiplayerManager = FindFirstObjectByType<Multiplayer>();
        }

        if (multiplayerManager == null)
        {
            Debug.LogError("[MenuMultiplayerBridge] Multiplayer manager not found!");
            return;
        }

        // Auto-find UI elements if not assigned
        AutoFindUIElements();

        // Subscribe to Photon events
        multiplayerManager.OnReadyForRoomOperations += OnReadyForRoomOperations;

        // Setup button listeners
        SetupButtonListeners();

        // Initially hide host panel (will show after creating room)
        if (hostMultiplayerPanel != null)
        {
            hostMultiplayerPanel.SetActive(false);
        }

        isInitialized = true;
        Debug.Log("[MenuMultiplayerBridge] Bridge initialized successfully");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (multiplayerManager != null)
        {
            multiplayerManager.OnReadyForRoomOperations -= OnReadyForRoomOperations;
        }

        // Remove button listeners
        RemoveButtonListeners();
    }

    void Update()
    {
        if (!isInitialized || multiplayerManager == null)
            return;

        // Update room info when in room
        if (PhotonNetwork.InRoom && hostMultiplayerPanel != null && hostMultiplayerPanel.activeSelf)
        {
            UpdateRoomInfo();
        }
    }

    #region Auto Find UI

    private void AutoFindUIElements()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[MenuMultiplayerBridge] No Canvas found in scene!");
            return;
        }

        // Find panels if not assigned (including inactive objects)
        if (hostMultiplayerPanel == null)
            hostMultiplayerPanel = FindInChildren(canvas.transform, "HostMultiplayerPanel");

        if (joinMultiplayerPanel == null)
            joinMultiplayerPanel = FindInChildren(canvas.transform, "JoinMultiplayerPanel");

        if (mainMenuPanel == null)
            mainMenuPanel = FindInChildren(canvas.transform, "MainMenuPanel");

        // Find host panel elements
        if (hostMultiplayerPanel != null)
        {
            if (roomCodeText == null)
            {
                GameObject obj = GameObject.Find("RoomCodeText");
                if (obj != null) roomCodeText = obj.GetComponent<TextMeshProUGUI>();
            }

            if (membersList == null)
            {
                GameObject obj = GameObject.Find("MembersList");
                if (obj != null) membersList = obj.GetComponent<TextMeshProUGUI>();
            }

            if (startGameButton == null)
            {
                GameObject obj = GameObject.Find("StartGameButton");
                if (obj != null) startGameButton = obj.GetComponent<Button>();
            }

            if (backButtonHost == null)
            {
                GameObject obj = GameObject.Find("BackButtonHost");
                if (obj != null) backButtonHost = obj.GetComponent<Button>();
            }
        }

        // Find join panel elements
        if (joinMultiplayerPanel != null)
        {
            if (roomCodeInput == null)
            {
                GameObject obj = GameObject.Find("RoomCodeInput");
                if (obj != null) roomCodeInput = obj.GetComponent<TMP_InputField>();
            }

            if (joinButtonConfirm == null)
            {
                GameObject obj = GameObject.Find("JoinButtonConfirm");
                if (obj != null) joinButtonConfirm = obj.GetComponent<Button>();
            }

            if (backButtonJoin == null)
            {
                GameObject obj = GameObject.Find("BackButtonJoin");
                if (obj != null) backButtonJoin = obj.GetComponent<Button>();
            }
        }
    }

    private GameObject FindInChildren(Transform parent, string name)
    {
        // Check direct children first
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
                return child.gameObject;
        }

        // Then check all descendants
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            GameObject result = FindInChildren(child, name);
            if (result != null)
                return result;
        }

        return null;
    }

    #endregion

    #region Button Setup

    private void SetupButtonListeners()
    {
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);

        if (backButtonHost != null)
            backButtonHost.onClick.AddListener(OnBackFromHostClicked);

        if (joinButtonConfirm != null)
            joinButtonConfirm.onClick.AddListener(OnJoinRoomClicked);

        if (backButtonJoin != null)
            backButtonJoin.onClick.AddListener(OnBackFromJoinClicked);
    }

    private void RemoveButtonListeners()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(OnStartGameClicked);

        if (backButtonHost != null)
            backButtonHost.onClick.RemoveListener(OnBackFromHostClicked);

        if (joinButtonConfirm != null)
            joinButtonConfirm.onClick.RemoveListener(OnJoinRoomClicked);

        if (backButtonJoin != null)
            backButtonJoin.onClick.RemoveListener(OnBackFromJoinClicked);
    }

    #endregion

    #region Photon Callbacks

    private void OnReadyForRoomOperations()
    {
        Debug.Log("[MenuMultiplayerBridge] Ready for room operations");
    }

    #endregion

    #region Public Methods (Called by MenuManager buttons)

    /// <summary>
    /// Called when "Host Multiplayer" button clicked in main menu
    /// </summary>
    public void OnHostMultiplayerClicked()
    {
        Debug.Log("[MenuMultiplayerBridge] OnHostMultiplayerClicked called");

        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[MenuMultiplayerBridge] Already in room, showing host panel");
            ShowHostPanel();
            return;
        }

        if (isCreatingRoom)
        {
            Debug.LogWarning("[MenuMultiplayerBridge] Already creating room, please wait...");
            return;
        }

        if (!multiplayerManager.IsConnectedToPhoton())
        {
            Debug.LogWarning("[MenuMultiplayerBridge] Not connected to Photon yet!");
            return;
        }

        Debug.Log("[MenuMultiplayerBridge] Creating room...");

        isCreatingRoom = true;

        // Create room
        multiplayerManager.CreateRoom();
    }

    /// <summary>
    /// Called when successfully joined/created room
    /// Shows host panel with room info
    /// </summary>
    public void ShowHostPanel()
    {
        Debug.Log("[MenuMultiplayerBridge] ShowHostPanel called");

        isCreatingRoom = false;

        if (hostMultiplayerPanel != null)
        {
            // Hide other panels
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (joinMultiplayerPanel != null) joinMultiplayerPanel.SetActive(false);

            // Show host panel
            hostMultiplayerPanel.SetActive(true);

            // Update room info
            UpdateRoomInfo();

            Debug.Log("[MenuMultiplayerBridge] Showing host panel");
        }
        else
        {
            Debug.LogError("[MenuMultiplayerBridge] Host panel is NULL!");
        }
    }

    #endregion

    #region Button Handlers

    private void OnStartGameClicked()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("[MenuMultiplayerBridge] Only host can start the game!");
            return;
        }

        Debug.Log("[MenuMultiplayerBridge] Starting game...");

        PhotonNetwork.CurrentRoom.IsOpen = false;

        // Load game scene for all players
        PhotonNetwork.LoadLevel("SampleScene");
    }

    private void OnBackFromHostClicked()
    {
        Debug.Log("[MenuMultiplayerBridge] Leaving room...");

        // Leave room
        if (PhotonNetwork.InRoom)
        {
            multiplayerManager.LeaveRoom();
        }

        isCreatingRoom = false;

        // Return to main menu
        if (menuManager != null)
        {
            menuManager.ShowMainMenu();
        }
    }

    private void OnJoinRoomClicked()
    {
        if (!multiplayerManager.IsConnectedToPhoton())
        {
            Debug.LogWarning("[MenuMultiplayerBridge] Not connected to Photon yet!");
            return;
        }

        if (roomCodeInput == null || string.IsNullOrEmpty(roomCodeInput.text))
        {
            Debug.LogWarning("[MenuMultiplayerBridge] Please enter a room code!");
            return;
        }

        string roomCode = roomCodeInput.text.Trim().ToUpper();

        Debug.Log($"[MenuMultiplayerBridge] Joining room: {roomCode}");

        // Join room
        multiplayerManager.JoinRoom(roomCode);
    }

    private void OnBackFromJoinClicked()
    {
        Debug.Log("[MenuMultiplayerBridge] Back to main menu");

        // Return to main menu
        if (menuManager != null)
        {
            menuManager.ShowMainMenu();
        }
    }

    #endregion

    #region UI Updates

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

        // Update members list
        if (membersList != null)
        {
            string membersListText = "Players:\n";

            foreach (Player player in PhotonNetwork.PlayerList)
            {
                string indicator = player.IsMasterClient ? "[HOST]" : "";
                membersListText += $"{indicator} {player.NickName}\n";
            }

            membersList.text = membersListText;
        }

        // Update start button (only host can see it)
        if (startGameButton != null)
        {
            bool isMasterClient = PhotonNetwork.IsMasterClient;
            startGameButton.gameObject.SetActive(isMasterClient);
            startGameButton.interactable = isMasterClient;
        }
    }

    #endregion
}