using UnityEngine;
using UnityEngine.UIElements;

public class Level2Manager : MonoBehaviour
{
    public static Level2Manager Instance { get; private set; }

    [Header("Mood")]
    public float MaxMood = 100f;
    public float CurrentMood = 100f;

    [Header("Energy")]
    public float MaxEnergy = 100f;
    public float Energy = 100f;

    [Header("Memory Tokens")]
    public int memoryTokensCollected = 0;
    public int totalMemoryTokens = 5;

    [Header("Hope Sparks")]
    public int hopeSparkCount = 3;
    public float hopeMoodRestore = 20f;

    public bool IsGameOver { get; private set; }
    public bool IsWin { get; private set; }

    // ── Compatibility properties (so other scripts can read these names) ──
    public float Mood => CurrentMood;
    public int MemoryTokensCollected => memoryTokensCollected;
    public int HopeSparkCount => hopeSparkCount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        IsGameOver = false;
        IsWin = false;
    }

    void Update()
    {
        if (IsGameOver) return;

        if (CurrentMood <= 0f || Energy <= 0f)
        {
            IsGameOver = true;
            IsWin = false;
        }
    }

    public bool AllTokensCollected() => memoryTokensCollected >= totalMemoryTokens;

    public void OnBeaconReached()
    {
        IsGameOver = true;
        IsWin = true;
    }

    // ── Called by ShadowNPC when it touches the player ──
    public void ReduceMood(float amount)
    {
        CurrentMood = Mathf.Clamp(CurrentMood - amount, 0f, MaxMood);

        if (CurrentMood <= 0f)
        {
            IsGameOver = true;
            IsWin = false;
        }
    }

    // ── Called by MemoryTokenPickup when the player collects a memory ──
    public void CollectMemoryToken(float moodGain, float energyGain)
    {
        memoryTokensCollected++;
        CurrentMood = Mathf.Min(CurrentMood + moodGain, MaxMood);
        Energy      = Mathf.Min(Energy + energyGain, MaxEnergy);
    }

    public void TryUseHopeSpark()
    {
        if (hopeSparkCount <= 0) return;
        hopeSparkCount--;
        CurrentMood = Mathf.Min(CurrentMood + hopeMoodRestore, MaxMood);
    }
}
