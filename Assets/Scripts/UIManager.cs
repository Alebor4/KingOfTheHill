using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("HUD")]
    public Text scoreText;
    public Text timeText;
    public GameObject timeGroup;
    public Text movesText;
    public GameObject movesGroup;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public Text resultLevelScoreText;
    public Text resultRunTotalText;
    public Button restartButton;
    public Button nextLevelButton;
    public Button replayWholeRunButton;
    public Button closeButton;

    [Header("Final")]
    public GameObject finalPanel;
    public Text finalTotalText;
    public Text finalTopText;

    void Start()
    {
        HideResult();
        HideFinal();
    }

    public void SetScore(float score)
    {
        if (scoreText != null) scoreText.text = $"{score:F2}";
    }

    public void ShowTimeMode()
    {
        if (timeGroup != null) timeGroup.SetActive(true);
        if (movesGroup != null) movesGroup.SetActive(false);
    }

    public void ShowMovesMode()
    {
        if (timeGroup != null) timeGroup.SetActive(false);
        if (movesGroup != null) movesGroup.SetActive(true);
    }

    public void SetTime(float seconds)
    {
        if (timeText != null) timeText.text = $"{seconds:F1}s";
    }

    public void SetMoves(int moves)
    {
        if (movesText != null) movesText.text = moves.ToString();
    }

    public void HideResult()
    {
        if (resultPanel != null) resultPanel.SetActive(false);
    }

    // show result after a level: levelScore, runTotal, isFinal
    public void ShowResult(float levelScore, float runTotal, bool isFinal)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);
        if (resultLevelScoreText != null) resultLevelScoreText.text = $"Level: {levelScore:F2}";
        if (resultRunTotalText != null) resultRunTotalText.text = $"Total: {runTotal:F2}";

        // button visibility logic: if final -> show ReplayWholeRun, otherwise Next
        if (replayWholeRunButton != null) replayWholeRunButton.gameObject.SetActive(isFinal);
        if (nextLevelButton != null) nextLevelButton.gameObject.SetActive(!isFinal);
        // Restart always available
        if (restartButton != null) restartButton.gameObject.SetActive(true);
    }

    public void ShowFinalResult(float finalTotal, float top)
    {
        if (finalPanel == null) return;
        finalPanel.SetActive(true);
        if (finalTotalText != null) finalTotalText.text = $"You: {finalTotal:F2}";
        if (finalTopText != null) finalTopText.text = top > -0.5f ? $"Top: {top:F2}" : "Top: —";
    }

    public void HideFinal()
    {
        if (finalPanel != null) finalPanel.SetActive(false);
    }
}
