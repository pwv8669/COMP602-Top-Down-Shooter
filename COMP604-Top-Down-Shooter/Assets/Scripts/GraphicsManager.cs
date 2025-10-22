using UnityEngine;
using UnityEngine.UI;

public class GraphicsManager : MonoBehaviour
{
    public static GraphicsManager Instance { get; private set; }
    
    private const string GRAPHICS_QUALITY_KEY = "GraphicsQuality";
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGraphicsSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGraphicsQuality(int qualityIndex)
    {
        // Validate the quality index before setting
        if (qualityIndex >= 0 && qualityIndex < QualitySettings.names.Length)
        {
            // Set the graphics quality
            QualitySettings.SetQualityLevel(qualityIndex);
            
            // Save the setting
            PlayerPrefs.SetInt(GRAPHICS_QUALITY_KEY, qualityIndex);
            PlayerPrefs.Save();
            
            Debug.Log($"Graphics quality set to: {QualitySettings.names[qualityIndex]} (Index: {qualityIndex})");
        }
        else
        {
            Debug.LogWarning($"Invalid quality index: {qualityIndex}. Available levels: {QualitySettings.names.Length}");
            // Set to default (Medium) if invalid
            int defaultIndex = GetDefaultQualityIndex();
            SetGraphicsQuality(defaultIndex);
        }
    }

    public int GetSavedGraphicsQuality()
    {
        int savedQuality = PlayerPrefs.GetInt(GRAPHICS_QUALITY_KEY, -1);
        
        // If no saved quality or saved quality is invalid, return default
        if (savedQuality < 0 || savedQuality >= QualitySettings.names.Length)
        {
            return GetDefaultQualityIndex();
        }
        
        return savedQuality;
    }

    private int GetDefaultQualityIndex()
    {
        // Default to Medium if available, otherwise use the highest available
        int mediumIndex = 1;
        if (mediumIndex < QualitySettings.names.Length)
        {
            return mediumIndex;
        }
        else
        {
            return QualitySettings.names.Length - 1; // Last available quality
        }
    }

    private void LoadGraphicsSettings()
    {
        int savedQuality = GetSavedGraphicsQuality();
        SetGraphicsQuality(savedQuality);
    }

    public int GetQualityLevelsCount()
    {
        return QualitySettings.names.Length;
    }
}