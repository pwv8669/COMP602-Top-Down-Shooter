using UnityEngine;
using Photon.Pun;
using Photon.Voice.Unity;
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
    private AudioSource audioSource;

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

        InitializeVoiceSystem();
    }

    void InitializeVoiceSystem()
    {
        SetupVoiceRecorder();
        SetupAudioOutput();

        if (enableVoiceChatOnStart && !isPushToTalkMode)
        {
            EnableMicrophone();
        }

        isVoiceSystemReady = true;

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice system initialized successfully!");
    }

    void SetupVoiceRecorder()
    {
        // Get or add Recorder component
        voiceRecorder = GetComponent<Recorder>();
        if (voiceRecorder == null)
        {
            voiceRecorder = gameObject.AddComponent<Recorder>();
        }

        // Configure recorder settings
        voiceRecorder.TransmitEnabled = false; // Start with mic disabled
        voiceRecorder.VoiceDetection = true;
        voiceRecorder.VoiceDetectionThreshold = 0.01f;

        // Set microphone volume (handle different Photon Voice versions)
        SetMicrophoneVolume(microphoneVolume);

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice recorder configured");
    }

    void SetupAudioOutput()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configure audio output
        audioSource.volume = speakerVolume;
        audioSource.spatialBlend = 0f; // 2D audio for voice chat

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Audio output configured");
    }

    void Update()
    {
        if (!isVoiceSystemReady) return;

        HandlePushToTalk();
        UpdatePlayerSpeakers();

        // TESTING CODE: This key input block is for testing and should be removed for launch.
        if (Input.GetKeyDown(KeyCode.M)) // Press 'M' for mute all players
        {
            MuteAllPlayers();
        }

        if (Input.GetKeyDown(KeyCode.U)) // Press 'U' for unmute all players
        {
            UnmuteAllPlayers();
        }
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
                Debug.Log("[VoiceChat] Microphone enabled");
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
            Debug.Log($"[VoiceChat] Player muted: {playerId}");
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
            Debug.Log($"[VoiceChat] Player unmuted: {playerId}");
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
            Debug.Log($"[VoiceChat] Microphone volume set to {microphoneVolume}");
    }

    public void SetSpeakerVolume(float volume)
    {
        speakerVolume = Mathf.Clamp(volume, 0f, 2f);
        if (audioSource != null)
        {
            audioSource.volume = speakerVolume;
        }

        if (showDebugLogs)
            Debug.Log($"[VoiceChat] Speaker volume set to {speakerVolume}");
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

    #region Event Handlers

    void OnPlayerEnteredRoom()
    {
        // Initialize mute state for all players
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!mutedPlayers.ContainsKey(player.UserId))
            {
                mutedPlayers[player.UserId] = false; // Default: not muted
            }
        }

        if (showDebugLogs)
            Debug.Log("[VoiceChat] New player entered - mute states updated");
    }

    void OnPlayerLeftRoom()
    {
        // Clean up data for disconnected players
        var currentPlayerIds = new List<string>();
        foreach (var player in PhotonNetwork.PlayerList)
        {
            currentPlayerIds.Add(player.UserId);
        }

        var playersToRemove = new List<string>();
        foreach (var kvp in mutedPlayers)
        {
            if (!currentPlayerIds.Contains(kvp.Key))
            {
                playersToRemove.Add(kvp.Key);
            }
        }

        foreach (string playerId in playersToRemove)
        {
            mutedPlayers.Remove(playerId);
            playerSpeakers.Remove(playerId);
        }

        if (showDebugLogs && playersToRemove.Count > 0)
            Debug.Log($"[VoiceChat] Cleaned up data for {playersToRemove.Count} disconnected players");
    }

    #endregion
}