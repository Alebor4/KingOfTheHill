// GMFinder.cs
// Помогает найти все экземпляры GameManager в сцене и принудительно вызвать FinishRun() на первом найденном.
// Используй ContextMenu -> ListAndForceFinish в инпекторе этого компонента.
using UnityEngine;

public class GMFinder : MonoBehaviour
{
    [ContextMenu("ListAndForceFinish")]
    public void ListAndForceFinish()
    {
        var gms = FindObjectsOfType<GameManager>();
        Debug.Log($"GMFinder: found {gms.Length} GameManager instance(s) in scene.");
        for (int i = 0; i < gms.Length; i++)
        {
            Debug.Log($"GMFinder: [{i}] GameObject='{gms[i].gameObject.name}'");
        }

        if (gms.Length == 0)
        {
            Debug.LogWarning("GMFinder: no GameManager found. Make sure GameManager is present in the scene.");
            return;
        }

        // Попробуем вызвать FinishRun() на первом найденном GameManager
        try
        {
            Debug.Log("GMFinder: calling FinishRun() on first GameManager.");
            gms[0].FinishRun();
            Debug.Log("GMFinder: FinishRun() call complete.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("GMFinder: FinishRun() call failed: " + ex);
        }
    }
}
