using UnityEngine;
using UnityEngine.UI;
using TMPro; 

public class VolumeSliderController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText; // For TextMeshPro

    private void Start()
    {
        if (volumeSlider != null)
        {
            // Load saved volume
            float savedVolume = AudioManager.Instance.GetMasterVolume();
            volumeSlider.value = savedVolume;
            
            // Update text immediately
            UpdateVolumeText(savedVolume);
            
            // Add listener for value changes
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
    }

    private void OnVolumeChanged(float volume)
    {
        AudioManager.Instance.SetMasterVolume(volume);
        UpdateVolumeText(volume);
    }

    private void UpdateVolumeText(float volume)
    {
        if (volumeText != null)
        {
            // Convert to percentage (0-100) and display
            int volumePercent = Mathf.RoundToInt(volume * 100);
            volumeText.text = $"{volumePercent}%";
        }
    }

    private void OnDestroy()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
    }
}