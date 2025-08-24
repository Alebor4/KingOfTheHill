using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Простейший контроллер для переключения двух кнопок (Next / ReplayWholeRun),
/// и удобных публичных методов для OnClick (Restart/Next/Replay/Close).
/// </summary>
public class ResultButtonsController : MonoBehaviour
{
    [Header("UI references")]
    public GameObject resultPanel;                 // панель результатов (обязательно)
    public Button restartButton;
    public Button nextLevelButton;                 // видна по-умолчанию
    public Button replayWholeRunButton;            // скрыта по-умолчанию
    public Button closeButton;                     // вернуть на холм
    public GameObject movesGroup;                  // опционально, группу шагов/таймеров скрывать/показывать

    void Awake()
    {
        // небольшая защита: убедиться что кнопки привязаны
        if (resultPanel == null) Debug.LogWarning("ResultButtonsController: resultPanel не привязан");
        if (nextLevelButton == null) Debug.LogWarning("ResultButtonsController: nextLevelButton не привязан");
        if (replayWholeRunButton == null) Debug.LogWarning("ResultButtonsController: replayWholeRunButton не привязан");
    }

    /// <summary>
    /// Показывает Next (и прячет ReplayWholeRun).
    /// Вызвать, когда есть следующий уровень.
    /// </summary>
    public void ShowNext()
    {
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(true);
        if (replayWholeRunButton != null) replayWholeRunButton.gameObject.SetActive(false);
        BlockGameplayClicks(true);
    }

    /// <summary>
    /// Показывает ReplayWholeRun (и прячет Next).
    /// Вызвать, когда это финальный уровень.
    /// </summary>
    public void ShowReplayWholeRun()
    {
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(false);
        if (replayWholeRunButton != null) replayWholeRunButton.gameObject.SetActive(true);
        BlockGameplayClicks(true);
    }

    /// <summary>
    /// Скрыть панель результатов и разблокировать клики.
    /// </summary>
    public void HideResultPanel()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
        BlockGameplayClicks(false);
    }

    /// <summary>
    /// Блокирует клики по игровым объектам — при показе панели.
    /// Делает это, включив/выключив весь movesGroup (если указан)
    /// и оставляя resultPanel активной (предполагается, что resultPanel содержит Image с RaycastTarget=true).
    /// </summary>
    void BlockGameplayClicks(bool block)
    {
        if (movesGroup != null)
            movesGroup.SetActive(!block);
        // убедись, что resultPanel имеет Image (может быть прозрачный) с Raycast Target = true
    }

    // --- методы для привязки к кнопкам OnClick в инспекторе ---
    public void OnRestartPressed()
    {
        UnityEngine.Debug.Log("ResultButtonsController: Restart pressed");
        // вызываем GameManager.RestartLevel(), если он есть
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.RestartLevel();
    }

    public void OnNextPressed()
    {
        UnityEngine.Debug.Log("ResultButtonsController: Next pressed");
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.NextLevel();
    }

    public void OnReplayWholeRunPressed()
    {
        UnityEngine.Debug.Log("ResultButtonsController: ReplayWholeRun pressed");
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.ReplayWholeRun();
    }

    public void OnClosePressed()
    {
        UnityEngine.Debug.Log("ResultButtonsController: Close pressed");
        var gm = FindObjectOfType<GameManager>();
        if (gm != null) gm.CloseToHill();
    }
}
