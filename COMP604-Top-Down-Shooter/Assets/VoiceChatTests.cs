using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class VoiceChatTests
{
    private VoiceChat voiceChat;
    private GameObject testObject;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("TestVoiceChat");
        voiceChat = testObject.AddComponent<VoiceChat>();
    }

    [TearDown]
    public void TearDown()
    {
        if (testObject != null)
        {
            Object.DestroyImmediate(testObject);
        }
    }

    #region Settings Tests (Safe - No Dependencies)

    [Test]
    public void EnableVoiceChatOnStart_ShouldHaveDefaultValue()
    {
        Assert.IsTrue(voiceChat.enableVoiceChatOnStart, "EnableVoiceChatOnStart should be true by default");
    }

    [Test]
    public void PushToTalkKey_ShouldHaveCorrectDefault()
    {
        Assert.AreEqual(KeyCode.T, voiceChat.pushToTalkKey, "Push-to-talk key should be T by default");
    }

    [Test]
    public void IsPushToTalkMode_ShouldHaveDefaultValue()
    {
        Assert.IsFalse(voiceChat.isPushToTalkMode, "IsPushToTalkMode should be false by default");
    }

    [Test]
    public void MicrophoneVolume_ShouldHaveValidDefault()
    {
        Assert.AreEqual(1f, voiceChat.microphoneVolume, "Default microphone volume should be 1.0");
        Assert.GreaterOrEqual(voiceChat.microphoneVolume, 0f, "Microphone volume should not be negative");
        Assert.LessOrEqual(voiceChat.microphoneVolume, 2f, "Microphone volume should not exceed 2.0");
    }

    [Test]
    public void SpeakerVolume_ShouldHaveValidDefault()
    {
        Assert.AreEqual(1f, voiceChat.speakerVolume, "Default speaker volume should be 1.0");
        Assert.GreaterOrEqual(voiceChat.speakerVolume, 0f, "Speaker volume should not be negative");
        Assert.LessOrEqual(voiceChat.speakerVolume, 2f, "Speaker volume should not exceed 2.0");
    }

    [Test]
    public void ShowDebugLogs_ShouldHaveDefaultValue()
    {
        Assert.IsTrue(voiceChat.showDebugLogs, "ShowDebugLogs should be true by default");
    }

    #endregion

    #region Volume Control Tests (Safe - Only Test Variables)

    [Test]
    public void GetMicrophoneVolume_ShouldReturnCurrentValue()
    {
        float volume = voiceChat.GetMicrophoneVolume();
        Assert.AreEqual(1f, volume, "Should return default microphone volume");
    }

    [Test]
    public void GetSpeakerVolume_ShouldReturnCurrentValue()
    {
        float volume = voiceChat.GetSpeakerVolume();
        Assert.AreEqual(1f, volume, "Should return default speaker volume");
    }

    [Test]
    public void SetMicrophoneVolume_ShouldUpdateVariable()
    {
        voiceChat.SetMicrophoneVolume(1.5f);
        Assert.AreEqual(1.5f, voiceChat.GetMicrophoneVolume(), "Should update microphone volume variable");
    }

    [Test]
    public void SetSpeakerVolume_ShouldUpdateVariable()
    {
        voiceChat.SetSpeakerVolume(0.8f);
        Assert.AreEqual(0.8f, voiceChat.GetSpeakerVolume(), "Should update speaker volume variable");
    }

    [Test]
    public void SetMicrophoneVolume_ShouldClampToValidRange()
    {
        voiceChat.SetMicrophoneVolume(3f); // Above max
        Assert.AreEqual(2f, voiceChat.GetMicrophoneVolume(), "Should clamp to maximum value 2.0");

        voiceChat.SetMicrophoneVolume(-1f); // Below min
        Assert.AreEqual(0f, voiceChat.GetMicrophoneVolume(), "Should clamp to minimum value 0.0");
    }

    [Test]
    public void SetSpeakerVolume_ShouldClampToValidRange()
    {
        voiceChat.SetSpeakerVolume(3f); // Above max
        Assert.AreEqual(2f, voiceChat.GetSpeakerVolume(), "Should clamp to maximum value 2.0");

        voiceChat.SetSpeakerVolume(-1f); // Below min
        Assert.AreEqual(0f, voiceChat.GetSpeakerVolume(), "Should clamp to minimum value 0.0");
    }

    #endregion

    #region Basic Mute System Tests (Safe - Only Dictionary Operations)

    [Test]
    public void IsPlayerMuted_WithEmptyPlayerId_ShouldReturnFalse()
    {
        Assert.IsFalse(voiceChat.IsPlayerMuted(""), "Empty player ID should return false");
        Assert.IsFalse(voiceChat.IsPlayerMuted(null), "Null player ID should return false");
    }

    [Test]
    public void IsPlayerMuted_WithValidPlayerId_Initially_ShouldReturnFalse()
    {
        Assert.IsFalse(voiceChat.IsPlayerMuted("player123"), "Player should not be muted initially");
    }

    [Test]
    public void MutePlayer_WithValidId_ShouldUpdateMuteStatus()
    {
        string playerId = "testPlayer";

        voiceChat.MutePlayer(playerId);
        Assert.IsTrue(voiceChat.IsPlayerMuted(playerId), "Player should be muted after MutePlayer call");
    }

    [Test]
    public void UnmutePlayer_WithValidId_ShouldUpdateMuteStatus()
    {
        string playerId = "testPlayer";

        voiceChat.MutePlayer(playerId);
        voiceChat.UnmutePlayer(playerId);
        Assert.IsFalse(voiceChat.IsPlayerMuted(playerId), "Player should be unmuted after UnmutePlayer call");
    }

    [Test]
    public void MutePlayer_WithEmptyId_ShouldNotCrash()
    {
        Assert.DoesNotThrow(() => voiceChat.MutePlayer(""));
        Assert.DoesNotThrow(() => voiceChat.MutePlayer(null));
    }

    [Test]
    public void GetMutedPlayerIds_Initially_ShouldBeEmpty()
    {
        List<string> mutedIds = voiceChat.GetMutedPlayerIds();
        Assert.IsNotNull(mutedIds, "Muted player IDs list should not be null");
        Assert.AreEqual(0, mutedIds.Count, "Should have no muted players initially");
    }

    [Test]
    public void GetMutedPlayerIds_AfterMuting_ShouldContainPlayer()
    {
        string playerId = "testPlayer";
        voiceChat.MutePlayer(playerId);

        List<string> mutedIds = voiceChat.GetMutedPlayerIds();
        Assert.Contains(playerId, mutedIds, "Muted player should be in the list");
        Assert.AreEqual(1, mutedIds.Count, "Should have exactly one muted player");
    }

    #endregion

    #region Safe Status Tests (No External Dependencies)

    [Test]
    public void IsMicrophoneEnabled_Initially_ShouldReturnFalse()
    {
        bool result = voiceChat.IsMicrophoneEnabled();
        Assert.IsFalse(result, "Microphone should not be enabled initially");
    }

    [Test]
    public void IsVoiceSystemReady_Initially_ShouldReturnFalse()
    {
        bool result = voiceChat.IsVoiceSystemReady();
        Assert.IsFalse(result, "Voice system should not be ready initially");
    }

    [Test]
    public void GetMicrophoneLevel_ShouldReturnValidValue()
    {
        float level = voiceChat.GetMicrophoneLevel();
        Assert.GreaterOrEqual(level, 0f, "Microphone level should not be negative");
    }

    [Test]
    public void GetConnectedPlayersCount_WithoutPhotonConnection_ShouldReturnZero()
    {
        int count = voiceChat.GetConnectedPlayersCount();
        Assert.AreEqual(0, count, "Should return 0 when not connected to Photon");
    }

    [Test]
    public void GetConnectedPlayerNames_WithoutPhotonConnection_ShouldReturnEmptyList()
    {
        List<string> names = voiceChat.GetConnectedPlayerNames();
        Assert.IsNotNull(names, "Player names list should not be null");
        Assert.AreEqual(0, names.Count, "Should return empty list when not connected to Photon");
    }

    #endregion

    #region Component Tests (Safe - Basic Unity)

    [Test]
    public void VoiceChat_ShouldBeMonoBehaviour()
    {
        Assert.IsInstanceOf<MonoBehaviour>(voiceChat, "VoiceChat should inherit from MonoBehaviour");
    }

    [Test]
    public void VoiceChat_GameObject_ShouldNotBeNull()
    {
        Assert.IsNotNull(voiceChat.gameObject, "VoiceChat GameObject should not be null");
        Assert.AreEqual("TestVoiceChat", voiceChat.gameObject.name, "GameObject should have correct name");
    }

    #endregion

    #region Boundary Value Tests (Safe - Pure Logic)

    [Test]
    [TestCase(0f)]
    [TestCase(0.5f)]
    [TestCase(1f)]
    [TestCase(2f)]
    public void SetMicrophoneVolume_WithValidValues_ShouldAccept(float volume)
    {
        voiceChat.SetMicrophoneVolume(volume);
        Assert.AreEqual(volume, voiceChat.GetMicrophoneVolume(), $"Should accept valid volume {volume}");
    }

    [Test]
    [TestCase(-1f, 0f)]
    [TestCase(3f, 2f)]
    [TestCase(10f, 2f)]
    public void SetMicrophoneVolume_WithInvalidValues_ShouldClamp(float input, float expected)
    {
        voiceChat.SetMicrophoneVolume(input);
        Assert.AreEqual(expected, voiceChat.GetMicrophoneVolume(), $"Input {input} should be clamped to {expected}");
    }

    [Test]
    [TestCase(0f)]
    [TestCase(0.5f)]
    [TestCase(1f)]
    [TestCase(2f)]
    public void SetSpeakerVolume_WithValidValues_ShouldAccept(float volume)
    {
        voiceChat.SetSpeakerVolume(volume);
        Assert.AreEqual(volume, voiceChat.GetSpeakerVolume(), $"Should accept valid volume {volume}");
    }

    [Test]
    [TestCase(-1f, 0f)]
    [TestCase(3f, 2f)]
    [TestCase(10f, 2f)]
    public void SetSpeakerVolume_WithInvalidValues_ShouldClamp(float input, float expected)
    {
        voiceChat.SetSpeakerVolume(input);
        Assert.AreEqual(expected, voiceChat.GetSpeakerVolume(), $"Input {input} should be clamped to {expected}");
    }

    #endregion
}
