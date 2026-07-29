using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
public class Level2UIManager : MonoBehaviour
{
    [Header("Meters")]
    public Slider moodBar;
    public Slider energyBar;

    [Header("Energy Bar Fill (optional pulse when low)")]
    public Image energyBarFill;

    [Header("Counters")]
    public TMP_Text memoryTokenText;
    public TMP_Text hopeSparkText;   

    [Header("Memory Token Icons (optional gems)")]
    public Image[] memoryTokenIcons;

    [Header("Vignette")]
    [Tooltip("A full-screen black Image inside a CanvasGroup. Darkens as Mood drops.")]
    public CanvasGroup vignette;

    [Header("Game State Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Message Popup")]
    public GameObject popupPanel;
    public TMP_Text   popupText;
    public float      popupDuration = 3f;

    static readonly Color C_ENERGY_OK  = new Color(0.18f, 0.64f, 0.64f); // teal
    static readonly Color C_ENERGY_LOW = new Color(0.88f, 0.29f, 0.29f); // red

    void Start()
    {
        if (Level2Manager.Instance != null)
        {
            if (moodBar   != null) { moodBar.minValue = 0;   moodBar.maxValue = Level2Manager.Instance.MaxMood; }
            if (energyBar != null) { energyBar.minValue = 0; energyBar.maxValue = Level2Manager.Instance.MaxEnergy; }
        }

        if (winPanel   != null) winPanel.SetActive(false);
        if (losePanel  != null) losePanel.SetActive(false);
        if (popupPanel != null) popupPanel.SetActive(false);
    }

    void Update()
    {
        if (Level2Manager.Instance == null) return;
        var gm = Level2Manager.Instance;

        if (moodBar   != null) moodBar.value   = gm.CurrentMood;
        if (energyBar != null) energyBar.value = gm.Energy;

        if (memoryTokenText != null)
            memoryTokenText.text = "Memories: " + gm.memoryTokensCollected + " / " + gm.totalMemoryTokens;
        if (hopeSparkText != null)
            hopeSparkText.text = "Hope: x" + gm.hopeSparkCount;

        UpdateEnergyColor(gm.Energy);
        UpdateMemoryIcons(gm.memoryTokensCollected);
        UpdateVignette(gm.CurrentMood, gm.MaxMood);
        HandleEndScreens(gm);
    }

    void UpdateEnergyColor(float energy)
    {
        if (energyBarFill == null) return;
        energyBarFill.color = (energy < 20f) ? C_ENERGY_LOW : C_ENERGY_OK;
    }

    void UpdateMemoryIcons(int collected)
    {
        if (memoryTokenIcons == null) return;
        for (int i = 0; i < memoryTokenIcons.Length; i++)
        {
            if (memoryTokenIcons[i] != null)
                memoryTokenIcons[i].enabled = (i < collected);
        }
    }

    void UpdateVignette(float mood, float maxMood)
    {
        if (vignette == null) return;
        // alpha = Lerp(0.85, 0, Mood/100) — darker when Mood is low
        vignette.alpha = Mathf.Lerp(0.85f, 0f, mood / maxMood);
    }

    void HandleEndScreens(Level2Manager gm)
    {
        if (!gm.IsGameOver) return;
        if (winPanel  != null) winPanel.SetActive( gm.IsWin);
        if (losePanel != null) losePanel.SetActive(!gm.IsWin);
    }

    // Hook this to the Hope Spark button
    public void OnHopeSparkButton()
    {
        if (Level2Manager.Instance != null)
            Level2Manager.Instance.TryUseHopeSpark();
    }

    // Popup (memory messages / "find all memories first")
    public void ShowPopup(string message)
    {
        if (popupPanel == null || popupText == null) return;
        StopAllCoroutines();
        popupText.text = message;
        popupPanel.SetActive(true);
        StartCoroutine(HidePopup());
    }

    private IEnumerator HidePopup()
    {
        yield return new WaitForSeconds(popupDuration);
        if (popupPanel != null) popupPanel.SetActive(false);
    }
}