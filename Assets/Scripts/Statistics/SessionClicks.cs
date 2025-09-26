using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SessionClicks : MonoBehaviour
{
    public TextMeshProUGUI sessionClickText;
    private int sessionClicks;

    void Start()
    {
        sessionClicks = 0;
        sessionClickText.text = sessionClicks.ToString();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // session click counter
            sessionClicks++;
            sessionClickText.text = sessionClicks.ToString();

            // click totals update
            if (AccountManager.Instance != null && AccountManager.Instance.currentUser != null)
            {
                AccountManager.Instance.currentUser.stats.totalClicks++;
                AccountManager.Instance.SaveProgress();
            }
        }
    }

    public int GetSessionClicks()
    {
        return sessionClicks;
    }
}
