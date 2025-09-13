using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelInfoPopup : MonoBehaviour
{
    public enum LevelMode { Moves, Timer, TimerWithBonus }

    [System.Serializable]
    public class LevelInfo
    {
        [Min(1)] public int LevelNumber = 1;
        public LevelMode Mode = LevelMode.Moves;
        public int Moves = 5;
        public int TimeSeconds = 10;
        public int BonusSecondsPerTile = 2;
        public string SceneName = "LevelScene_01";
    }

    [Header("UI")]
    [SerializeField] private RectTransform root;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text bestText;
    [SerializeField] private Button playButton;
    [SerializeField] private Button closeButton;

    [Header("Config")]
    [SerializeField] private List<LevelInfo> levels = new List<LevelInfo>();

    private int _currentLevel = -1;

    private void Awake()
    {
        if (!root) root = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // стартуем скрытыми, и не перехватываем клики
        HideInstant();

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }
        if (playButton)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayPressed);
        }
    }

    public void ShowForLevel(int levelNumber)
    {
        _currentLevel = levelNumber;

        var data = levels.Find(l => l.LevelNumber == levelNumber);

        // Заголовок
        if (titleText) titleText.text = $"Уровень {levelNumber}";

        // Описание по правилу
        if (descText)
        {
            if (data != null)
            {
                switch (data.Mode)
                {
                    case LevelMode.Moves:
                        descText.text = $"Соберите максимум очков за <b>{data.Moves}</b> ходов";
                        break;
                    case LevelMode.Timer:
                        descText.text = $"Соберите максимум очков за <b>{data.TimeSeconds}</b> сек.";
                        break;
                    case LevelMode.TimerWithBonus:
                        descText.text = $"Соберите максимум очков за <b>{data.TimeSeconds}</b> сек. " +
                                        $"Бонус: +{data.BonusSecondsPerTile} сек за бонусный тайл в цепочке";
                        break;
                }
            }
            else
            {
                descText.text = "Соберите максимум очков";
                Debug.LogWarning($"[LevelInfoPopup] Нет конфига для уровня {levelNumber}", this);
            }
        }

        // Лучший результат (если нет — «—»)
        if (bestText)
        {
            var key = $"best_l{levelNumber}";
            bestText.text = PlayerPrefs.HasKey(key) ? $"Best: {PlayerPrefs.GetInt(key)}" : "Best: —";
        }

        ShowInstant();
    }

    public void Hide() => HideInstant();

    private void OnPlayPressed()
    {
        if (_currentLevel < 1)
        {
            Debug.LogError("[LevelInfoPopup] Уровень не выбран перед запуском.", this);
            return;
        }

        var data = levels.Find(l => l.LevelNumber == _currentLevel);
        var scene = (data != null && !string.IsNullOrEmpty(data.SceneName)) ? data.SceneName : "LevelScene_01";

        // Можем сохранить выбранный уровень, если нужно
        PlayerPrefs.SetInt("selected_level", _currentLevel);
        PlayerPrefs.Save();

        SceneManager.LoadScene(scene);
    }

    private void ShowInstant()
    {
        if (root) root.gameObject.SetActive(true);
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideInstant()
    {
        if (root) root.gameObject.SetActive(true); // корень оставляем активным — управляем через CanvasGroup
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
