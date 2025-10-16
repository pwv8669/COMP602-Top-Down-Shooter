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

    [Header("Hotkeys")]
    public KeyCode toggleMicKey = KeyCode.V;  // V key to toggle own mic
    public KeyCode muteAllKey = KeyCode.M;    // M key to mute all players

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Photon Voice Components
    private Recorder voiceRecorder;
    private Speaker voiceSpeaker;
    private AudioSource audioSource;

    // Mute system
    private Dictionary<string, Speaker> playerSpeakers = new Dictionary<string, Speaker>();
    private bool allPlayersMuted = false;

    // Voice state
    private bool isMicrophoneEnabled = false;
    private bool isVoiceSystemReady = false;

    void Start()
    {
        // Wait for room connection before initializing voice system
        StartCoroutine(WaitForRoomAndInitialize());
    }

    IEnumerator WaitForRoomAndInitialize()
    {
        // Wait until connected and in a room
        while (!PhotonNetwork.InRoom)
        {
            if (showDebugLogs)
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
        SetupVoiceRecorder();
        SetupVoiceSpeaker();
        SetupAudioOutput();

        isVoiceSystemReady = true;

        if (showDebugLogs)
        {
            Debug.Log("[VoiceChat] Voice system initialized successfully!");
            Debug.Log($"[VoiceChat] Press '{toggleMicKey}' to toggle your microphone");
            Debug.Log($"[VoiceChat] Press '{muteAllKey}' to mute/unmute all players");
        }
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
        voiceRecorder.VoiceDetection = true;
        voiceRecorder.VoiceDetectionThreshold = 0.01f;

        // Setup microphone device explicitly
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

        // Auto-enable microphone on start
        if (enableVoiceChatOnStart)
        {
            voiceRecorder.TransmitEnabled = true;
            isMicrophoneEnabled = true;
            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone auto-enabled - You can speak!");
        }
        else
        {
            voiceRecorder.TransmitEnabled = false;
            isMicrophoneEnabled = false;
        }

        // Set microphone volume
        SetMicrophoneVolume(microphoneVolume);

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice recorder configured");
    }

    void SetupVoiceSpeaker()
    {
        // Get or add Speaker component
        voiceSpeaker = GetComponent<Speaker>();
        if (voiceSpeaker == null)
        {
            voiceSpeaker = gameObject.AddComponent<Speaker>();
        }

        // Configure speaker settings
        voiceSpeaker.enabled = true;

        if (showDebugLogs)
            Debug.Log("[VoiceChat] Voice speaker configured");
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

        UpdatePlayerSpeakers();

        // V key: Toggle own microphone
        if (Input.GetKeyDown(toggleMicKey))
        {
            ToggleMicrophone();
        }

        // M key: Mute/unmute all players
        if (Input.GetKeyDown(muteAllKey))
        {
            ToggleMuteAll();
        }
    }

    void UpdatePlayerSpeakers()
    {
        // Update speaker components for all players
        Speaker[] speakers = FindObjectsByType<Speaker>(FindObjectsSortMode.None);
        foreach (Speaker speaker in speakers)
        {
            // Skip own speaker
            if (speaker == voiceSpeaker) continue;

            // Try to get the PhotonView to identify the player
            PhotonView photonView = speaker.GetComponent<PhotonView>();
            if (photonView != null && photonView.Owner != null)
            {
                string playerId = photonView.Owner.UserId;
                if (!string.IsNullOrEmpty(playerId))
                {
                    playerSpeakers[playerId] = speaker;

                    // Apply mute all status
                    speaker.enabled = !allPlayersMuted;
                }
            }
        }
    }

    #region Simple Controls

    public void ToggleMicrophone()
    {
        if (voiceRecorder == null)
        {
            if (showDebugLogs)
                Debug.LogError("[VoiceChat] Cannot toggle microphone - Recorder not initialized!");
            return;
        }

        if (isMicrophoneEnabled)
        {
            // Mute own microphone
            voiceRecorder.TransmitEnabled = false;
            isMicrophoneEnabled = false;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone MUTED - You cannot speak");
        }
        else
        {
            // Unmute own microphone
            voiceRecorder.TransmitEnabled = true;
            isMicrophoneEnabled = true;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Microphone UNMUTED - You can speak!");
        }
    }

    public void ToggleMuteAll()
    {
        allPlayersMuted = !allPlayersMuted;

        // Apply to all current players
        foreach (var kvp in playerSpeakers)
        {
            if (kvp.Value != null)
            {
                kvp.Value.enabled = !allPlayersMuted;
            }
        }

        if (showDebugLogs)
        {
            if (allPlayersMuted)
                Debug.Log("[VoiceChat] ALL PLAYERS MUTED - You cannot hear anyone");
            else
                Debug.Log("[VoiceChat] ALL PLAYERS UNMUTED - You can hear everyone");
        }
    }

    #endregion

    #region Individual Player Mute (for UI use)

    public void MutePlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        if (playerSpeakers.ContainsKey(playerId))
        {
            playerSpeakers[playerId].enabled = false;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Player muted: " + playerId);
        }
    }

    public void UnmutePlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return;

        if (playerSpeakers.ContainsKey(playerId))
        {
            playerSpeakers[playerId].enabled = true;

            if (showDebugLogs)
                Debug.Log("[VoiceChat] Player unmuted: " + playerId);
        }
    }

    public bool IsPlayerMuted(string playerId)
    {
        if (string.IsNullOrEmpty(playerId)) return false;

        if (playerSpeakers.ContainsKey(playerId))
        {
            return !playerSpeakers[playerId].enabled;
        }

        return false;
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

    #region Status Methods

    public bool IsMicrophoneEnabled()
    {
        return isMicrophoneEnabled;
    }

    public bool AreAllPlayersMuted()
    {
        return allPlayersMuted;
    }

    public bool IsVoiceSystemReady()
    {
        return isVoiceSystemReady;
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