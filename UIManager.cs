using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    // Inspector references 
    [Header("Meters")]
    public Slider stressBar;
    public Slider confidenceBar;

    [Header("Stress Bar Fill")]
    public Image stressBarFill; 

    [Header("Card Count")]
    public TMP_Text positiveCardText;

    [Header("Game State Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Positive Card Popup")]
    public GameObject cardPopupPanel;
    public TMP_Text   cardPopupText;
    public float      popupDuration = 3f;

    //  Stress colors 
    static readonly Color C_SAFE   = new Color(0.388f, 0.600f, 0.133f); // #639922
    static readonly Color C_WARN   = new Color(0.937f, 0.623f, 0.153f); // #EF9F27
    static readonly Color C_DANGER = new Color(0.886f, 0.294f, 0.290f); // #E24B4A

    //  Unity lifecycle 
    void Start()
    {
        if (GameManager.Instance != null)
        {
            stressBar.minValue     = 0;
            stressBar.maxValue     = GameManager.Instance.MaxStress;
            confidenceBar.minValue = 0;
            confidenceBar.maxValue = GameManager.Instance.MaxStress; 
        }

        if (winPanel       != null) winPanel.SetActive(false);
        if (losePanel      != null) losePanel.SetActive(false);
        if (cardPopupPanel != null) cardPopupPanel.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        float s = GameManager.Instance.Stress;
        float c = GameManager.Instance.Confidence;

        stressBar.value       = s;
        confidenceBar.value   = c;
        positiveCardText.text = "Cards: " + GameManager.Instance.PositiveCardCount;

        UpdateStressColor(s);
        HandleEndScreens();
    }

    // Stress bar color changes dynamically 
    void UpdateStressColor(float s)
    {
        if (stressBarFill == null) return;
        if      (s >= 70) stressBarFill.color = C_DANGER; 
        else if (s >= 40) stressBarFill.color = C_WARN;   
        else              stressBarFill.color = C_SAFE;    
    }

    void HandleEndScreens()
    {
        if (!GameManager.Instance.IsGameOver) return;
        if (winPanel  != null) winPanel.SetActive( GameManager.Instance.IsWin);
        if (losePanel != null) losePanel.SetActive(!GameManager.Instance.IsWin);
    }

    // Button callbacks 
    public void OnUseCardButton()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.UsePositiveCard();
    }

    //  Card Popup 
    public void ShowCardPopup(string message)
    {
        if (cardPopupPanel == null || cardPopupText == null) return;
        StopAllCoroutines();
        cardPopupText.text = message;
        cardPopupPanel.SetActive(true);
        StartCoroutine(HideCardPopup());
    }

    private IEnumerator HideCardPopup()
    {
        yield return new WaitForSeconds(popupDuration);
        if (cardPopupPanel != null)
            cardPopupPanel.SetActive(false);
    }
}

