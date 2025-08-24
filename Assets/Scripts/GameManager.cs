using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Refs (Inspector)")]
    public GridManager gridManager;
    public UIManager uiManager;
    public ScoreManager scoreManager;
    public MonoBehaviour inputManager; // assign your InputManager component if any

    [Header("Level data (assign LevelData ScriptableObject)")]
    public LevelData levelData;
    public int startLevelIndex = 0;

    [Header("Scene names")]
    public string hillSceneName = "HillScene_01";

    // runtime
    private int currentLevelIndex = 0;
    private bool levelActive = false;
    private int movesRemaining = 0;
    private float timeRemaining = 0f;
    private float levelTotalTime = 0f;
    private float thisLevelScore = 0f;
    private float runTotalScore = 0f;

    // guard
    private bool finishingInProgress = false;

    const string PREF_SELECTED_HILL = "selectedHillId";
    const string KEY_TOP_PREFIX = "top_";
    const string KEY_PERSONAL_PREFIX = "personal_";

    void Awake()
    {
        if (gridManager == null) gridManager = FindObjectOfType<GridManager>();
        if (uiManager == null) uiManager = FindObjectOfType<UIManager>();
        if (scoreManager == null) scoreManager = FindObjectOfType<ScoreManager>();
        if (inputManager == null) inputManager = FindObjectOfType<InputManager>() as MonoBehaviour;

        // try to inform grid about GM if it has field
        if (gridManager != null)
        {
            try { gridManager.gameManager = this; } catch { }
        }
    }

    void Start()
    {
        if (levelData != null && levelData.levels != null && levelData.levels.Count > 0)
            StartRun(startLevelIndex);
    }

    public void StartRun(int fromLevel = 0)
    {
        if (levelData == null || levelData.levels == null || levelData.levels.Count == 0)
        {
            Debug.LogError("GameManager.StartRun: levelData not set or empty");
            return;
        }

        runTotalScore = 0f;
        currentLevelIndex = Mathf.Clamp(fromLevel, 0, levelData.levels.Count - 1);
        StartLevel(currentLevelIndex);
    }

    public void StartLevel(int index)
    {
        if (levelData == null) { Debug.LogError("StartLevel: levelData null"); return; }
        if (index < 0 || index >= levelData.levels.Count) { Debug.LogError("StartLevel: index out of range"); return; }

        finishingInProgress = false;
        currentLevelIndex = index;
        thisLevelScore = 0f;
        levelActive = true;

        var entry = levelData.levels[currentLevelIndex];

        if (entry.mode == LevelData.LevelMode.Time)
        {
            levelTotalTime = Mathf.Max(1f, entry.timeLimitSeconds);
            timeRemaining = levelTotalTime;
            movesRemaining = 0;
            uiManager?.ShowTimeMode();
            uiManager?.SetTime(timeRemaining);
        }
        else // Moves
        {
            movesRemaining = Mathf.Max(1, entry.moveLimit);
            timeRemaining = 0f;
            levelTotalTime = 0f;
            uiManager?.ShowMovesMode();
            uiManager?.SetMoves(movesRemaining);
        }

        uiManager?.SetScore(runTotalScore);
        uiManager?.HideResult();

        if (gridManager != null)
        {
            try { gridManager.InitGrid(); }
            catch { if (gridManager != null) { Debug.Log("GridManager.InitGrid failed or not found."); } }
        }

        if (inputManager != null) inputManager.enabled = true;

        Debug.Log($"StartLevel idx={currentLevelIndex} mode={entry.mode} time={timeRemaining} moves={movesRemaining}");
    }

    void Update()
    {
        if (!levelActive) return;
        if (levelData == null) return;

        var entry = levelData.levels[currentLevelIndex];
        if (entry.mode == LevelData.LevelMode.Time)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f) timeRemaining = 0f;
            uiManager?.SetTime(timeRemaining);
            if (timeRemaining <= 0f && !finishingInProgress)
            {
                StartCoroutine(DelayedFinishLevel());
            }
        }
    }

    IEnumerator DelayedFinishLevel()
    {
        finishingInProgress = true;
        yield return new WaitForSeconds(0.12f);
        FinishCurrentLevel();
    }

    // GridManager должен вызывать этот метод: gameManager.OnChainProcessed(chainLength, wasPlayerMove)
    public void OnChainProcessed(int chainLength, bool wasPlayerMove)
    {
        if (!levelActive) return;
        if (chainLength <= 0) return;

        float gained = 0f;
        if (scoreManager != null)
        {
            try { gained = scoreManager.CalculateScoreForChain(chainLength, timeRemaining, levelTotalTime); }
            catch { gained = chainLength * 10f; }
        }
        else gained = chainLength * 10f;

        thisLevelScore += gained;
        runTotalScore += gained;
        uiManager?.SetScore(runTotalScore);

        // reduce moves only for player moves and only in Moves mode
        if (!IsCurrentLevelTimeMode() && wasPlayerMove)
        {
            movesRemaining = Mathf.Max(0, movesRemaining - 1);
            uiManager?.SetMoves(movesRemaining);
            if (movesRemaining <= 0 && !finishingInProgress) StartCoroutine(DelayedFinishLevel());
        }

        // time bonus example
        if (IsCurrentLevelTimeMode() && chainLength >= 5)
        {
            float bonus = Mathf.Clamp(chainLength - 4, 1f, 5f);
            timeRemaining += bonus;
            uiManager?.SetTime(timeRemaining);
        }
    }

    // overload
    public void OnChainProcessed(int chainLength) { OnChainProcessed(chainLength, true); }

    void FinishCurrentLevel()
    {
        if (!levelActive) return;
        levelActive = false;
        finishingInProgress = false;

        Debug.Log($"FinishCurrentLevel idx={currentLevelIndex} levelScore={thisLevelScore} runTotal={runTotalScore}");
        bool isFinal = (currentLevelIndex == levelData.levels.Count - 1);

        uiManager?.ShowResult(thisLevelScore, runTotalScore, isFinal);

        if (inputManager != null) inputManager.enabled = false;
    }

    // UI buttons (bind to these methods in inspector)
    public void RestartLevel()
    {
        Debug.Log("RestartLevel button");
        StartLevel(currentLevelIndex);
    }

    public void NextLevel()
    {
        Debug.Log("NextLevel button");
        int next = currentLevelIndex + 1;
        if (next < levelData.levels.Count) StartLevel(next);
        else FinishRun();
    }

    public void ReplayWholeRun()
    {
        Debug.Log("ReplayWholeRun button");
        StartRun(startLevelIndex);
    }

    public void CloseToHill()
    {
        Debug.Log("CloseToHill button -> " + hillSceneName);
        if (!string.IsNullOrEmpty(hillSceneName)) SceneManager.LoadScene(hillSceneName);
    }

    // Finish whole run and save
    public void FinishRun()
    {
        Debug.Log("FinishRun: saving scores");
        float finalTotal = runTotalScore;

        string hillId = PlayerPrefs.GetString(PREF_SELECTED_HILL, "hill_default");
        if (string.IsNullOrEmpty(hillId)) hillId = "hill_default";

        string topKey = KEY_TOP_PREFIX + hillId;
        string personalKey = KEY_PERSONAL_PREFIX + hillId;

        float prevTop = PlayerPrefs.GetFloat(topKey, -1f);
        PlayerPrefs.SetFloat(personalKey, finalTotal);

        if (finalTotal > prevTop)
        {
            PlayerPrefs.SetFloat(topKey, finalTotal);
            Debug.Log($"New top for {hillId} = {finalTotal}");
        }
        PlayerPrefs.Save();

        uiManager?.ShowFinalResult(finalTotal, PlayerPrefs.GetFloat(topKey, -1f));
    }

    bool IsCurrentLevelTimeMode()
    {
        if (levelData == null) return false;
        return levelData.levels[currentLevelIndex].mode == LevelData.LevelMode.Time;
    }
}
