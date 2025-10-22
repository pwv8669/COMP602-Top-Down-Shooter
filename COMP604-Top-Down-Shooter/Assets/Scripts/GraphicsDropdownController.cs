using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class GraphicsDropdownController : MonoBehaviour
{
    private TMP_Dropdown graphicsDropdown;

    private void Awake()
    {
        // Automatically get the TMP_Dropdown component on the same GameObject
        graphicsDropdown = GetComponent<TMP_Dropdown>();
        
        if (graphicsDropdown == null)
        {
            Debug.LogError("GraphicsDropdownController: No TMP_Dropdown component found on the same GameObject!");
            return;
        }
    }

    private void Start()
    {
        SetupDropdown();
    }

    private void SetupDropdown()
    {
        // Update dropdown options first
        UpdateDropdownOptions();
        
        // Then set the current value
        int savedQuality = GraphicsManager.Instance.GetSavedGraphicsQuality();
        
        // Make sure the saved quality is valid for the dropdown
        if (savedQuality >= 0 && savedQuality < graphicsDropdown.options.Count)
        {
            graphicsDropdown.value = savedQuality;
        }
        else
        {
            // Use a safe default
            graphicsDropdown.value = Mathf.Clamp(1, 0, graphicsDropdown.options.Count - 1);
        }
        
        // Add listener for value changes
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);
        
        Debug.Log("Graphics dropdown setup complete!");
    }

    private void OnGraphicsQualityChanged(int qualityIndex)
    {
        Debug.Log($"Graphics quality changed to index: {qualityIndex}");
        GraphicsManager.Instance.SetGraphicsQuality(qualityIndex);
    }

    private void UpdateDropdownOptions()
    {
        // Clear existing options
        graphicsDropdown.ClearOptions();
        
        // Get available quality levels from Unity's Quality Settings
        string[] qualityNames = QualitySettings.names;
        
        // Create new options
        var dropdownOptions = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
        foreach (string qualityName in qualityNames)
        {
            dropdownOptions.Add(new TMP_Dropdown.OptionData(qualityName));
        }
        
        graphicsDropdown.AddOptions(dropdownOptions);
        
        Debug.Log($"Dropdown updated with {qualityNames.Length} quality levels: {string.Join(", ", qualityNames)}");
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (graphicsDropdown != null)
            graphicsDropdown.onValueChanged.RemoveListener(OnGraphicsQualityChanged);
    }
}