using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [SerializeField] private AudioMixer audioMixer;
    
    private const string MASTER_VOLUME_KEY = "MasterVolume";
    private const string MIXER_MASTER_PARAM = "MasterVol";

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadVolumeSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetMasterVolume(float volumePercent)
    {
        // Convert linear 0-1 to logarithmic -80 to 0 dB
        float volumeDB = Mathf.Log10(volumePercent) * 20;
        if (volumePercent <= 0.001f) // Minimum threshold to avoid log(0)
            volumeDB = -80f;
            
        audioMixer.SetFloat(MIXER_MASTER_PARAM, volumeDB);
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, volumePercent);
        PlayerPrefs.Save();
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0.75f); // Default to 75%
    }

    private void LoadVolumeSettings()
    {
        float savedVolume = GetMasterVolume();
        SetMasterVolume(savedVolume);
    }
}