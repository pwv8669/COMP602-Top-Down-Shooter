using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;

public class VoiceChat : MonoBehaviour
{
    [Header("Voice Chat Settings")]
    public bool enableVoiceChatOnStart = true;

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
    private PhotonView photonView;

    // Reference to Multiplayer system
    private Multiplayer multiplayerManager;

    // Mute system
    private Dictionary<string, bool> mutedPlayers = new Dictionary<string, bool>();
    private Dictionary<string, Speaker> playerSpeakers = new Dictionary<string, Speaker>();

    // Voice state
    private bool isMicrophoneEnabled = false;
    private bool isVoiceSystemReady = false;
    private bool isOtherPlayerMuted = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        if (showDebugLogs)
            Debug.Log($"[VoiceChat] Starting initialization for {gameObject.name}");

        // Find multiplayer manager for room status checks
        multiplayerManager = FindFirstObjectByType<Multiplayer>();

        // Start initialization coroutine
        StartCoroutine(WaitForRoomAndInitialize());
    }

    IEnumerator WaitForRoomAndInitialize()
    {
        // Wait until PhotonView.IsMine is properly set
        // In Unity 6 with Photon PUN, this might take a few frames
        int maxAttempts = 20;
        int attempts = 0;

        while (photonView != null && !photonView.IsMine && attempts < maxAttempts)
        {
            yield return new WaitForSeconds(0.1f);
            attempts++;
        }

        // Final check: is this the local player?
        if (photonView != null && !photonView.IsMine)
        {
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Remote player - Voice chat disabled");
            enabled = false;
            yield break;
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Local player detected! Starting voice initialization...");

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
        yield return new WaitForSeconds(1.5f);

        InitializeVoiceSystem();
    }

    void InitializeVoiceSystem()
    {
        SetupVoiceRecorder();
        SetupVoiceSpeaker();
        SetupAudioOutput();

        isVoiceSystemReady = true;

        if (showDebugLogs)
        {
            Debug.Log("[VoiceChat] Voice system initialized successfully!");
            Debug.Log("[VoiceChat] Controls: T = Toggle my mic, M = Toggle other player mute, V = Status");
            ShowVoiceStatus();
        }
    }

    void SetupVoiceRecorder()
    {
        // Get Recorder component from prefab (must be added manually)
        voiceRecorder = GetComponent<Recorder>();
        if (voiceRecorder == null)
        {
            if (showDebugLogs)
                Debug.LogError("[VoiceChat] Recorder component not found! Add it to the Character prefab.");
            return;
        }

        // Enable recording and transmission
        voiceRecorder.RecordingEnabled = true;
        voiceRecorder.TransmitEnabled = enableVoiceChatOnStart;

        // Set microphone device if available
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

        isMicrophoneEnabled = enableVoiceChatOnStart;

        if (enableVoiceChatOnStart && showDebugLogs)
            Debug.Log("[VoiceChat] Microphone auto-enabled");

        SetMicrophoneVolume(microphoneVolume);

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice recorder configured");
    }

    void SetupVoiceSpeaker()
    {
        // Get Speaker component from prefab
        voiceSpeaker = GetComponent<Speaker>();
        if (voiceSpeaker == null)
        {
            if (showDebugLogs)
                Debug.LogError("[VoiceChat] Speaker component not found! Add it to the Character prefab.");
            return;
        }

        // Speaker will automatically find AudioSource on same GameObject
        voiceSpeaker.enabled = true;

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice speaker configured");
    }

    void SetupAudioOutput()
    {
        // Get AudioSource component (Speaker will find it automatically)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            if (showDebugLogs)
                Debug.LogError("[VoiceChat] AudioSource component not found! Add it to the Character prefab.");
            return;
        }

        // Configure AudioSource for voice output
        audioSource.volume = speakerVolume;
        audioSource.spatialBlend = 0f; // 2D audio for voice chat

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Audio output configured");
    }

    void Update()
    {
        // Only process input for local player
        if (photonView != null && !photonView.IsMine)
            return;

        if (!isVoiceSystemReady) return;

        UpdatePlayerSpeakers();

        // Keyboard controls for voice chat
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleMicrophone();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleOtherPlayerMute();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            ShowVoiceStatus();
        }
    }

    void ShowVoiceStatus()
    {
        Debug.Log("=== VOICE CHAT STATUS ===");
        Debug.Log("Voice System Ready: " + isVoiceSystemReady);
        Debug.Log("My Microphone Enabled: " + isMicrophoneEnabled);
        Debug.Log("Other Player Muted: " + isOtherPlayerMuted);
        Debug.Log("In Room: " + PhotonNetwork.InRoom);
        Debug.Log("Connected Players: " + GetConnectedPlayersCount());

        Debug.Log("Available Microphones: " + Microphone.devices.Length);
        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            Debug.Log("  - Microphone " + i + ": " + Microphone.devices[i]);
        }

        if (voiceRecorder != null)
        {
            Debug.Log("Recorder Recording: " + voiceRecorder.RecordingEnabled);
            Debug.Log("Recorder Transmitting: " + voiceRecorder.TransmitEnabled);
            Debug.Log("Microphone Level: " + GetMicrophoneLevel().ToString("F3"));
        }

        var playerNames = GetConnectedPlayerNames();
        Debug.Log("Players in room: " + string.Join(", ", playerNames.ToArray()));
        Debug.Log("========================");
    }

    void UpdatePlayerSpeakers()
    {
        // Find all Speaker components in the scene
        Speaker[] speakers = FindObjectsByType<Speaker>(FindObjectsSortMode.None);

        foreach (Speaker speaker in speakers)
        {
            PhotonView speakerPhotonView = speaker.GetComponent<PhotonView>();

            // Skip own speaker
            if (speakerPhotonView != null && speakerPhotonView.IsMine)
                continue;

            // Apply mute settings to remote players
            if (speakerPhotonView != null && speakerPhotonView.Owner != null)
            {
                string playerId = speakerPhotonView.Owner.UserId;
                if (!string.IsNullOrEmpty(playerId))
                {
                    playerSpeakers[playerId] = speaker;

                    // Enable/disable speaker based on mute status
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
                Debug.Log("[VoiceChat] My microphone enabled - I can speak!");
        }
    }

    public void DisableMicrophone()
    {
        if (voiceRecorder != null)
        {
            voiceRecorder.TransmitEnabled = false;
            isMicrophoneEnabled = false;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] My microphone disabled - I cannot speak");
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

        if (playerSpeakers.ContainsKey(playerId))
        {
            playerSpeakers[playerId].enabled = true;
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Player unmuted: " + playerId);
    }

    public void MuteAllPlayers()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                MutePlayer(player.UserId);
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Other player muted");
    }

    public void UnmuteAllPlayers()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                UnmutePlayer(player.UserId);
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Other player unmuted");
    }

    public void ToggleOtherPlayerMute()
    {
        isOtherPlayerMuted = !isOtherPlayerMuted;

        if (isOtherPlayerMuted)
        {
            MuteAllPlayers();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Other player muted (Press M to unmute)");
        }
        else
        {
            UnmuteAllPlayers();
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Other player unmuted (Press M to mute)");
        }
    }

    public bool IsOtherPlayerMuted()
    {
        return isOtherPlayerMuted;
    }

    #endregion

    #region Volume Control

    public void SetMicrophoneVolume(float volume)
    {
        microphoneVolume = Mathf.Clamp(volume, 0f, 2f);
        if (voiceRecorder != null)
        {
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
                    Debug.LogWarning("[VoiceChat] Could not set microphone volume");
            }
        }
    }

    public void SetSpeakerVolume(float volume)
    {
        speakerVolume = Mathf.Clamp(volume, 0f, 2f);
        if (audioSource != null)
        {
            audioSource.volume = speakerVolume;
        }
    }

    #endregion

    #region Utility Methods

    public float GetMicrophoneLevel()
    {
        if (voiceRecorder != null)
        {
            try
            {
                var levelMeter = voiceRecorder.LevelMeter;
                if (levelMeter != null)
                {
                    var levelMeterType = levelMeter.GetType();
                    var currentAvgAmpProperty = levelMeterType.GetProperty("CurrentAvgAmp");
                    if (currentAvgAmpProperty != null)
                    {
                        return (float)currentAvgAmpProperty.GetValue(levelMeter);
                    }
                }
            }
            catch { }
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