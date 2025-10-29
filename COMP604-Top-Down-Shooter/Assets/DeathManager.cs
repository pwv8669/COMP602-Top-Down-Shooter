using UnityEngine;
using System.Collections; // Required for Coroutines

public class DeathManager : MonoBehaviour
{
    // A reference to the death screen's CanvasGroup
    public CanvasGroup deathScreenCanvasGroup;

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

        // Optional: After fading in, you could load the main menu or allow a restart
        // For example: UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}