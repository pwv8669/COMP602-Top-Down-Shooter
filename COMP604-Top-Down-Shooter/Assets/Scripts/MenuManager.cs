using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private GameObject settingsPanel;
    private GameObject hostMultiplayerPanel;
    private GameObject joinMultiplayerPanel;

    private Slider volumeSlider;
    private Text volumeText;
    private Dropdown graphicsDropdown;

    // Multiplayer references
    private Multiplayer multiplayer;
    private TMP_Text roomCodeText;
    private TMP_Text membersText;
    private TMP_Text membersList;
    private TMP_InputField roomCodeInput;
    private Button startGameButton;

    private bool isInMultiplayerPanel = false;

    void Start()
    {
        // Find all panels automatically
        FindPanels();

        // Find settings UI elements
        FindSettingsElements();

        // Find multiplayer UI elements
        FindMultiplayerElements();

        // Setup events
        SetupEvents();

        // Show main menu at start
        ShowMainMenu();
    }

    void FindPanels()
    {
        mainMenuPanel = GameObject.Find("MainMenuPanel");
        settingsPanel = GameObject.Find("SettingsPanel");
        hostMultiplayerPanel = GameObject.Find("HostMultiplayerPanel");
        joinMultiplayerPanel = GameObject.Find("JoinMultiplayerPanel");
    }

    void FindSettingsElements()
    {
        // Find volume slider and text
        GameObject volumeSliderObj = GameObject.Find("VolumeSlider");
        if (volumeSliderObj != null)
        {
            volumeSlider = volumeSliderObj.GetComponent<Slider>();

            // Find the text child of the slider
            Transform volumeTextTransform = volumeSliderObj.transform.Find("VolumeValueText");
            if (volumeTextTransform != null)
                volumeText = volumeTextTransform.GetComponent<Text>();
        }

        // Find graphics dropdown
        GameObject graphicsDropdownObj = GameObject.Find("GraphicsDropdown");
        if (graphicsDropdownObj != null)
            graphicsDropdown = graphicsDropdownObj.GetComponent<Dropdown>();
    }

    void FindMultiplayerElements()
    {
        multiplayer = FindFirstObjectByType<Multiplayer>();

        if (multiplayer == null)
        {
            Debug.LogWarning("[MenuManager] Multiplayer component not found! Create a NetworkManager object.");
            return;
        }

        // Find in HostMultiplayerPanel (even if inactive)
        if (hostMultiplayerPanel != null)
        {
            roomCodeText = hostMultiplayerPanel.transform.Find("RoomCodeText")?.GetComponent<TMP_Text>();
            Debug.Log($"[MenuManager] RoomCodeText found: {roomCodeText != null}");

            membersText = hostMultiplayerPanel.transform.Find("MembersText")?.GetComponent<TMP_Text>();
            Debug.Log($"[MenuManager] MembersText found: {membersText != null}");

            membersList = hostMultiplayerPanel.transform.Find("MembersList")?.GetComponent<TMP_Text>();
            Debug.Log($"[MenuManager] MembersList found: {membersList != null}");

            startGameButton = hostMultiplayerPanel.transform.Find("StartGameButton")?.GetComponent<Button>();
            Debug.Log($"[MenuManager] StartGameButton found: {startGameButton != null}");
        }

        // Find in JoinMultiplayerPanel (even if inactive)
        if (joinMultiplayerPanel != null)
        {
            roomCodeInput = joinMultiplayerPanel.transform.Find("RoomCodeInput")?.GetComponent<TMP_InputField>();
        }
    }

    void SetupEvents()
    {
        // Setup volume slider
        if (volumeSlider != null && volumeText != null)
        {
            volumeSlider.onValueChanged.AddListener(UpdateVolumeText);
            UpdateVolumeText(volumeSlider.value);
        }

        // Setup graphics dropdown
        if (graphicsDropdown != null)
        {
            graphicsDropdown.onValueChanged.AddListener(OnGraphicsChanged);
        }
    }

    void Update()
    {
        // Only update UI when in multiplayer panel
        if (isInMultiplayerPanel && multiplayer != null && multiplayer.IsInRoom())
        {
            // Turn into waiting room if guest joined
            if (joinMultiplayerPanel != null && joinMultiplayerPanel.activeSelf)
            {
                Debug.Log("[MenuManager] Guest joined room! Switching to waiting room...");
                joinMultiplayerPanel.SetActive(false);
                if (hostMultiplayerPanel != null)
                    hostMultiplayerPanel.SetActive(true);
            }

            UpdateMultiplayerUI();
        }
    }

    void UpdateMultiplayerUI()
    {
        if (roomCodeText != null)
        {
            string code = multiplayer.GetRoomCode();
            roomCodeText.text = $"Room Code: {code}";
        }

        if (membersText != null)
        {
            int count = multiplayer.GetPlayerCount();
            membersText.text = $"Players: {count}/2";
        }

        if (membersList != null)
        {
            string playerNames = "";
            foreach (var player in Photon.Pun.PhotonNetwork.PlayerList)
            {
                playerNames += player.NickName + "\n";
            }
            membersList.text = playerNames;
        }

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(multiplayer.IsMasterClient());
    }

    void UpdateVolumeText(float value)
    {
        if (volumeText != null)
            volumeText.text = $"Volume: {Mathf.RoundToInt(value * 100)}%";
    }

    // ===== MENU NAVIGATION FUNCTIONS =====
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ShowSettings()
    {
        HideAllPanels();
        isInMultiplayerPanel = false;
        if (settingsPanel != null) settingsPanel.SetActive(true);
        HideTitle();
    }

    public void ShowHostMultiplayer()
    {
        HideAllPanels();
        if (hostMultiplayerPanel != null) hostMultiplayerPanel.SetActive(true);
        HideTitle();

        isInMultiplayerPanel = true;

        if (multiplayer != null)
        {
            // Connect and create room
            if (!multiplayer.IsConnected())
            {
                multiplayer.Connect();
                StartCoroutine(WaitAndCreateRoom());
            }
            else
            {
                multiplayer.CreateRoomWithCode();
            }
        }
    }

    IEnumerator WaitAndCreateRoom()
    {
        // Wait for lobby join
        float timeout = 10f;
        float elapsed = 0f;

        Debug.Log("[MenuManager] Waiting for lobby...");

        while (!multiplayer.IsInLobby() && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }

        if (multiplayer.IsInLobby())
        {
            Debug.Log("[MenuManager] Lobby joined! Creating room...");
            multiplayer.CreateRoomWithCode();
        }
        else
        {
            Debug.LogError("[MenuManager] Failed to join lobby!");
        }
    }

    public void ShowJoinMultiplayer()
    {
        HideAllPanels();
        if (joinMultiplayerPanel != null) joinMultiplayerPanel.SetActive(true);
        HideTitle();

        isInMultiplayerPanel = true;

        if (multiplayer != null && !multiplayer.IsConnected())
        {
            multiplayer.Connect();
        }
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        isInMultiplayerPanel = false;
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ShowTitle();
    }

    // ===== MULTIPLAYER FUNCTIONS =====
    public void OnJoinButtonClicked()
    {
        if (multiplayer == null || roomCodeInput == null) return;

        string code = roomCodeInput.text;
        multiplayer.JoinRoomWithCode(code);
    }

    public void OnStartGameButtonClicked()
    {
        if (multiplayer != null)
            multiplayer.StartGame();
    }

    public void OnBackFromMultiplayer()
    {
        if (multiplayer != null && multiplayer.IsInRoom())
            multiplayer.LeaveRoom();

        ShowMainMenu();
    }

    public void OnGraphicsChanged(int index)
    {
        Debug.Log($"Graphics quality changed to index: {index}");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (hostMultiplayerPanel != null) hostMultiplayerPanel.SetActive(false);
        if (joinMultiplayerPanel != null) joinMultiplayerPanel.SetActive(false);
    }

    void HideTitle()
    {
        GameObject title = GameObject.Find("TitleText");
        if (title != null) title.SetActive(false);
    }

    void ShowTitle()
    {
        GameObject title = GameObject.Find("TitleText");
        if (title != null) title.SetActive(true);
    }
}