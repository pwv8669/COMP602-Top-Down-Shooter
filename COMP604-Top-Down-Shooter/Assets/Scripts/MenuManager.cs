using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject hostMultiplayerPanel;
    public GameObject joinMultiplayerPanel;

    [Header("Settings References")]
    public Slider volumeSlider;
    public Text volumeText;
    public Dropdown graphicsDropdown;

    void Start()
    {
        // Show main menu, hide others at start
        ShowMainMenu();
        
        // Setup volume slider event
        if (volumeSlider != null && volumeText != null)
        {
            volumeSlider.onValueChanged.AddListener(UpdateVolumeText);
            UpdateVolumeText(volumeSlider.value);
        }
    }

    // ===== MAIN MENU FUNCTIONS =====
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void ShowSettings()
    {
        HideAllPanels();
        settingsPanel.SetActive(true);
    }

    public void ShowHostMultiplayer()
    {
        HideAllPanels();
        hostMultiplayerPanel.SetActive(true);
    }

    public void ShowJoinMultiplayer()
    {
        HideAllPanels();
        joinMultiplayerPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ===== SETTINGS FUNCTIONS =====
    void UpdateVolumeText(float value)
    {
        volumeText.text = $"Volume: {Mathf.RoundToInt(value * 100)}%";
    }

    public void OnGraphicsChanged(int index)
    {
        Debug.Log($"Graphics quality set to: {graphicsDropdown.options[index].text}");
    }

    // ===== BACK BUTTON FUNCTIONS =====
    public void ShowMainMenu()
    {
        HideAllPanels();
        mainMenuPanel.SetActive(true);
    }

    void HideAllPanels()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        hostMultiplayerPanel.SetActive(false);
        joinMultiplayerPanel.SetActive(false);
    }
}