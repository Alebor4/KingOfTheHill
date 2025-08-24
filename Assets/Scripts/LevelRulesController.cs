using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelRulesController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject movesGroup; // вся группа с худами ходов
    [SerializeField] private TMP_Text movesText;   // число ходов
    [SerializeField] private TMP_Text timerText;   // число секунд
    [SerializeField] private TMP_Text scoreText;   // опционально: "Score: 0"

    [Header("Запасные дефолты, если PlayerPrefs пуст")]
    [SerializeField] private int fallbackMovesLevel1 = 5;    // режим 1
    [SerializeField] private float fallbackTimeLevel2 = 10f;  // режим 2
    [SerializeField] private float fallbackTimeLevel3 = 20f;  // режим 3
    [SerializeField] private float fallbackBonusSec = 2f;   // для режима 3

    [Header("Сцена холма для возврата")]
    [SerializeField] private string hillSceneName = "HillScene_01";

    // выбранные параметры
    private int _selectedLevel;      // 1/2/3
    private int _mode;               // 1=Moves, 2=Timer, 3=TimerWithBonus
    private bool _useTimer;
    private float _timeLeft;
    private int _movesLeft;
    private float _bonusPerTile;

    public static LevelRulesController Instance { get; private set; }

    // Ключи PlayerPrefs (совместимы с LevelInfoPopup)
    private const string PP_SelectedLevel = "KOTH_SelectedLevel";
    private const string PP_SelectedMode = "KOTH_SelectedMode";
    private const string PP_TimeSeconds = "KOTH_TimeSeconds";
    private const string PP_Moves = "KOTH_Moves";
    private const string PP_BonusSec = "KOTH_BonusSec";

    private void Awake()
    {
        Instance = this;

        _selectedLevel = Mathf.Clamp(PlayerPrefs.GetInt(PP_SelectedLevel, 1), 1, 3);
        _mode = PlayerPrefs.GetInt(PP_SelectedMode, ModeFromLevel(_selectedLevel));

        switch (_mode)
        {
            case 1: // Moves
                _useTimer = false;
                _movesLeft = PlayerPrefs.GetInt(PP_Moves, fallbackMovesLevel1);
                break;

            case 2: // Timer
                _useTimer = true;
                _timeLeft = GetTimeOrFallback(_selectedLevel);
                break;

            case 3: // Timer + Bonus
                _useTimer = true;
                _timeLeft = GetTimeOrFallback(_selectedLevel);
                _bonusPerTile = PlayerPrefs.GetFloat(PP_BonusSec, fallbackBonusSec);
                break;

            default:
                _useTimer = false;
                _movesLeft = fallbackMovesLevel1;
                break;
        }

        UpdateUIVisibility();
        RefreshUI();
    }

    private void Update()
    {
        if (_useTimer && _timeLeft > 0f)
        {
            _timeLeft -= Time.deltaTime;
            if (_timeLeft < 0f) _timeLeft = 0f;
            RefreshUI();

            if (_timeLeft <= 0f)
                EndLevel();
        }
    }

    // === Публичные вызовы для твоего геймплея ===

    /// Вызывай каждый раз, когда игрок совершил ход (режим 1: Moves).
    public void NotifyMoveUsed()
    {
        if (_mode != 1 || _movesLeft <= 0) return;

        _movesLeft--;
        RefreshUI();

        if (_movesLeft <= 0)
            EndLevel();
    }

    /// Вызывай, если цепочка включала бонус-тайлы (режим 3).
    /// bonusTilesInChain — сколько бонусных тайлов было в собранной цепочке.
    public void NotifyBonusTilesConsumed(int bonusTilesInChain)
    {
        if (_mode != 3 || bonusTilesInChain <= 0) return;

        _timeLeft += _bonusPerTile * bonusTilesInChain;
        RefreshUI();
    }

    // === Вспомогательное ===

    private void UpdateUIVisibility()
    {
        // Показываем ИЛИ movesGroup ИЛИ timerText
        bool showMoves = (_mode == 1);
        if (movesGroup) movesGroup.SetActive(showMoves);
        if (timerText) timerText.gameObject.SetActive(!showMoves);
    }

    private void RefreshUI()
    {
        if (movesText && _mode == 1)
            movesText.text = _movesLeft.ToString();

        if (timerText && _useTimer)
            timerText.text = Mathf.CeilToInt(_timeLeft).ToString();

        if (scoreText)
            scoreText.text = "Score: 0"; // здесь позже подставим реальный счёт
    }

    private void EndLevel()
    {
        // сюда позже добавим подсчёт очков и сохранение лучшего результата
        if (!string.IsNullOrEmpty(hillSceneName))
            SceneManager.LoadScene(hillSceneName);
        else
            Debug.LogWarning("[LevelRulesController] hillSceneName пуст — укажи имя сцены холма.");
    }

    private float GetTimeOrFallback(int level)
    {
        float t = PlayerPrefs.GetFloat(PP_TimeSeconds, -1f);
        if (t >= 0f) return t;

        // fallback по номеру уровня
        if (level == 2) return fallbackTimeLevel2;
        if (level == 3) return fallbackTimeLevel3;
        return 0f;
    }

    private int ModeFromLevel(int level)
    {
        if (level == 1) return 1;
        if (level == 2) return 2;
        if (level == 3) return 3;
        return 1;
    }
}
