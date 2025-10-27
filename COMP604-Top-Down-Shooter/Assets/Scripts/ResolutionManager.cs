using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResolutionManager : MonoBehaviour
{
    [System.Serializable]
    public class ResolutionOption
    {
        public string label;
        public int width;
        public int height;
    }

    [Header("UI References")]
    public TMP_Dropdown resolutionDropdown;

    [Header("Resolution Mapping")]
    public ResolutionOption[] resolutionMap = new ResolutionOption[]
    {
        new ResolutionOption { label = "1920x1080 (Full HD)", width = 1920, height = 1080 },
        new ResolutionOption { label = "1366x768", width = 1366, height = 768 },
        new ResolutionOption { label = "1280x720 (HD)", width = 1280, height = 720 },
        new ResolutionOption { label = "1024x768", width = 1024, height = 768 }
    };

    private void Start()
    {
        Debug.Log("ResolutionManager Started!");

        SetupResolutionMapping();
        
        LoadSavedResolution();

        DebugCurrentResolution();

        VerifyResolutionSettings();
    }

    void SetupResolutionMapping()
    {
        Debug.Log("Setting up resolution mapping...");
        
        // Remove any existing listeners to avoid duplicates
        resolutionDropdown.onValueChanged.RemoveAllListeners();
        
        // Add our listener
        resolutionDropdown.onValueChanged.AddListener(TestDropdownChange);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        
        Debug.Log($"Dropdown has {resolutionDropdown.options.Count} options");
    }

 public void SetResolution(int dropdownIndex)
    {
        string selectedText = resolutionDropdown.options[dropdownIndex].text;
        ResolutionOption selectedRes = FindMatchingResolution(selectedText);
        
        if (selectedRes != null)
        {
            // Always save the preference
            PlayerPrefs.SetString("SavedResolution", selectedText);
            PlayerPrefs.Save();
            
            // Apply the resolution
            Screen.SetResolution(selectedRes.width, selectedRes.height, Screen.fullScreen);
            
            Debug.Log($"Resolution set to: {selectedRes.width} x {selectedRes.height}");
            Debug.Log($"Screen reports: {Screen.width} x {Screen.height}");
            
            // Refresh UI
            StartCoroutine(RefreshUIAfterResolutionChange());
            
            // Verify settings were saved
            VerifyResolutionSettings();
        }
    }

    ResolutionOption FindMatchingResolution(string dropdownText)
    {
        foreach (var res in resolutionMap)
        {
            if (dropdownText.Contains(res.width.ToString()) && dropdownText.Contains(res.height.ToString()))
            {
                return res;
            }
        }
        
        // Fallback: try to parse the text directly
        string[] dimensions = dropdownText.Split('x');
        if (dimensions.Length >= 2)
        {
            // Extract numbers from the text (handles "1920x1080 (Full HD)")
            string widthStr = System.Text.RegularExpressions.Regex.Match(dimensions[0], @"\d+").Value;
            string heightStr = System.Text.RegularExpressions.Regex.Match(dimensions[1], @"\d+").Value;
            
            if (int.TryParse(widthStr, out int width) && int.TryParse(heightStr, out int height))
            {
                return new ResolutionOption { label = dropdownText, width = width, height = height };
            }
        }
        
        Debug.LogWarning($"Could not parse resolution from: {dropdownText}");
        return null;
    }

    IEnumerator RefreshUIAfterResolutionChange()
    {
        yield return new WaitForEndOfFrame();
        
        // Force all canvases to update
        Canvas.ForceUpdateCanvases();
        
        // Refresh all UI elements
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            canvas.enabled = false;
            canvas.enabled = true;
        }
        
        // Force layout rebuild
        foreach (var layoutGroup in FindObjectsOfType<LayoutGroup>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        
        DebugCurrentResolution(); // Debug after resolution change
    }

    void LoadSavedResolution()
    {
        string savedResolution = PlayerPrefs.GetString("SavedResolution", "");
        
        if (!string.IsNullOrEmpty(savedResolution))
        {
            // Find the dropdown option that matches the saved resolution
            for (int i = 0; i < resolutionDropdown.options.Count; i++)
            {
                if (resolutionDropdown.options[i].text.Contains(savedResolution) || 
                    resolutionDropdown.options[i].text == savedResolution)
                {
                    resolutionDropdown.value = i;
                    resolutionDropdown.RefreshShownValue();
                    
                    // Apply the resolution
                    ResolutionOption savedRes = FindMatchingResolution(savedResolution);
                    if (savedRes != null)
                    {
                        Screen.SetResolution(savedRes.width, savedRes.height, Screen.fullScreen);
                    }
                    break;
                }
            }
        }
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("IsFullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    void DebugCurrentResolution()
    {
        Debug.Log($"Current resolution: {Screen.width} x {Screen.height}");
        Debug.Log($"Safe area: {Screen.safeArea}");
        Debug.Log($"Fullscreen: {Screen.fullScreen}");
    }

    public void TestDropdownChange(int index)
    {
        Debug.Log("TEST: Dropdown value changed!");
    }

    public void VerifyResolutionSettings()
    {
        Debug.Log($"Current resolution: {Screen.width} x {Screen.height}");
        Debug.Log($"Fullscreen: {Screen.fullScreen}");
        Debug.Log($"Dropdown value: {resolutionDropdown.value}");
        Debug.Log($"Dropdown option: {resolutionDropdown.options[resolutionDropdown.value].text}");
    }

}