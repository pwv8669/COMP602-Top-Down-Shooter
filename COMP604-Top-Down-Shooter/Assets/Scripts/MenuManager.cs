using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private GameObject mainMenuPanel;
    private GameObject settingsPanel;
    private GameObject hostMultiplayerPanel;
    private GameObject joinMultiplayerPanel;
    
    private Slider volumeSlider;
    private Text volumeText;
    private Dropdown graphicsDropdown;

    void Start()
    {
        // Find all panels automatically
        FindPanels();
        
        // Find settings UI elements
        FindSettingsElements();
        
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
        HideTitle(); // Hide title on settings page
    }

    public void ShowHostMultiplayer()
    {
        HideAllPanels();
        if (hostMultiplayerPanel != null) hostMultiplayerPanel.SetActive(true);
        HideTitle(); // Hide title on host page
    }

    public void ShowJoinMultiplayer()
    {
        HideAllPanels();
        if (joinMultiplayerPanel != null) joinMultiplayerPanel.SetActive(true);
        HideTitle(); // Hide title on join page
    }

    public void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        ShowTitle(); // Show title only on main menu
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

    void ShowTitle()
    {
        GameObject title = GameObject.Find("TitleText");
        if (title != null) title.SetActive(true);
    }
}