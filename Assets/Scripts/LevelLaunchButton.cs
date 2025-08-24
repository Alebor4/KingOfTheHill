using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Button))]
public class LevelLaunchButton : MonoBehaviour
{
    [Header("Scene to load")]
    [SerializeField] private string sceneName = "LevelScene_01";

    [Header("Level number shown on this button (1..3)")]
    [SerializeField] private int levelNumber = 1;

    private void Awake()
    {
        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Launch);
    }

    private void Launch()
    {
        // Сохраняем выбранный уровень (если нужно использовать внутри сцены уровня)
        PlayerPrefs.SetInt("KOTH_SelectedLevel", Mathf.Clamp(levelNumber, 1, 3));
        PlayerPrefs.Save();

        // Загружаем сцену уровней
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[LevelLaunchButton] sceneName пуст. Укажи имя сцены уровня в инспекторе.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
