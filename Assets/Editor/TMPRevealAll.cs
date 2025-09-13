#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class TMPRevealAll
{
    [MenuItem("KOTH/Fix TMP: Reveal ALL (center & reset)")]
    public static void RevealAll()
    {
        // Попробуем взять дефолтный шрифт TMP
        TMP_FontAsset fallback =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (!fallback)
            fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Packages/com.unity.textmeshpro/Fonts & Materials/LiberationSans SDF.asset");

        int shown = 0;
        foreach (var tmp in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            Undo.RecordObject(tmp, "Reveal TMP");
            tmp.gameObject.SetActive(true);

            // Шрифт и цвет
            if (tmp.font == null && fallback) tmp.font = fallback;
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 72;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;

            // Рамка и положение — в центр экрана
            var rt = tmp.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            var sd = rt.sizeDelta;
            if (sd.x < 300) sd.x = 600;
            if (sd.y < 50) sd.y = 80;
            rt.sizeDelta = sd;

            EditorUtility.SetDirty(tmp);
            EditorUtility.SetDirty(rt);
            shown++;
        }

        Debug.Log($"[KOTH] TMP revealed: {shown}");
    }
}
#endif
