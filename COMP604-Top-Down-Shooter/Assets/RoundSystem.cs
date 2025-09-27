using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoundManager : MonoBehaviour
{
    [Header("Round Settings")]
    public int currentRound = 1;
    public int maxRounds = 10;
    public float roundDuration = 60f; // seconds
    public bool enableRoundTimer = true;

    [Header("UI References")]
    public Text roundNumberText;
    public Text roundTimerText;
    public Text roundStatusText;
    public GameObject roundEndPanel;

    [Header("Audio (Optional)")]
    public AudioClip roundStartSound;
    public AudioClip roundEndSound;
    private AudioSource audioSource;

    // Round state
    public enum RoundState
    {
        WaitingToStart,
        InProgress,
        Ending,
        GameOver
    }

    public RoundState currentState = RoundState.WaitingToStart;
    private float currentRoundTime;
    private bool isRoundActive = false;

    // Events for other systems to subscribe to
    public System.Action<int> OnRoundStart;
    public System.Action<int> OnRoundEnd;
    public System.Action OnGameEnd;

    void Start()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Initialize UI
        UpdateRoundDisplay();
        
        // Start first round after a short delay
        StartCoroutine(StartRoundCountdown(3f));
    }

    void Update()
    {
        // Handle round timer
        if (isRoundActive && enableRoundTimer)
        {
            currentRoundTime -= Time.deltaTime;
            UpdateTimerDisplay();

            if (currentRoundTime <= 0)
            {
                EndRound();
            }
        }

        // Debug controls (remove in production)
        HandleDebugInput();
    }

    void HandleDebugInput()
    {
        // R key to manually start next round
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentState == RoundState.WaitingToStart)
            {
                StartRound();
            }
            else if (currentState == RoundState.InProgress)
            {
                EndRound();
            }
        }

        // G key to end game
        if (Input.GetKeyDown(KeyCode.G))
        {
            EndGame();
        }

        // Reset game with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ResetGame();
        }
    }

    IEnumerator StartRoundCountdown(float delay)
    {
        if (roundStatusText != null)
        {
            for (int i = (int)delay; i > 0; i--)
            {
                roundStatusText.text = "Round " + currentRound + " starts in: " + i;
                yield return new WaitForSeconds(1f);
            }
            roundStatusText.text = "Round " + currentRound + " - GO!";
        }
        
        yield return new WaitForSeconds(1f);
        StartRound();
    }

    public void StartRound()
    {
        if (currentRound > maxRounds)
        {
            EndGame();
            return;
        }

        currentState = RoundState.InProgress;
        isRoundActive = true;
        currentRoundTime = roundDuration;

        // Play sound effect
        if (audioSource != null && roundStartSound != null)
        {
            audioSource.PlayOneShot(roundStartSound);
        }

        // Update UI
        UpdateRoundDisplay();
        if (roundStatusText != null)
        {
            roundStatusText.text = "Round " + currentRound + " - Fight!";
        }

        // Hide round end panel
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(false);
        }

        // Notify other systems
        OnRoundStart?.Invoke(currentRound);

        Debug.Log("[RoundManager] Round " + currentRound + " started!");
    }

    public void EndRound()
    {
        if (!isRoundActive) return;

        currentState = RoundState.Ending;
        isRoundActive = false;

        // Play sound effect
        if (audioSource != null && roundEndSound != null)
        {
            audioSource.PlayOneShot(roundEndSound);
        }

        // Update UI
        if (roundStatusText != null)
        {
            roundStatusText.text = "Round " + currentRound + " Complete!";
        }

        // Show round end panel
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(true);
        }

        // Notify other systems
        OnRoundEnd?.Invoke(currentRound);

        Debug.Log("[RoundManager] Round " + currentRound + " ended!");

        // Prepare for next round
        currentRound++;
        StartCoroutine(PrepareNextRound());
    }

    IEnumerator PrepareNextRound()
    {
        yield return new WaitForSeconds(3f);
        
        if (currentRound <= maxRounds)
        {
            currentState = RoundState.WaitingToStart;
            StartCoroutine(StartRoundCountdown(3f));
        }
        else
        {
            EndGame();
        }
    }

    public void EndGame()
    {
        currentState = RoundState.GameOver;
        isRoundActive = false;

        if (roundStatusText != null)
        {
            roundStatusText.text = "Game Complete! Final Round: " + (currentRound - 1);
        }

        // Notify other systems
        OnGameEnd?.Invoke();

        Debug.Log("[RoundManager] Game ended! Total rounds: " + (currentRound - 1));
    }

    public void ResetGame()
    {
        currentRound = 1;
        currentState = RoundState.WaitingToStart;
        isRoundActive = false;
        currentRoundTime = roundDuration;

        UpdateRoundDisplay();
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(false);
        }

        StartCoroutine(StartRoundCountdown(3f));
        Debug.Log("[RoundManager] Game reset!");
    }

    void UpdateRoundDisplay()
    {
        if (roundNumberText != null)
        {
            roundNumberText.text = "Round: " + currentRound + "/" + maxRounds;
        }
    }

    void UpdateTimerDisplay()
    {
        if (roundTimerText != null && enableRoundTimer)
        {
            int minutes = Mathf.FloorToInt(currentRoundTime / 60f);
            int seconds = Mathf.FloorToInt(currentRoundTime % 60f);
            roundTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Public methods for other systems to interact with rounds
    public bool IsRoundActive()
    {
        return isRoundActive;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public float GetRoundTimeRemaining()
    {
        return currentRoundTime;
    }

    public RoundState GetCurrentState()
    {
        return currentState;
    }

    // Method to manually trigger round end (for game logic)
    public void TriggerRoundEnd()
    {
        if (isRoundActive)
        {
            EndRound();
        }
    }
}