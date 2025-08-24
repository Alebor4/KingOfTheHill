// LevelButtonsController.cs
// Привязывается к ResultPanel (или любому удобному GameObject).
// В Inspector: перетащи RestartButton, NextButton и (опционально) сам ResultPanel.
// Скрипт автоматически подпишет кнопки на вызовы GameManager.RestartLevel / NextLevel.
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonsController : MonoBehaviour
{
    [Header("UI")]
    public Button RestartButton;
    public Button NextButton;
    public GameObject ResultPanel; // опционально — объект панели результатов, скроем после нажатия

    GameManager gm;

    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        if (gm == null)
            Debug.LogWarning("LevelButtonsController: GameManager not found in scene.");

        if (RestartButton != null)
        {
            RestartButton.onClick.RemoveAllListeners();
            RestartButton.onClick.AddListener(OnRestartPressed);
        }

        if (NextButton != null)
        {
            NextButton.onClick.RemoveAllListeners();
            NextButton.onClick.AddListener(OnNextPressed);
        }
    }

    void OnDestroy()
    {
        if (RestartButton != null) RestartButton.onClick.RemoveAllListeners();
        if (NextButton != null) NextButton.onClick.RemoveAllListeners();
    }

    // Вызывается при нажатии Restart: перезапускает текущий уровень
    public void OnRestartPressed()
    {
        if (gm != null)
        {
            gm.RestartLevel();
        }
        else
        {
            Debug.LogWarning("OnRestartPressed: GameManager not found.");
        }

        // прячем панель результатов (если указана)
        if (ResultPanel != null) ResultPanel.SetActive(false);
    }

    // Вызывается при нажатии Next: идет к следующему уровню (или завершает ран)
    public void OnNextPressed()
    {
        if (gm != null)
        {
            gm.NextLevel();
        }
        else
        {
            Debug.LogWarning("OnNextPressed: GameManager not found.");
        }

        if (ResultPanel != null) ResultPanel.SetActive(false);
    }
}
