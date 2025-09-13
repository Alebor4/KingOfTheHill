#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class TMPHardReset
{
    [MenuItem("KOTH/Fix TMP: Hard Reset (Selected)")]
    public static void HardResetSelectedTMP()
    {
        var selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("[KOTH] Ничего не выбрано. Выдели в Hierarchy невидимые тексты и повтори.");
            return;
        }

        // Попробуем найти дефолтный шрифт
        TMP_FontAsset fallback =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (!fallback)
            fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Packages/com.unity.textmeshpro/Fonts & Materials/LiberationSans SDF.asset");

        int processed = 0, reset = 0;

        // Временный объект с “чистым” TMP
        var tempGO = new GameObject("__TMP__DEFAULT__");
        tempGO.hideFlags = HideFlags.HideAndDontSave;
        var tempTMP = tempGO.AddComponent<TextMeshProUGUI>();
        if (fallback) tempTMP.font = fallback;

        foreach (var go in selection)
        {
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (!tmp) continue;

            Undo.RecordObject(tmp, "TMP Hard Reset");

            // Копируем значения “чистого” TMP в выбранный (сохраняем сам компонент — ссылки не сломаются)
            if (UnityEditorInternal.ComponentUtility.CopyComponent(tempTMP))
            {
                UnityEditorInternal.ComponentUtility.PasteComponentValues(tmp);
                reset++;
            }

            // Подстрахуемся: шрифт/материал/цвет
            if (tmp.font == null && fallback) tmp.font = fallback;

            var mat = tmp.fontSharedMaterial;
            if (mat != null && mat.HasProperty(TMPro.ShaderUtilities.ID_FaceColor))
            {
                mat.SetColor(TMPro.ShaderUtilities.ID_FaceColor, Color.white);
                mat.SetFloat(TMPro.ShaderUtilities.ID_FaceDilate, 0f);
                EditorUtility.SetDirty(mat);
            }

            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 72;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.alignment = TextAlignmentOptions.Center;

            // Форсим перестройку
            tmp.SetAllDirty();
            tmp.ForceMeshUpdate(true, true);

            EditorUtility.SetDirty(tmp);
            processed++;
        }

        Object.DestroyImmediate(tempGO);
        Debug.Log($"[KOTH] TMP Hard Reset: processed {processed}, reset {reset}.");
    }
}
#endif
