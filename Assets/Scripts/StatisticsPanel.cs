using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class StatisticsPanelToggle : MonoBehaviour
{
    public GameObject statisticsPanel;
    public TMP_Text usernameText;
    public TMP_Text playtimeText;
    public TMP_Text clicksText;
    public TMP_Text sessionsText;
    public TMP_Text moneyText;
    public TMP_Text killsText;

    private bool panelActive = false;

    void Start()
    {
        if (statisticsPanel != null)
            statisticsPanel.SetActive(false);
    }

    void Update()
    {
        if (statisticsPanel == null)
            return;

        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
        {
            panelActive = !panelActive;
            statisticsPanel.SetActive(panelActive);
        }

        if (panelActive)
        {
            var user = AccountManager.Instance.currentUser;
            if (user != null && user.stats != null)
            {
                user.stats.totalPlayTime += (int)Time.deltaTime;

                if (usernameText != null)
                {
                    usernameText.text = user.username;
                }
                if (playtimeText != null)
                {
                    playtimeText.text = $"{user.stats.totalPlayTime:F2} sec";
                }
                if (clicksText != null)
                {
                    clicksText.text = $"{user.stats.totalClicks}";
                }
                if (sessionsText != null)
                {
                    sessionsText.text = $"{user.stats.sessionsPlayed}";
                }
                if (moneyText != null)
                {
                    moneyText.text = $"{user.stats.totalMoney}";
                }
                if (killsText != null)
                {
                    killsText.text = $"{user.stats.kills}";
                }

                Debug.Log($"Updating stats: {user.stats.totalPlayTime:F2}");
            }
        }
    }
}
