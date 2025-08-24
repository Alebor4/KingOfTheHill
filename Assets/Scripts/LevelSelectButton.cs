using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int levelNumber = 1;          // 1..3
    [SerializeField] private LevelInfoPopup popup;         // Ссылка на попап на сцене

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            if (popup != null) popup.ShowForLevel(levelNumber);
            else Debug.LogError("[LevelSelectButton] Не назначен popup.");
        });
    }
}
