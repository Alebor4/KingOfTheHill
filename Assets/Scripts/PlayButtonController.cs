using UnityEngine;
using UnityEngine.SceneManagement;

// На кнопку Play в сцене холма. 
// Если выставлен hillMenu (опционально) — возьмёт hillId из него, иначе используем hillIdField.
public class PlayButtonController : MonoBehaviour
{
    [Header("Target level scene to load")]
    public string levelSceneName = "LevelScene";

    [Header("Hill id (fallback)")]
    [Tooltip("Если у вас на сцене есть HillMenuController, можно оставить поле пустым и назначить hillMenu.")]
    public string hillIdField = "hill_1";

    [Header("Optional reference to HillMenuController on the same scene")]
    public HillMenuController hillMenu; // optional - если есть, берем hillId из него

    const string PREF_SELECTED_HILL = "selectedHillId";

    // Вызывать из Button OnClick -> PlayPressed
    public void PlayPressed()
    {
        string hillId = hillIdField;
        if (hillMenu != null)
        {
            if (!string.IsNullOrEmpty(hillMenu.hillId))
                hillId = hillMenu.hillId;
        }

        if (string.IsNullOrEmpty(hillId))
        {
            Debug.LogWarning("PlayButtonController: hillId is empty. Set hillIdField or assign HillMenuController.");
        }
        else
        {
            PlayerPrefs.SetString(PREF_SELECTED_HILL, hillId);
            PlayerPrefs.Save();
            Debug.Log($"PlayButtonController: saved selectedHillId = {hillId}");
        }

        if (string.IsNullOrEmpty(levelSceneName))
        {
            Debug.LogError("PlayButtonController: levelSceneName is empty. Set the LevelScene name in inspector.");
            return;
        }

        SceneManager.LoadScene(levelSceneName);
    }
}
