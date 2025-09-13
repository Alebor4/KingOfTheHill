#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using TMPro;

public static class TMPResetMaterial
{
    [MenuItem("KOTH/Fix TMP: Reset Materials (all)")]
    public static void ResetAll()
    {
        // Берём стандартный шрифт TMP как запасной
        TMP_FontAsset fallback =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (!fallback)
            fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Packages/com.unity.textmeshpro/Fonts & Materials/LiberationSans SDF.asset");

        int fixedCount = 0;

        foreach (var tmp in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            Undo.RecordObject(tmp, "Reset TMP Material");

            // Если шрифт сломан — подставим рабочий
            if (tmp.font == null && fallback != null)
                tmp.font = fallback;

            // Сброс материалов к дефолтному материалу шрифта
            if (tmp.font != null)
            {
                var defaultMat = tmp.font.material;      // материал шрифта по умолчанию
                tmp.fontSharedMaterial = defaultMat;     // общий материал
                tmp.fontMaterial = defaultMat;     // инстанс-материал
            }

            // Цвета/альфа — в видимые значения
            tmp.color = Color.white;
            tmp.enableAutoSizing = true;
            tmp.overflowMode = TextOverflowModes.Overflow;
            tmp.richText = true;

            // На всякий случай «выпрямим» FaceColor в материале шрифта
            var mat = tmp.fontSharedMaterial;
            if (mat != null && mat.HasProperty(TMPro.ShaderUtilities.ID_FaceColor))
            {
                mat.SetColor(TMPro.ShaderUtilities.ID_FaceColor, Color.white);
                mat.SetFloat(TMPro.ShaderUtilities.ID_FaceDilate, 0f);
                EditorUtility.SetDirty(mat);
            }

            // Форсируем перестройку меша/графики
            tmp.SetAllDirty();
            tmp.ForceMeshUpdate(true, true);

            EditorUtility.SetDirty(tmp);
            fixedCount++;
        }

        Debug.Log($"[KOTH] TMP materials reset on {fixedCount} texts.");
    }
}
#endif
