using UnityEngine;
using TMPro;

public class SessionTimer : MonoBehaviour
{
    private float sessionTime = 0f;             // Tracks session time for display
    private float playtimeAccumulator = 0f;     // Tracks partial seconds for adding to totalPlayTime

    public TextMeshProUGUI timerText;

    void Update()
    {
        sessionTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(sessionTime / 60);
        int seconds = Mathf.FloorToInt(sessionTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (AccountManager.Instance != null && AccountManager.Instance.currentUser != null)
        {
            playtimeAccumulator += Time.deltaTime;
            if (playtimeAccumulator >= 1f)
            {
                int secondsToAdd = Mathf.FloorToInt(playtimeAccumulator);
                AccountManager.Instance.currentUser.stats.totalPlayTime += secondsToAdd;
                playtimeAccumulator -= secondsToAdd;

                AccountManager.Instance.SaveProgress();
            }
        }
    }

    public int GetSessionTime()
    {
        return Mathf.FloorToInt(sessionTime);
    }
}
