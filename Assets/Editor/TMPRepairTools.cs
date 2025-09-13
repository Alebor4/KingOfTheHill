#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class TMPRepairTools
{
    [MenuItem("KOTH/Fix TMP: Restore Visibility")]
    public static void FixTmpVisibility()
    {
        int fixedTexts = 0, resizedRects = 0, assignedFonts = 0;

        // Попробуем найти стандартный шрифт TMP (если у элементов шрифт сломан/нулевой)
        TMP_FontAsset fallback = null;
        // В большинстве проектов путь такой:
        fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (!fallback)
            fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Packages/com.unity.textmeshpro/Fonts & Materials/LiberationSans SDF.asset");

        foreach (var tmp in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            Undo.RecordObject(tmp, "Fix TMP");

            // 1) Цвет и альфа
            var c = tmp.color;
            if (float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || float.IsNaN(c.a) || c.a < 0.99f)
            {
                tmp.color = Color.white;
                fixedTexts++;
            }

            // 2) Шрифт
            if (tmp.font == null && fallback != null)
            {
                tmp.font = fallback;
                assignedFonts++;
            }

            // 3) Авторазмер и переполнение
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 72;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.richText = true;

            EditorUtility.SetDirty(tmp);

            // 4) Приведём рамку текса к разумному размеру
            var rt = tmp.rectTransform;
            bool resized = false;

            if (float.IsNaN(rt.sizeDelta.x) || float.IsNaN(rt.sizeDelta.y))
            {
                rt.sizeDelta = new Vector2(400, 80);
                resized = true;
            }
            else
            {
                var sd = rt.sizeDelta;
                if (sd.x < 120) { sd.x = 300; resized = true; }
                if (sd.y < 30) { sd.y = 60; resized = true; }
                if (resized) rt.sizeDelta = sd;
            }

            if (resized) { resizedRects++; EditorUtility.SetDirty(rt); }
        }

        Debug.Log($"[KOTH TMP Repair] Texts fixed: {fixedTexts}, Rects resized: {resizedRects}, Fonts assigned: {assignedFonts}");
    }
}
#endif
