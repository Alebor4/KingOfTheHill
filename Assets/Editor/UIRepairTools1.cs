#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class UIRepairTools
{
    // Восстановить спрайты UISprite всем Image с пустым sprite
    [MenuItem("KOTH/UI: Restore Missing Sprites (UISprite)")]
    public static void RestoreMissingSprites()
    {
        Sprite uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (!uiSprite)
        {
            Debug.LogError("[KOTH] Builtin UISprite не найден.");
            return;
        }

        int fixedCount = 0;
        foreach (var img in Object.FindObjectsOfType<Image>(true))
        {
            if (!img) continue;
            Undo.RecordObject(img, "Restore Missing Sprite");

            if (img.sprite == null)
            {
                img.sprite = uiSprite;
                img.type = Image.Type.Sliced;

                var c = img.color;
                if (c.a <= 0f) { c.a = 1f; img.color = c; }

                img.SetAllDirty();
                EditorUtility.SetDirty(img);
                fixedCount++;
            }
        }

        Debug.Log($"[KOTH] Восстановлено спрайтов у Image: {fixedCount}");
    }

    // Нормализовать материалы: подбираем UI/Default или Sprites/Default (в зависимости от рендера)
    [MenuItem("KOTH/UI: Normalize Materials (safe)")]
    public static void NormalizeUIMaterialsSafe()
    {
        // 1) Пытаемся найти подходящий шейдер для UI-изображений
        Shader uiShader = Shader.Find("UI/Default");
        if (uiShader == null)
        {
            // В URP иногда удобнее "Sprites/Default"
            uiShader = Shader.Find("Sprites/Default");
        }
        if (uiShader == null)
        {
            Debug.LogError("[KOTH] Не найден ни 'UI/Default', ни 'Sprites/Default'. Материалы не будут проставлены.");
            return;
        }

        // 2) Создаём/грузим единый материал
        const string matPath = "Assets/All Materials/_UIDefault_Auto.mat";
        System.IO.Directory.CreateDirectory("Assets/All Materials");
        var uiDefaultMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (uiDefaultMat == null)
        {
            uiDefaultMat = new Material(uiShader);
            AssetDatabase.CreateAsset(uiDefaultMat, matPath);
            AssetDatabase.SaveAssets();
        }
        else
        {
            // Если в проекте сменили пайплайн — обновим шейдер
            if (uiDefaultMat.shader != uiShader)
            {
                uiDefaultMat.shader = uiShader;
                EditorUtility.SetDirty(uiDefaultMat);
                AssetDatabase.SaveAssets();
            }
        }

        int imgFixed = 0, tmpFixed = 0;

        // 3) Проставляем материал всем Image
        foreach (var img in Object.FindObjectsOfType<Image>(true))
        {
            if (!img) continue;
            Undo.RecordObject(img, "Normalize UI Material (Image)");

            // Явно задаём материал (даже если раньше было None)
            img.material = uiDefaultMat;

            // Гарантируем видимую альфу
            var c = img.color;
            if (c.a <= 0f) { c.a = 1f; img.color = c; }

            img.SetAllDirty();
            EditorUtility.SetDirty(img);
            imgFixed++;
        }

        // 4) TMP: вернём дефолтные материалы шрифта (на всякий)
        foreach (var t in Object.FindObjectsOfType<TextMeshProUGUI>(true))
        {
            if (!t) continue;
            Undo.RecordObject(t, "Normalize UI Material (TMP)");

            if (t.font != null)
            {
                var defMat = t.font.material; // стандартный материал шрифта
                t.fontSharedMaterial = defMat;
                t.fontMaterial = defMat;
            }
            if (t.color.a <= 0f) t.color = Color.white;

            t.SetAllDirty();
            t.ForceMeshUpdate(true, true);
            EditorUtility.SetDirty(t);
            tmpFixed++;
        }

        Debug.Log($"[KOTH] Materials normalized (safe) → Images: {imgFixed}, TMP: {tmpFixed}. Материал: {matPath} (шейдер: {uiShader.name})");
    }
}
#endif
