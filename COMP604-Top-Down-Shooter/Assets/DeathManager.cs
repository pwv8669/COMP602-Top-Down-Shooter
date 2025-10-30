using UnityEngine;
using System.Collections;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviourPunCallbacks
{
    // A reference to the death screen's CanvasGroup
    public CanvasGroup deathScreenCanvasGroup;

    // Scene name to return to
    public string mainMenuSceneName = "MainMenu";

    // A static instance of the DeathManager to make it easily accessible
    public static DeathManager Instance { get; private set; }

    private void Awake()
    {
        // Set up the singleton instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowDeathScreen()
    {
        Debug.Log("Player has died. Showing death screen.");
        // Start the fade-in coroutine
        StartCoroutine(FadeInDeathScreen());
    }

    private IEnumerator FadeInDeathScreen()
    {
        float duration = 1.5f; // Duration of the fade in seconds
        float currentTime = 0f;

        // Enable the CanvasGroup to block mouse clicks
        deathScreenCanvasGroup.blocksRaycasts = true;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            // Gradually increase the alpha of the CanvasGroup
            deathScreenCanvasGroup.alpha = Mathf.Lerp(0, 1, currentTime / duration);
            yield return null;
        }

        // After fading in, return to main menu
        Debug.Log("Death screen fade complete. Returning to main menu...");
        yield return new WaitForSeconds(10f); // Optional: brief pause before returning
        ReturnToMainMenu();
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");

        // Disconnect from Photon if connected
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Disconnecting from Photon...");
            PhotonNetwork.Disconnect();
        }

        // Load main menu scene
        StartCoroutine(LoadMainMenuScene());
    }

    private IEnumerator LoadMainMenuScene()
    {
        // Wait for Photon to disconnect if it was connected
        if (PhotonNetwork.IsConnected)
        {
            float timeout = 3f;
            float elapsed = 0f;

            while (PhotonNetwork.IsConnected && elapsed < timeout)
            {
                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }

            if (PhotonNetwork.IsConnected)
            {
                Debug.LogWarning("Timeout waiting for disconnect, loading anyway...");
            }
        }

        // Load the main menu scene
        Debug.Log($"Loading scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}