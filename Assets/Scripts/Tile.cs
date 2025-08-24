// Tile.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class Tile : MonoBehaviour
{
    public TileType type;
    [HideInInspector] public int x, y;

    SpriteRenderer sr;
    Color originalColor;
    Vector3 originalScale;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) Debug.LogWarning($"Tile '{name}': SpriteRenderer found in children.", this);
        }
        if (sr == null)
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            Debug.LogError($"Tile '{name}': SpriteRenderer was missing — auto-added.", this);
        }

        originalColor = sr.color;
        originalScale = transform.localScale;
    }

    public void SetSprite(Sprite s)
    {
        if (sr == null) return;
        if (s != null) sr.sprite = s;
    }

    // Визуальная подсветка при выделении цепочки
    public void SetHighlighted(bool state)
    {
        if (sr == null) return;
        if (state)
        {
            sr.color = new Color(1f, 1f, 1f, 0.85f); // чуть ярче / можно изменить
            transform.localScale = originalScale * 1.08f;
        }
        else
        {
            sr.color = originalColor;
            transform.localScale = originalScale;
        }
    }

    public void MoveTo(Vector3 target, float speed = 12f)
    {
        StopAllCoroutines();
        StartCoroutine(MoveCoroutine(target, speed));
    }

    System.Collections.IEnumerator MoveCoroutine(Vector3 target, float speed)
    {
        while ((transform.position - target).sqrMagnitude > 0.0001f)
        {
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * speed);
            yield return null;
        }
        transform.position = target;
    }
}
