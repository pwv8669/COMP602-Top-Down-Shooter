using NUnit.Framework;
using UnityEngine;

public class MultiplayerTests
{
    private Multiplayer multiplayer;
    private GameObject testObject;

    [SetUp]
    public void Setup()
    {
        testObject = new GameObject("TestMultiplayer");
        multiplayer = testObject.AddComponent<Multiplayer>();
    }

    [TearDown]
    public void TearDown()
    {
        if (testObject != null)
        {
            Object.DestroyImmediate(testObject);
        }
    }

    #region Status Check Tests (No Log Errors)

    [Test]
    public void IsReadyToCreateRoom_Initially_ShouldReturnFalse()
    {
        bool result = multiplayer.IsReadyToCreateRoom();
        Assert.IsFalse(result, "Should not be ready to create room initially");
    }

    [Test]
    public void IsReadyToJoinRoom_Initially_ShouldReturnFalse()
    {
        bool result = multiplayer.IsReadyToJoinRoom();
        Assert.IsFalse(result, "Should not be ready to join room initially");
    }

    [Test]
    public void IsConnectedToPhoton_Initially_ShouldReturnFalse()
    {
        bool result = multiplayer.IsConnectedToPhoton();
        Assert.IsFalse(result, "Should not be connected to Photon initially");
    }

    [Test]
    public void IsInRoom_Initially_ShouldReturnFalse()
    {
        bool result = multiplayer.IsInRoom();
        Assert.IsFalse(result, "Should not be in room initially");
    }

    [Test]
    public void IsMasterClient_Initially_ShouldReturnFalse()
    {
        bool result = multiplayer.IsMasterClient();
        Assert.IsFalse(result, "Should not be master client initially");
    }

    #endregion

    #region Settings Tests (No Log Errors)

    [Test]
    public void MaxPlayersPerRoom_ShouldHaveValidDefault()
    {
        Assert.AreEqual(4, multiplayer.maxPlayersPerRoom, "Default max players should be 4");
        Assert.Greater(multiplayer.maxPlayersPerRoom, 0, "Max players should be greater than 0");
        Assert.LessOrEqual(multiplayer.maxPlayersPerRoom, 20, "Max players should not exceed Photon's room limit");
    }

    [Test]
    public void GameVersion_ShouldHaveCorrectDefault()
    {
        Assert.IsNotEmpty(multiplayer.gameVersion, "Game version should not be empty");
        Assert.AreEqual("1.0", multiplayer.gameVersion, "Default game version should be 1.0");
    }

    [Test]
    public void ShowDebugLogs_ShouldHaveDefaultValue()
    {
        Assert.IsTrue(multiplayer.showDebugLogs, "ShowDebugLogs should be true by default");
    }

    #endregion

    #region Component Tests (No Log Errors)

    [Test]
    public void Multiplayer_ShouldBeMonoBehaviour()
    {
        Assert.IsInstanceOf<MonoBehaviour>(multiplayer, "Multiplayer should inherit from MonoBehaviour");
    }

    [Test]
    public void Multiplayer_GameObject_ShouldNotBeNull()
    {
        Assert.IsNotNull(multiplayer.gameObject, "Multiplayer GameObject should not be null");
        Assert.AreEqual("TestMultiplayer", multiplayer.gameObject.name, "GameObject should have correct name");
    }

    #endregion

    #region Room Code Validation Helper Tests (No Log Errors)

    [Test]
    [TestCase("ABC123", true)]
    [TestCase("ABCDEF", true)]
    [TestCase("123456", true)]
    [TestCase("abc123", false)] // lowercase
    [TestCase("ABC12", false)]  // too short
    [TestCase("ABC1234", false)] // too long
    [TestCase("", false)]       // empty
    [TestCase(null, false)]     // null
    [TestCase("ABC@12", false)] // special character
    public void ValidateRoomCode_ShouldReturnCorrectResult(string roomCode, bool expectedResult)
    {
        bool result = IsValidRoomCode(roomCode);
        Assert.AreEqual(expectedResult, result, $"Room code '{roomCode}' validation should return {expectedResult}");
    }

    // Helper method to test room code validation logic
    private bool IsValidRoomCode(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode) || roomCode.Length != 6)
            return false;

        foreach (char c in roomCode)
        {
            if (!char.IsLetterOrDigit(c) || (char.IsLetter(c) && !char.IsUpper(c)))
                return false;
        }
        return true;
    }

    #endregion

    #region Settings Boundary Tests (No Log Errors)

    [Test]
    public void MaxPlayersPerRoom_ShouldBeWithinPhotonLimits()
    {
        Assert.GreaterOrEqual(multiplayer.maxPlayersPerRoom, 1, "Should allow at least 1 player");
        Assert.LessOrEqual(multiplayer.maxPlayersPerRoom, 20, "Should not exceed Photon's 20 player limit");
    }

    [Test]
    public void GameVersion_ShouldFollowVersionFormat()
    {
        string version = multiplayer.gameVersion;
        Assert.IsTrue(version.Contains("."), "Version should contain a dot separator");
        Assert.IsTrue(version.Length >= 3, "Version should be at least 3 characters (e.g., '1.0')");
    }

    #endregion
}