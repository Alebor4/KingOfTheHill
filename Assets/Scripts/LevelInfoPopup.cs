using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelInfoPopup : MonoBehaviour
{
    public enum LevelMode { Moves = 1, Timer = 2, TimerWithBonus = 3 }

    [Serializable]
    public struct LevelConfig
    {
        [Range(1, 99)] public int levelNumber;   // 1..N
        public string displayTitle;
        public LevelMode mode;

        [Header("Параметры режима")]
        public int moves;               // для Moves
        public float timeSeconds;         // для Timer/TimerWithBonus
        public float bonusSecondsPerTile; // для TimerWithBonus

        [Header("Сцена уровня")]
        public string sceneName;          // обычно "LevelScene_01"
    }

    [Header("UI ссылки")]
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    [Header("Конфигурация уровней")]
    [SerializeField] private LevelConfig[] levels = new LevelConfig[3];

    private LevelConfig _current;

    private const string PP_SelectedLevel = "KOTH_SelectedLevel";
    private const string PP_SelectedMode = "KOTH_SelectedMode";     // 1/2/3
    private const string PP_TimeSeconds = "KOTH_TimeSeconds";
    private const string PP_Moves = "KOTH_Moves";
    private const string PP_BonusSec = "KOTH_BonusSec";

    private void Awake()
    {
        if (root == null) root = gameObject;

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => SetActive(false));
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }

        SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Автофикс: если какие-то levelNumber равны 0 — назначаем 1..N
        if (levels != null)
        {
            for (int i = 0; i < levels.Length; i++)
                if (levels[i].levelNumber <= 0)
                    levels[i].levelNumber = i + 1;
        }
    }
#endif

    private static string BestKey(int levelNumber) => $"KOTH_Best_Level_{levelNumber}";

    public void ShowForLevel(int levelNumber)
    {
        if (!TryGetConfig(levelNumber, out _current))
        {
            // Подстраховка: если ровно не нашли — пробуем взять по индексу (levelNumber-1)
            if (levels != null && levels.Length >= levelNumber && levelNumber > 0)
            {
                _current = levels[levelNumber - 1];

                // Если у элемента нулевой/неверный номер — поправим на лету
                if (_current.levelNumber != levelNumber)
                {
                    _current.levelNumber = levelNumber;
                    levels[levelNumber - 1] = _current; // записали обратно в массив
                }
            }
            else
            {
                Debug.LogError($"[LevelInfoPopup] Нет конфига для уровня {levelNumber}. " +
                               $"Заполните массив Levels и поставьте правильные levelNumber.");
                return;
            }
        }

        // Заголовок
        if (titleText) titleText.text = string.IsNullOrWhiteSpace(_current.displayTitle)
            ? $"Уровень {_current.levelNumber}"
            : _current.displayTitle;

        // Описание
        if (descText) descText.text = BuildDescription(_current);

        // Личный лучший
        int best = PlayerPrefs.GetInt(BestKey(_current.levelNumber), 0);
        if (bestText) bestText.text = $"Личный рекорд: {best}";

        SetActive(true);
    }

    private string BuildDescription(LevelConfig cfg)
    {
        switch (cfg.mode)
        {
            case LevelMode.Moves:
                return $"Набери максимум очков за {cfg.moves} ходов.";
            case LevelMode.Timer:
                return $"Набери максимум очков за {Mathf.CeilToInt(cfg.timeSeconds)} секунд.";
            case LevelMode.TimerWithBonus:
                return $"Набери максимум очков за {Mathf.CeilToInt(cfg.timeSeconds)} секунд.\n" +
                       $"Бонус-тайлы в цепочке добавляют по {cfg.bonusSecondsPerTile:0.#} с.";
            default:
                return "Режим не задан.";
        }
    }

    private void OnPlayClicked()
    {
        // Сохраняем выбранные параметры для LevelScene_01
        PlayerPrefs.SetInt(PP_SelectedLevel, _current.levelNumber);
        PlayerPrefs.SetInt(PP_SelectedMode, (int)_current.mode);
        PlayerPrefs.SetInt(PP_Moves, Mathf.Max(0, _current.moves));
        PlayerPrefs.SetFloat(PP_TimeSeconds, Mathf.Max(0f, _current.timeSeconds));
        PlayerPrefs.SetFloat(PP_BonusSec, Mathf.Max(0f, _current.bonusSecondsPerTile));
        PlayerPrefs.Save();

        if (string.IsNullOrEmpty(_current.sceneName))
        {
            Debug.LogError("[LevelInfoPopup] Не задано имя сцены уровня.");
            return;
        }

        SceneManager.LoadScene(_current.sceneName);
    }

    private bool TryGetConfig(int levelNumber, out LevelConfig cfg)
    {
        if (levels != null)
        {
            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].levelNumber == levelNumber)
                {
                    cfg = levels[i];
                    return true;
                }
            }
        }

        cfg = default;
        return false;
    }

    private void SetActive(bool on)
    {
        if (root != null) root.SetActive(on);
    }
}
