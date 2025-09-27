using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    [Header("Character Management")]
    private List<Character> playerCharacters = new List<Character>();
    private List<Vector3> playerStartPositions = new List<Vector3>();
    private Dictionary<Character, float> originalSpeeds = new Dictionary<Character, float>();

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

        // 플레이어 캐릭터들 찾기
        FindAllPlayerCharacters();

        // Initialize UI
        UpdateRoundDisplay();

        Debug.Log("[RoundManager] === 좀비 서바이벌 게임 시작 ===");
        Debug.Log($"[RoundManager] 총 라운드: {maxRounds}, 라운드당 시간: {roundDuration}초");
        Debug.Log($"[RoundManager] 발견된 플레이어 수: {playerCharacters.Count}");

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
                Debug.Log("[RoundManager] ⏰ 시간 종료! 좀비들로부터 살아남았습니다!");
                EndRound();
            }
        }

        // 라운드 상태에 따른 캐릭터 제어
        ControlPlayerMovement();

        // Debug controls (remove in production)
        HandleDebugInput();
    }

    void FindAllPlayerCharacters()
    {
        Character[] characters = FindObjectsOfType<Character>();
        playerCharacters.Clear();
        playerStartPositions.Clear();
        originalSpeeds.Clear();

        foreach (Character character in characters)
        {
            playerCharacters.Add(character);
            playerStartPositions.Add(character.transform.position);
            originalSpeeds[character] = character.Speed; // 원래 속도 저장

            Debug.Log($"[RoundManager] 🎮 플레이어 발견: {character.gameObject.name}");
            Debug.Log($"  - 시작 위치: {character.transform.position}");
            Debug.Log($"  - 원래 속도: {character.Speed}");
        }

        if (playerCharacters.Count == 0)
        {
            Debug.LogWarning("[RoundManager] ⚠️ 플레이어 캐릭터가 없습니다!");
        }
    }

    void ControlPlayerMovement()
    {
        if (!isRoundActive)
        {
            // 라운드 비활성화 시 모든 플레이어 정지
            FreezeAllPlayers();
        }
        else
        {
            // 라운드 활성화 시 모든 플레이어 움직임 허용
            UnfreezeAllPlayers();
        }
    }

    void FreezeAllPlayers()
    {
        foreach (Character player in playerCharacters)
        {
            if (player != null)
            {
                player.Speed = 0f;
            }
        }
    }

    void UnfreezeAllPlayers()
    {
        foreach (Character player in playerCharacters)
        {
            if (player != null && originalSpeeds.ContainsKey(player))
            {
                player.Speed = originalSpeeds[player];
            }
        }
    }

    void ResetAllPlayerPositions()
    {
        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null && i < playerStartPositions.Count)
            {
                CharacterController cc = playerCharacters[i].GetComponent<CharacterController>();
                if (cc != null)
                {
                    cc.enabled = false;
                    playerCharacters[i].transform.position = playerStartPositions[i];
                    cc.enabled = true;
                }
                else
                {
                    playerCharacters[i].transform.position = playerStartPositions[i];
                }
                Debug.Log($"[RoundManager] 📍 {playerCharacters[i].gameObject.name} 시작 위치로 이동");
            }
        }
    }

    void HandleDebugInput()
    {
        // R key to manually start next round
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (currentState == RoundState.WaitingToStart)
            {
                Debug.Log("[RoundManager] 🔧 R키 - 라운드 수동 시작");
                StartRound();
            }
            else if (currentState == RoundState.InProgress)
            {
                Debug.Log("[RoundManager] 🔧 R키 - 라운드 수동 종료");
                EndRound();
            }
        }

        // G key to end game
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("[RoundManager] 🔧 G키 - 게임 강제 종료");
            EndGame();
        }

        // Reset game with F5
        if (Input.GetKeyDown(KeyCode.F5))
        {
            Debug.Log("[RoundManager] 🔧 F5키 - 게임 리셋");
            ResetGame();
        }

        // C key to show current status
        if (Input.GetKeyDown(KeyCode.C))
        {
            ShowCurrentStatus();
        }

        // Z key to simulate zombie kill (for testing round progression)
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("[RoundManager] 🔧 Z키 - 좀비 처치 시뮬레이션 (모든 좀비 처치됨)");
            OnAllZombiesKilled();
        }
    }

    void ShowCurrentStatus()
    {
        Debug.Log("=== 🎮 현재 게임 상태 ===");
        Debug.Log($"현재 라운드: {currentRound}/{maxRounds}");
        Debug.Log($"라운드 상태: {currentState}");
        Debug.Log($"남은 시간: {currentRoundTime:F1}초");
        Debug.Log($"라운드 활성화: {isRoundActive}");
        Debug.Log($"플레이어 수: {playerCharacters.Count}");

        for (int i = 0; i < playerCharacters.Count; i++)
        {
            if (playerCharacters[i] != null)
            {
                Debug.Log($"  - {playerCharacters[i].gameObject.name}: Speed = {playerCharacters[i].Speed}");
                Debug.Log($"    위치: {playerCharacters[i].transform.position}");
            }
        }

        Debug.Log("=== 🎮 디버그 키 ===");
        Debug.Log("R: 라운드 시작/종료");
        Debug.Log("G: 게임 종료");
        Debug.Log("F5: 게임 리셋");
        Debug.Log("C: 현재 상태 표시");
        Debug.Log("Z: 좀비 전멸 시뮬레이션");
    }

    IEnumerator StartRoundCountdown(float delay)
    {
        Debug.Log($"[RoundManager] 🚨 라운드 {currentRound} 카운트다운 시작!");

        if (roundStatusText != null)
        {
            for (int i = (int)delay; i > 0; i--)
            {
                string message = $"Round {currentRound} starts in: {i}";
                roundStatusText.text = message;
                Debug.Log($"[RoundManager] ⏰ {message}");
                yield return new WaitForSeconds(1f);
            }
            roundStatusText.text = $"Round {currentRound} - SURVIVE!";
            Debug.Log($"[RoundManager] 🧟 라운드 {currentRound} - 좀비들이 나타났다!");
        }
        else
        {
            // UI가 없을 때는 콘솔로만
            for (int i = (int)delay; i > 0; i--)
            {
                Debug.Log($"[RoundManager] ⏰ 라운드 {currentRound} 시작까지: {i}초");
                yield return new WaitForSeconds(1f);
            }
            Debug.Log($"[RoundManager] 🧟 라운드 {currentRound} - 좀비 웨이브 시작!");
        }

        yield return new WaitForSeconds(1f);
        StartRound();
    }

    public void StartRound()
    {
        if (currentRound > maxRounds)
        {
            Debug.Log("[RoundManager] 🎉 모든 웨이브를 클리어했습니다!");
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
            Debug.Log("[RoundManager] 🔊 라운드 시작 사운드 재생");
        }

        // Update UI
        UpdateRoundDisplay();
        if (roundStatusText != null)
        {
            roundStatusText.text = $"Round {currentRound} - FIGHT!";
        }

        // Hide round end panel
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(false);
        }

        // 플레이어들을 시작 위치로 이동하고 움직임 허용
        ResetAllPlayerPositions();
        UnfreezeAllPlayers();

        // Notify other systems (좀비 스포너, 무기 시스템 등)
        OnRoundStart?.Invoke(currentRound);

        Debug.Log($"[RoundManager] 🧟 === 라운드 {currentRound} 시작! ===");
        Debug.Log($"[RoundManager] ⏰ 생존 시간: {roundDuration}초");
        Debug.Log($"[RoundManager] 🏃 플레이어들 움직임 활성화!");
        Debug.Log($"[RoundManager] 🎯 목표: {roundDuration}초 동안 살아남으세요!");
    }

    public void EndRound()
    {
        if (!isRoundActive)
        {
            Debug.Log("[RoundManager] ⚠️ 이미 종료된 라운드입니다.");
            return;
        }

        currentState = RoundState.Ending;
        isRoundActive = false;

        // Play sound effect
        if (audioSource != null && roundEndSound != null)
        {
            audioSource.PlayOneShot(roundEndSound);
            Debug.Log("[RoundManager] 🔊 라운드 클리어 사운드 재생");
        }

        // Update UI
        if (roundStatusText != null)
        {
            roundStatusText.text = $"Round {currentRound} - SURVIVED!";
        }

        // Show round end panel
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(true);
        }

        // 플레이어 움직임 정지
        FreezeAllPlayers();

        // Notify other systems
        OnRoundEnd?.Invoke(currentRound);

        Debug.Log($"[RoundManager] 🎉 === 라운드 {currentRound} 클리어! ===");
        Debug.Log($"[RoundManager] 🛡️ 좀비 웨이브에서 살아남았습니다!");
        Debug.Log($"[RoundManager] ⏸️ 플레이어들 움직임 정지");

        // Prepare for next round
        currentRound++;
        StartCoroutine(PrepareNextRound());
    }

    IEnumerator PrepareNextRound()
    {
        Debug.Log("[RoundManager] ⏳ 다음 웨이브 준비 중... (3초 휴식)");
        Debug.Log("[RoundManager] 💊 체력 회복 및 탄약 보급 시간!");
        yield return new WaitForSeconds(3f);

        if (currentRound <= maxRounds)
        {
            currentState = RoundState.WaitingToStart;
            Debug.Log($"[RoundManager] 🔄 웨이브 {currentRound} 준비 완료");
            StartCoroutine(StartRoundCountdown(3f));
        }
        else
        {
            Debug.Log("[RoundManager] 🏆 최종 웨이브 완료!");
            EndGame();
        }
    }

    public void EndGame()
    {
        currentState = RoundState.GameOver;
        isRoundActive = false;

        // 모든 플레이어 정지
        FreezeAllPlayers();

        if (roundStatusText != null)
        {
            roundStatusText.text = $"YOU SURVIVED! Waves: {currentRound - 1}";
        }

        // Notify other systems
        OnGameEnd?.Invoke();

        Debug.Log("🏆 === 게임 클리어! ===");
        Debug.Log($"[RoundManager] 🧟 총 클리어한 웨이브: {currentRound - 1}");
        Debug.Log($"[RoundManager] 🎯 좀비 아포칼립스에서 살아남았습니다!");
        Debug.Log($"[RoundManager] 🛑 모든 플레이어 움직임 정지");
    }

    public void ResetGame()
    {
        Debug.Log("🔄 === 게임 리셋! ===");

        currentRound = 1;
        currentState = RoundState.WaitingToStart;
        isRoundActive = false;
        currentRoundTime = roundDuration;

        UpdateRoundDisplay();
        if (roundEndPanel != null)
        {
            roundEndPanel.SetActive(false);
        }

        // 플레이어들 다시 찾기
        FindAllPlayerCharacters();

        StartCoroutine(StartRoundCountdown(3f));
        Debug.Log("[RoundManager] 🎮 새로운 좀비 서바이벌 시작!");
    }

    void UpdateRoundDisplay()
    {
        if (roundNumberText != null)
        {
            roundNumberText.text = $"Wave: {currentRound}/{maxRounds}";
        }

        // UI가 없을 때 콘솔로 표시
        if (roundNumberText == null)
        {
            Debug.Log($"[RoundManager] 📊 현재 웨이브: {currentRound}/{maxRounds}");
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

        // 10초마다 콘솔에 시간 표시
        if (roundTimerText == null && enableRoundTimer && Time.frameCount % 600 == 0)
        {
            int minutes = Mathf.FloorToInt(currentRoundTime / 60f);
            int seconds = Mathf.FloorToInt(currentRoundTime % 60f);
            Debug.Log($"[RoundManager] ⏰ 생존 남은 시간: {minutes:00}:{seconds:00}");
        }
    }

    // 좀비가 모두 처치되었을 때 호출할 메서드 (좀비 스포너에서 호출)
    public void OnAllZombiesKilled()
    {
        if (isRoundActive)
        {
            Debug.Log("[RoundManager] 💀 모든 좀비 처치 완료! 라운드 조기 클리어!");
            EndRound();
        }
    }

    // 플레이어가 죽었을 때 호출할 메서드
    public void OnPlayerDeath(Character player)
    {
        if (player != null)
        {
            Debug.Log($"[RoundManager] 💀 {player.gameObject.name} 사망!");
        }

        // 모든 플레이어가 죽었는지 확인
        bool allPlayersDead = true;
        foreach (Character p in playerCharacters)
        {
            if (p != null && p.gameObject.activeInHierarchy)
            {
                allPlayersDead = false;
                break;
            }
        }

        if (allPlayersDead)
        {
            Debug.Log("[RoundManager] 💀 모든 플레이어 사망! 게임 오버!");
            // 게임 오버 처리 (필요에 따라 구현)
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

    public int GetPlayerCount()
    {
        return playerCharacters.Count;
    }

    // Method to manually trigger round end (for game logic)
    public void TriggerRoundEnd()
    {
        if (isRoundActive)
        {
            Debug.Log("[RoundManager] 🔧 외부에서 라운드 종료 요청");
            EndRound();
        }
    }
}