using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Voice.PUN;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;

public class VoiceChat : MonoBehaviour
{
    [Header("Voice Chat Settings")]
    public bool enableVoiceChatOnStart = true;
    public KeyCode pushToTalkKey = KeyCode.T;
    public bool isPushToTalkMode = false;

    [Header("Audio Settings")]
    [Range(0f, 2f)]
    public float microphoneVolume = 1f;
    [Range(0f, 2f)]
    public float speakerVolume = 1f;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Photon Voice Components
    private Recorder voiceRecorder;
    private Speaker voiceSpeaker;
    private AudioSource audioSource;
    private PhotonVoiceView photonVoiceView;

    // Reference to Multiplayer system
    private Multiplayer multiplayerManager;

    // Mute system
    private Dictionary<string, bool> mutedPlayers = new Dictionary<string, bool>();
    private Dictionary<string, Speaker> playerSpeakers = new Dictionary<string, Speaker>();

    // Voice state
    private bool isMicrophoneEnabled = false;
    private bool isVoiceSystemReady = false;

    void Start()
    {
        // Find multiplayer manager
        multiplayerManager = FindFirstObjectByType<Multiplayer>();
        if (multiplayerManager == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("[VoiceChat] Multiplayer manager not found! Voice chat will initialize when room is joined.");
        }

        // Wait for room connection before initializing voice system
        StartCoroutine(WaitForRoomAndInitialize());
    }

    IEnumerator WaitForRoomAndInitialize()
    {
        // Wait until connected and in a room
        while (multiplayerManager == null || !multiplayerManager.IsInRoom())
        {
            if (showDebugLogs && multiplayerManager != null)
                Debug.Log("[VoiceChat] Waiting for room connection...");
            yield return new WaitForSeconds(1f);
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Room connected! Initializing voice system...");

        // Additional wait time to ensure Photon Voice full initialization
        yield return new WaitForSeconds(2f);

        InitializeVoiceSystem();
    }

    void InitializeVoiceSystem()
    {
        SetupPhotonVoiceView();
        SetupVoiceRecorder();
        SetupVoiceSpeaker();
        SetupAudioOutput();
        ConnectPhotonVoiceComponents();

        isVoiceSystemReady = true;

        if (showDebugLogs)
        {
            Debug.Log("[VoiceChat] Voice system initialized successfully!");
            Debug.Log("[VoiceChat] Press 'V' to check voice status");
            ShowVoiceStatus();
        }
    }

    void SetupPhotonVoiceView()
    {
        // Get existing PhotonVoiceView component first
        photonVoiceView = GetComponent<PhotonVoiceView>();
        if (photonVoiceView == null)
        {
            photonVoiceView = gameObject.AddComponent<PhotonVoiceView>();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] PhotonVoiceView component created automatically");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Using existing PhotonVoiceView component from GameObject");
        }
    }

    void SetupVoiceRecorder()
    {
        // Get existing Recorder component first
        voiceRecorder = GetComponent<Recorder>();
        if (voiceRecorder == null)
        {
            voiceRecorder = gameObject.AddComponent<Recorder>();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Recorder component created automatically");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Using existing Recorder component from GameObject");
        }

        // Configure recorder settings
        voiceRecorder.VoiceDetection = true;
        voiceRecorder.VoiceDetectionThreshold = 0.01f;
        voiceRecorder.TransmitEnabled = false; // Start disabled, will be enabled based on settings

        // Set microphone device explicitly
        if (Microphone.devices.Length > 0)
        {
            try
            {
                var micDevice = new Photon.Voice.DeviceInfo(Microphone.devices[0]);
                voiceRecorder.MicrophoneDevice = micDevice;
                if (showDebugLogs)
                    Debug.Log("[VoiceChat] Selected microphone: " + Microphone.devices[0]);
            }
            catch
            {
                if (showDebugLogs)
                    Debug.Log("[VoiceChat] Using default microphone device");
            }
        }

        // Enable microphone based on initial settings
        if (enableVoiceChatOnStart && !isPushToTalkMode)
        {
            voiceRecorder.TransmitEnabled = true;
            isMicrophoneEnabled = true;
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone auto-enabled on start");
        }
        else
        {
            isMicrophoneEnabled = false;
        }

        // Set microphone volume
        SetMicrophoneVolume(microphoneVolume);

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice recorder configured successfully");
    }

    void SetupVoiceSpeaker()
    {
        // Get existing Speaker component first
        voiceSpeaker = GetComponent<Speaker>();
        if (voiceSpeaker == null)
        {
            voiceSpeaker = gameObject.AddComponent<Speaker>();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Speaker component created automatically");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Using existing Speaker component from GameObject");
        }

        // Configure speaker settings
        voiceSpeaker.enabled = true;

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice speaker configured successfully");
    }

    void SetupAudioOutput()
    {
        // Get existing AudioSource component first
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] AudioSource component created automatically");
        }
        else
        {
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Using existing AudioSource component from GameObject");
        }

        // Configure audio output settings
        audioSource.volume = speakerVolume;
        audioSource.spatialBlend = 0f; // 2D audio for voice chat
        audioSource.playOnAwake = false;

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Audio output configured successfully");
    }

    void ConnectPhotonVoiceComponents()
    {
        // Connect Recorder and Speaker to PhotonVoiceView
        if (photonVoiceView != null)
        {
            if (voiceRecorder != null && voiceSpeaker != null)
            {
                try
                {
                    // Try different PhotonVoiceView setup methods based on version
                    var voiceViewType = photonVoiceView.GetType();

                    // Method 1: Try SetupRecorderSpeaker method (newer versions)
                    var setupMethod = voiceViewType.GetMethod("SetupRecorderSpeaker");
                    if (setupMethod != null)
                    {
                        setupMethod.Invoke(photonVoiceView, new object[] { voiceRecorder, voiceSpeaker });
                        if (showDebugLogs)
                            Debug.Log("[VoiceChat] PhotonVoiceView connected via SetupRecorderSpeaker method");
                        return;
                    }

                    // Method 2: Try direct property assignment (older versions)
                    var recorderProperty = voiceViewType.GetProperty("RecorderInUse");
                    var speakerProperty = voiceViewType.GetProperty("SpeakerInUse");

                    if (recorderProperty != null && speakerProperty != null)
                    {
                        recorderProperty.SetValue(photonVoiceView, voiceRecorder);
                        speakerProperty.SetValue(photonVoiceView, voiceSpeaker);
                        if (showDebugLogs)
                            Debug.Log("[VoiceChat] PhotonVoiceView connected via property assignment");
                        return;
                    }

                    // Method 3: Manual Inspector assignment message
                    if (showDebugLogs)
                        Debug.LogWarning("[VoiceChat] Automatic PhotonVoiceView connection failed. Please manually assign Recorder and Speaker in PhotonVoiceView Inspector.");
                }
                catch (System.Exception e)
                {
                    if (showDebugLogs)
                        Debug.LogWarning("[VoiceChat] Could not connect PhotonVoiceView automatically: " + e.Message + ". Please assign manually in Inspector.");
                }
            }
        }
    }

    void Update()
    {
        if (!isVoiceSystemReady) return;

        HandlePushToTalk();
        UpdatePlayerSpeakers();

        // Voice status check key (V key)
        if (Input.GetKeyDown(KeyCode.V))
        {
            ShowVoiceStatus();
        }

        // Debug keys for testing - remove in production build
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Input.GetKeyDown(KeyCode.M)) // Press 'M' for mute all players
        {
            MuteAllPlayers();
        }

        if (Input.GetKeyDown(KeyCode.U)) // Press 'U' for unmute all players
        {
            UnmuteAllPlayers();
        }
