using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform _rt;
    private Rect _lastSafe;
    private ScreenOrientation _lastOrientation;

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        Apply();
    }

    private void OnEnable() => Apply();

    private void Update()
    {
        // Переприменяем при смене ориентации/разрешения/экрана
        if (_lastSafe != Screen.safeArea || _lastOrientation != Screen.orientation)
            Apply();
    }

    private void Apply()
    {
        if (_rt == null) _rt = GetComponent<RectTransform>();

        _lastSafe = Screen.safeArea;
        _lastOrientation = Screen.orientation;

        var sa = Screen.safeArea;

        // Переводим safeArea в нормализованные якоря [0..1]
        Vector2 anchorMin = sa.position;
        Vector2 anchorMax = sa.position + sa.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        _rt.anchorMin = anchorMin;
        _rt.anchorMax = anchorMax;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }
}
