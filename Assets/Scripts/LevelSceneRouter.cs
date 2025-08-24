using UnityEngine;

public class LevelSceneRouter : MonoBehaviour
{
    [Header("Корни контента трёх уровней в этой сцене")]
    [SerializeField] private GameObject level1Root;
    [SerializeField] private GameObject level2Root;
    [SerializeField] private GameObject level3Root;

    [Header("Если номер не найден в PlayerPrefs")]
    [SerializeField] private int defaultLevelNumber = 1;

    private void Awake()
    {
        int n = Mathf.Clamp(PlayerPrefs.GetInt("KOTH_SelectedLevel", defaultLevelNumber), 1, 3);
        ActivateLevel(n);
    }

    private void ActivateLevel(int n)
    {
        if (level1Root) level1Root.SetActive(n == 1);
        if (level2Root) level2Root.SetActive(n == 2);
        if (level3Root) level3Root.SetActive(n == 3);
    }
}