#endif
    }

    void ShowVoiceStatus()
    {
        Debug.Log("=== VOICE CHAT STATUS ===");
        Debug.Log("Voice System Ready: " + isVoiceSystemReady);
        Debug.Log("Microphone Enabled: " + isMicrophoneEnabled);
        Debug.Log("In Room: " + PhotonNetwork.InRoom);
        Debug.Log("Connected Players: " + GetConnectedPlayersCount());
        Debug.Log("Recorder Available: " + (voiceRecorder != null));
        Debug.Log("Speaker Available: " + (voiceSpeaker != null));
        Debug.Log("PhotonVoiceView Available: " + (photonVoiceView != null));

        // Check microphone devices
        Debug.Log("Available Microphones: " + Microphone.devices.Length);
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log("  - Microphone " + i + ": " + Microphone.devices[i]);
        }

        if (voiceRecorder != null)
        {
            Debug.Log("Recorder Transmitting: " + voiceRecorder.TransmitEnabled);
            Debug.Log("Microphone Level: " + GetMicrophoneLevel().ToString("F3"));

            // Safe microphone device name check
            string micDevice = "Default";
            try
            {
                if (voiceRecorder.MicrophoneDevice != null)
                {
                    micDevice = voiceRecorder.MicrophoneDevice.ToString();
                }
            }
            catch
            {
                micDevice = "Unknown";
            }
            Debug.Log("Microphone Device: " + micDevice);
        }

        Debug.Log("Muted Players Count: " + GetMutedPlayerIds().Count);

        // Player list
        var playerNames = GetConnectedPlayerNames();
        Debug.Log("Players in room: " + string.Join(", ", playerNames.ToArray()));

        // Check for VoiceLogger component
        var voiceLogger = GetComponent<VoiceLogger>();
        Debug.Log("VoiceLogger Present: " + (voiceLogger != null));

        Debug.Log("========================");
    }

    void HandlePushToTalk()
    {
        if (!isPushToTalkMode) return;

        if (Input.GetKeyDown(pushToTalkKey))
        {
            EnableMicrophone();
        }
        else if (Input.GetKeyUp(pushToTalkKey))
        {
            DisableMicrophone();
        }
    }

    void UpdatePlayerSpeakers()
    {
        // Update speaker components for all players
        Speaker[] speakers = FindObjectsByType<Speaker>(FindObjectsSortMode.None);
        foreach (Speaker speaker in speakers)
        {
            // Try to get the PhotonView to identify the player
            PhotonView photonView = speaker.GetComponent<PhotonView>();
            if (photonView != null && photonView.Owner != null)
            {
                string playerId = photonView.Owner.UserId;
                if (!string.IsNullOrEmpty(playerId))
                {
                    playerSpeakers[playerId] = speaker;

                    // Apply mute status
                    if (mutedPlayers.ContainsKey(playerId) && mutedPlayers[playerId])
                    {
                        speaker.enabled = false;
                    }
                    else
                    {
                        speaker.enabled = true;
                    }
                }
            }
        }
    }

    #region Microphone Control

    public void EnableMicrophone()
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = true;
            isMicrophoneEnabled = true;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone enabled - You can now speak!");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogError("[VoiceChat] Cannot enable microphone - Recorder not initialized!");
        }
    }

    public void DisableMicrophone()
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = false;
            isMicrophoneEnabled = false;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone disabled");
        }
    }

    public void ToggleMicrophone()
    {
        if (isMicrophoneEnabled)
            DisableMicrophone();
        else
            EnableMicrophone();
    }

    public bool IsMicrophoneEnabled()
    {
        return isMicrophoneEnabled;
    }

    #endregion

    #region Mute System

    public void MutePlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        mutedPlayers[playerId] = true;

        // Apply mute to existing speaker
        if (playerSpeakers.ContainsKey(playerId))
        {
            playerSpeakers[playerId].enabled = false;
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Player muted: " + playerId);
    }

    public void UnmutePlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        mutedPlayers[playerId] = false;

        // Apply unmute to existing speaker
        if (playerSpeakers.ContainsKey(playerId))
        {
            playerSpeakers[playerId].enabled = true;
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Player unmuted: " + playerId);
    }

    public void MuteAllPlayers()
    {
        // Get all players from PhotonNetwork directly
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer) // Don't mute yourself
            {
                MutePlayer(player.UserId);
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] All players muted");
    }

    public void UnmuteAllPlayers()
    {
        // Get all players from PhotonNetwork directly
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer) // Don't unmute yourself
            {
                UnmutePlayer(player.UserId);
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] All players unmuted");
    }

    public bool IsPlayerMuted(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return false;
        return mutedPlayers.ContainsKey(playerId) && mutedPlayers[playerId];
    }

    public List<string> GetMutedPlayerIds()
    {
        List<string> mutedList = new List<string>();
        foreach (var kvp in mutedPlayers)
        {
            if (kvp.Value) // if muted
                mutedList.Add(kvp.Key);
        }
        return mutedList;
    }

    #endregion

    #region Volume Control

    public void SetMicrophoneVolume(float volume)
    {
        microphoneVolume = Mathf.Clamp(volume, 0f, 2f);
        if (voiceRecorder != null)
        {
            // Handle different Photon Voice versions
            try
            {
                var recorderType = voiceRecorder.GetType();
                var amplificationProperty = recorderType.GetProperty("AmplificationFactor");
                if (amplificationProperty != null)
                {
                    amplificationProperty.SetValue(voiceRecorder, microphoneVolume);
                }
            }
            catch
            {
                if (showDebugLogs)
                    Debug.LogWarning("[VoiceChat] Could not set microphone volume - property not available in this Photon Voice version");
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Microphone volume set to " + microphoneVolume);
    }

    public void SetSpeakerVolume(float volume)
    {
        speakerVolume = Mathf.Clamp(volume, 0f, 2f);
        if (audioSource != null)
        {
            audioSource.volume = speakerVolume;
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Speaker volume set to " + speakerVolume);
    }

    public float GetMicrophoneVolume()
    {
        return microphoneVolume;
    }

    public float GetSpeakerVolume()
    {
        return speakerVolume;
    }

    #endregion

    #region Utility Methods

    public bool IsVoiceSystemReady()
    {
        return isVoiceSystemReady;
    }

    public float GetMicrophoneLevel()
    {
        if (voiceRecorder != null)
        {
            try
            {
                var levelMeter = voiceRecorder.LevelMeter;
                if (levelMeter != null)
                {
                    // Handle different Photon Voice versions
                    var levelMeterType = levelMeter.GetType();
                    var currentAvgAmpProperty = levelMeterType.GetProperty("CurrentAvgAmp");
                    if (currentAvgAmpProperty != null)
                    {
                        return (float)currentAvgAmpProperty.GetValue(levelMeter);
                    }
                }
            }
            catch
            {
                // Fallback for different Photon Voice versions
            }
        }
        return 0f;
    }

    public int GetConnectedPlayersCount()
    {
        if (PhotonNetwork.CurrentRoom != null)
            return PhotonNetwork.CurrentRoom.PlayerCount;
        return 0;
    }

    public List<string> GetConnectedPlayerNames()
    {
        List<string> playerNames = new List<string>();
        if (PhotonNetwork.CurrentRoom != null)
        {
            foreach (var player in PhotonNetwork.PlayerList)
            {
                playerNames.Add(player.NickName);
            }
        }
        return playerNames;
    }

    #endregion
}