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
        // Set the graphics quality
        QualitySettings.SetQualityLevel(qualityIndex);
        
        // Save the setting
        PlayerPrefs.SetInt(GRAPHICS_QUALITY_KEY, qualityIndex);
        PlayerPrefs.Save();
        
        Debug.Log($"Graphics quality set to: {QualitySettings.names[qualityIndex]}");
    }

    public int GetSavedGraphicsQuality()
    {
        return PlayerPrefs.GetInt(GRAPHICS_QUALITY_KEY, 1); // Default to Medium (index 1)
    }

    private void LoadGraphicsSettings()
    {
        int savedQuality = GetSavedGraphicsQuality();
        SetGraphicsQuality(savedQuality);
    }
}