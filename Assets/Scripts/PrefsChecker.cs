using UnityEngine;
using UnityEngine.UI;

public class PrefsChecker : MonoBehaviour
{
    public Text topText;
    public Text personalText;

    const string PREF_SELECTED_HILL = "selectedHillId";
    const string KEY_TOP_PREFIX = "top_";
    const string KEY_PERSONAL_PREFIX = "personal_";

    void Start()
    {
        RefreshHillUI();
    }

    public void RefreshHillUI()
    {
        string hillId = PlayerPrefs.GetString(PREF_SELECTED_HILL, "hill_default");
        if (string.IsNullOrEmpty(hillId)) hillId = "hill_default";

        string topKey = KEY_TOP_PREFIX + hillId;
        string personalKey = KEY_PERSONAL_PREFIX + hillId;

        float top = PlayerPrefs.GetFloat(topKey, -1f);
        float personal = PlayerPrefs.GetFloat(personalKey, -1f);

        if (topText != null) topText.text = top > -0.5f ? $"Top: {top:F2}" : "Top: —";
        if (personalText != null) personalText.text = personal > -0.5f ? $"You: {personal:F2}" : "You: —";

        Debug.Log($"PrefsChecker: selectedHillId = '{hillId}' top={top} personal={personal}");
    }
}
