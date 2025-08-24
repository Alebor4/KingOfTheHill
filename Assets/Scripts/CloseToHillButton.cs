using UnityEngine;
using UnityEngine.SceneManagement;

// Небольшой helper для кнопки Close на сцене уровней.
// Загружает сцену холма по имени; если оставить пустым — попытается взять из GameManager.hillSceneName.
public class CloseToHillButton : MonoBehaviour
{
    [Tooltip("Имя сцены холма (например HillScene_01). Если пусто, будет использовано GameManager.hillSceneName (если есть).")]
    public string hillSceneName = "";

    public void ClosePressed()
    {
        string sceneToLoad = hillSceneName;

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            var gm = FindObjectOfType<GameManager>();
            if (gm != null)
                sceneToLoad = gm.hillSceneName;
        }

        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("CloseToHillButton: hill scene name is empty. Set hillSceneName or GameManager.hillSceneName.");
            return;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}
