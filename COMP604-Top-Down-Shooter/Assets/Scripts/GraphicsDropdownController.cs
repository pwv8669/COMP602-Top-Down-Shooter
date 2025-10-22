using UnityEngine;
using UnityEngine.UI;

public class GraphicsDropdownController : MonoBehaviour
{
    [SerializeField] private Dropdown graphicsDropdown;

    private void Start()
    {
        // If not assigned in inspector, try to find it
        if (graphicsDropdown == null)
        {
            graphicsDropdown = GetComponent<Dropdown>();
            if (graphicsDropdown == null)
            {
                Debug.LogError("GraphicsDropdownController: No Dropdown component found!");
                return;
            }
        }

        // Set up the dropdown
        // Load saved graphics quality
        int savedQuality = GraphicsManager.Instance.GetSavedGraphicsQuality();
        graphicsDropdown.value = savedQuality;
        
        // Add listener for value changes
        graphicsDropdown.onValueChanged.AddListener(OnGraphicsQualityChanged);
        
        // Update dropdown options based on available quality levels
        UpdateDropdownOptions();
    }

    private void OnGraphicsQualityChanged(int qualityIndex)
    {
        GraphicsManager.Instance.SetGraphicsQuality(qualityIndex);
    }

    private void UpdateDropdownOptions()
    {
        // Clear existing options
        graphicsDropdown.ClearOptions();
        
        // Get available quality levels from Unity's Quality Settings
        string[] qualityNames = QualitySettings.names;
        
        // Create new options
        var dropdownOptions = new System.Collections.Generic.List<Dropdown.OptionData>();
        foreach (string qualityName in qualityNames)
        {
            dropdownOptions.Add(new Dropdown.OptionData(qualityName));
        }
        
        graphicsDropdown.AddOptions(dropdownOptions);
        
        // Set current value
        graphicsDropdown.value = GraphicsManager.Instance.GetSavedGraphicsQuality();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (graphicsDropdown != null)
            graphicsDropdown.onValueChanged.RemoveListener(OnGraphicsQualityChanged);
    }
}