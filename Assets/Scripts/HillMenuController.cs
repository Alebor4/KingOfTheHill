// HillMenuController.cs
using UnityEngine;
using TMPro;

public class HillMenuController : MonoBehaviour
{
    [Header("Identity")]
    public string hillId = "hill_1";
    public string displayName = "Hill";

    [Header("UI (TMP)")]
    public TextMeshProUGUI HillNameText;
    public TextMeshProUGUI TopScoreText;
    public TextMeshProUGUI YourBestText;

    void Start()
    {
        RefreshScores();
        if (HillNameText != null) HillNameText.text = displayName;
    }

    // вызывается прямо из PrefsChecker.RefreshHillUI или при старте сцены
    public void RefreshScores()
    {
        if (string.IsNullOrEmpty(hillId))
        {
            Debug.LogWarning("HillMenuController.RefreshScores: hillId is empty");
            return;
        }

        float top = PlayerPrefs.GetFloat($"top_{hillId}_score", -1f);
        float personal = PlayerPrefs.GetFloat($"personal_{hillId}_score", -1f);

        if (TopScoreText != null)
            TopScoreText.text = top >= 0f ? $"Top: {top:F2}" : "Top: —";

        if (YourBestText != null)
            YourBestText.text = personal >= 0f ? $"Your best: {personal:F2}" : "Your best: —";

        Debug.Log($"HillMenuController.RefreshScores: hillId={hillId}, top={top}, personal={personal}");
    }
}
