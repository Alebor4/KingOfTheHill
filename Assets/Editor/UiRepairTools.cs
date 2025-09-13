#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public static class UiRepairTools
{
    [MenuItem("KOTH/Fix UI: Freeze Layouts & Repair NaNs")]
    public static void FreezeLayoutsAndFix()
    {
        var rootCanvases = Object.FindObjectsOfType<Canvas>(true);
        int disabledLayouts = 0, removedCSF = 0;

        foreach (var c in rootCanvases)
        {
            foreach (var csf in c.GetComponentsInChildren<ContentSizeFitter>(true))
            {
                Undo.RecordObject(csf, "Disable CSF");
                csf.enabled = false; // можно и удалить, но пока просто выключим
                disabledLayouts++; removedCSF++;
                EditorUtility.SetDirty(csf);
            }
            foreach (var lg in c.GetComponentsInChildren<LayoutGroup>(true))
            {
                Undo.RecordObject(lg, "Disable LayoutGroup");
                lg.enabled = false;
                disabledLayouts++;
                EditorUtility.SetDirty(lg);
            }
        }

        int fixedRects = FixAllRectTransformsTopDown();
        int fixedImages = RestoreMissingSprites();
        int fixedScalers = FixCanvasScalers();

        Debug.Log($"[KOTH UI Repair] Disabled Layouts: {disabledLayouts} (CSF: {removedCSF}), Fixed RectTransforms: {fixedRects}, Images: {fixedImages}, CanvasScalers: {fixedScalers}");
    }

    static int FixAllRectTransformsTopDown()
    {
        var all = Object.FindObjectsOfType<RectTransform>(true);
        // сортировка по глубине: сначала корни, потом дети
        System.Array.Sort(all, (a, b) => GetDepth(a).CompareTo(GetDepth(b)));
        int fixedCount = 0;

        foreach (var rt in all)
        {
            bool touched = false;
            Undo.RecordObject(rt, "Fix NaN RectTransform");

            if (HasNaN(rt.anchorMin)) { rt.anchorMin = new Vector2(0.5f, 0.5f); touched = true; }
            if (HasNaN(rt.anchorMax)) { rt.anchorMax = new Vector2(0.5f, 0.5f); touched = true; }
            if (HasNaN(rt.pivot)) { rt.pivot = new Vector2(0.5f, 0.5f); touched = true; }
            if (HasNaN(rt.anchoredPosition)) { rt.anchoredPosition = Vector2.zero; touched = true; }
            if (HasNaN(rt.sizeDelta)) { rt.sizeDelta = new Vector2(200, 60); touched = true; }
            if (HasNaN(rt.localPosition)) { rt.localPosition = Vector3.zero; touched = true; }
            if (HasNaN(rt.localScale)) { rt.localScale = Vector3.one; touched = true; }

            var q = rt.localRotation;
            if (float.IsNaN(q.x) || float.IsNaN(q.y) || float.IsNaN(q.z) || float.IsNaN(q.w))
            { rt.localRotation = Quaternion.identity; touched = true; }

            if (touched) { fixedCount++; EditorUtility.SetDirty(rt); }
        }
        return fixedCount;
    }

    static int RestoreMissingSprites()
    {
        int fixedImages = 0;
        var builtin = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
        foreach (var img in Object.FindObjectsOfType<Image>(true))
        {
            if (img.sprite == null)
            {
                Undo.RecordObject(img, "Restore UISprite");
                img.sprite = builtin;
                fixedImages++;
                EditorUtility.SetDirty(img);
            }
            // уберём NaN в цвете на всякий
            var c = img.color;
            if (float.IsNaN(c.r) || float.IsNaN(c.g) || float.IsNaN(c.b) || float.IsNaN(c.a))
            { img.color = Color.white; EditorUtility.SetDirty(img); }
        }
        return fixedImages;
    }

    static int FixCanvasScalers()
    {
        int fixedScalers = 0;
        foreach (var scaler in Object.FindObjectsOfType<CanvasScaler>(true))
        {
            if (scaler.uiScaleMode == CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                if (scaler.referenceResolution.x <= 0 || scaler.referenceResolution.y <= 0)
                {
                    Undo.RecordObject(scaler, "Fix CanvasScaler");
                    scaler.referenceResolution = new Vector2(1080, 1920);
                    fixedScalers++;
                    EditorUtility.SetDirty(scaler);
                }
            }
        }
        return fixedScalers;
    }

    static int GetDepth(Transform t)
    {
        int d = 0; while (t != null) { d++; t = t.parent; }
        return d;
    }
    static bool HasNaN(Vector2 v) => float.IsNaN(v.x) || float.IsNaN(v.y);
    static bool HasNaN(Vector3 v) => float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
}
#endif
