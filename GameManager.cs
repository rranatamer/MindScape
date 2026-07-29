using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    
    public static GameManager Instance { get; private set; }

    
    [Header("Player Stats")]
    public int Stress;
    public int Confidence;
    public int PositiveCardCount;

    [Header("Limits")]
    public int MaxStress      = 100;
    public int MaxConfidence  = 100;
    public int WinStressThreshold     = 40;
    public int WinConfidenceThreshold = 30;
    public int RequiredPositiveCards  = 2;  

    [Header("Zone Card Tracking")]
    [Tooltip("How many cards the player must collect IN THIS zone before the exit unlocks. Set per scene: Zone A = 10, Zone B = 15, Zone C = 15.")]
    public int cardsRequiredThisZone = 10;
    [Tooltip("Cards collected in the current zone (resets each scene). Read-only.")]
    public int cardsInCurrentZone = 0;

    [Header("Collision Limit")]
    public int CollisionCount;
    public int MaxCollisions = 5;

    [Header("High-Stress Audio")]
    [Tooltip("When Stress reaches this value, heartbeat + breathing start. They stop when it drops below.")]
    public int highStressThreshold = 60;
    private bool highStressAudioOn = false;

    [Header("Game State")]
    public bool IsGameOver;
    public bool IsWin;

    
    private bool reachedSafeSpot = false;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResetStats(); // Always start clean
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!IsGameOver)
        {
            CheckWinLoseConditions();
            UpdateHighStressAudio();
        }

        
        if (IsGameOver && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartLevel();
        }
    }

    
    
    public void OnSafeSpotReached()
    {
        reachedSafeSpot = true;
    }
    void UpdateHighStressAudio()
    {
        if (AudioManager.Instance == null) return;

        bool shouldBeOn = Stress >= highStressThreshold;

        if (shouldBeOn && !highStressAudioOn)
        {
            highStressAudioOn = true;
            AudioManager.Instance.OnEnemyAppear();    // fade heartbeat in
            AudioManager.Instance.PlayBreathing(true);
        }
        else if (!shouldBeOn && highStressAudioOn)
        {
            highStressAudioOn = false;
            AudioManager.Instance.OnEnemyDisappear(); // fade heartbeat down
            AudioManager.Instance.PlayBreathing(false);
        }
    }

    
    public void AddStress(int amount)
    {
        Stress = Mathf.Clamp(Stress + amount, 0, MaxStress);
    }

    public void ReduceStress(int amount)
    {
        Stress = Mathf.Clamp(Stress - amount, 0, MaxStress);
    }

    public void AddConfidence(int amount)
    {
        Confidence = Mathf.Clamp(Confidence + amount, 0, MaxConfidence);
    }

    public void ReduceConfidence(int amount)
    {
        Confidence = Mathf.Clamp(Confidence - amount, 0, MaxConfidence);
    }

    
    public void CollectPositiveCard()
    {
        PositiveCardCount++;
        cardsInCurrentZone++;
        AddConfidence(15);
        ReduceStress(10);
        if (NegativeTextManager.Instance != null)
            NegativeTextManager.Instance.RemoveThoughts();
    }
    public bool ZoneCardsComplete()
    {
        return cardsInCurrentZone >= cardsRequiredThisZone;
    }
    public void ResetZoneCards()
    {
        cardsInCurrentZone = 0;
    }

    public void BreathingSuccess()
    {
        ReduceStress(15);
        AddConfidence(10);
    }

    public void UseCalmBomb()
    {
        ReduceStress(25);

        
        if (Time.timeScale > 0f)
        {
            Time.timeScale = 0.5f;
            Invoke(nameof(ResetTimeScale), 2f);
        }
    }

    void ResetTimeScale()
    {
        
        if (!PauseMenuUI.IsPaused)
            Time.timeScale = 1f;
    }

    public void EnemyImpact()
    {
        AddStress(10);
        ReduceConfidence(5);
        CollisionCount++;
    }

    
    void CheckWinLoseConditions()
    {
        
        if (Stress >= MaxStress || CollisionCount >= MaxCollisions)
        {
            TriggerGameOver(win: false);
            return;
        }

        
        if (reachedSafeSpot
            && Stress       <= WinStressThreshold
            && Confidence   >= WinConfidenceThreshold
            && PositiveCardCount >= RequiredPositiveCards)
        {
            TriggerGameOver(win: true);
        }
    }

    void TriggerGameOver(bool win)
    {
        IsGameOver = true;
        IsWin      = win;

        if (AudioManager.Instance != null)
        {
            AudioClip clip = win ? AudioManager.Instance.gameWin : AudioManager.Instance.gameOver;
            AudioManager.Instance.PlaySFX(clip);
        }
    }
    void RestartLevel()
    {
        ResetStats();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ResetStats()
    {
        Stress           = 0;
        Confidence       = 50;
        PositiveCardCount = 0;
        CollisionCount   = 0;
        IsGameOver       = false;
        IsWin            = false;
        reachedSafeSpot  = false;
        highStressAudioOn = false;
        Time.timeScale   = 1f; 
    }
}