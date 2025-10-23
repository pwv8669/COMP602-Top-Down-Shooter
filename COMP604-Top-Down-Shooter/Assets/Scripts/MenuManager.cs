using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject hostMultiplayerPanel;
    [SerializeField] private GameObject joinMultiplayerPanel;
    
    [Header("Settings UI GameObjects")]
    [SerializeField] private GameObject volumeSliderObj;
    [SerializeField] private GameObject volumeTextObj;
    [SerializeField] private GameObject graphicsDropdownObj;

    // Private component references
    private Slider volumeSlider;
    private Text volumeText;
    private Dropdown graphicsDropdown;

    void Start()
    {
        // Get components from the GameObjects
        if (volumeSliderObj != null) volumeSlider = volumeSliderObj.GetComponent<Slider>();
        if (volumeTextObj != null) volumeText = volumeTextObj.GetComponent<Text>();
        if (graphicsDropdownObj != null) graphicsDropdown = graphicsDropdownObj.GetComponent<Dropdown>();
        
        // Setup events
        SetupEvents();
        
        // Show main menu at start
        ShowMainMenu();
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
        if (settingsPanel != null) settingsPanel.SetActive(true);
        HideTitle();
    }

    public void ShowHostMultiplayer()
    {
        HideAllPanels();
        if (hostMultiplayerPanel != null) hostMultiplayerPanel.SetActive(true);
        HideTitle();
    }

    public void ShowJoinMultiplayer()
    {
        HideAllPanels();
        if (joinMultiplayerPanel != null) joinMultiplayerPanel.SetActive(true);
        HideTitle();
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ShowTitle();
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