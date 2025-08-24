// InputManager.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public Camera mainCam;
    public GridManager gridManager;
    public float inputCooldown = 0.12f;
    public int minChainLength = 3;

    List<Tile> selected = new List<Tile>();
    TileType? selectionType = null;
    bool inputLocked = false;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (gridManager == null) Debug.LogWarning("InputManager: gridManager not assigned.");
    }

    void Update()
    {
        if (inputLocked) return;

        // поддержка мыши и touch (основная логика через позицию)
        Vector2 pointerPos;
        bool pressing = false;
        bool released = false;
        bool begin = false;

        if (Input.touchCount > 0)
        {
            var t = Input.GetTouch(0);
            pointerPos = mainCam.ScreenToWorldPoint(t.position);

            // Если палец находится над UI — игнорируем этот кадр/этот палец
            if (IsPointerOverUIForTouch(t.fingerId))
            {
                // если мы уже в процессе выделения, но палец ушёл над UI — считаем это как отпуск
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                    released = true;
                else
                    return;
            }

            pressing = (t.phase == TouchPhase.Began || t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary);
            released = released || (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
            begin = (t.phase == TouchPhase.Began);
        }
        else
        {
            pointerPos = mainCam.ScreenToWorldPoint(Input.mousePosition);

            // если мышь над UI — не обрабатывать ввод
            if (IsPointerOverUIForMouse())
            {
                // если была предварительная выборка — при отпускании мыши всё равно обработаем EndSelection
                if (Input.GetMouseButtonUp(0))
                {
                    released = true;
                }
                else
                {
                    return;
                }
            }

            if (Input.GetMouseButtonDown(0)) { pressing = true; begin = true; }
            else if (Input.GetMouseButton(0)) pressing = true;
            else if (Input.GetMouseButtonUp(0)) released = true;
        }

        if (begin)
        {
            BeginSelection(pointerPos);
        }

        if (pressing)
        {
            ContinueSelection(pointerPos);
        }

        if (released)
        {
            EndSelection();
        }
    }

    bool IsPointerOverUIForMouse()
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject();
    }

    bool IsPointerOverUIForTouch(int fingerId)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    void BeginSelection(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Tile t = hit.collider.GetComponent<Tile>();
            if (t != null)
            {
                selected.Clear();
                selectionType = t.type;
                AddTileToSelection(t);
            }
        }
    }

    void ContinueSelection(Vector2 worldPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider == null) return;
        Tile t = hit.collider.GetComponent<Tile>();
        if (t == null) return;

        // если тип другой — игнорируем
        if (selectionType.HasValue && t.type != selectionType.Value) return;

        // если уже выбран — проверим backtrack: если он предпоследний — удаляем последний
        if (selected.Count >= 2 && t == selected[selected.Count - 2])
        {
            Tile last = selected[selected.Count - 1];
            last.SetHighlighted(false);
            selected.RemoveAt(selected.Count - 1);
            return;
        }
        // если уже выбран и не предпоследний — ничего
        if (selected.Contains(t)) return;

        // проверяем соседство (8 направлений)
        if (selected.Count > 0)
        {
            Tile prev = selected[selected.Count - 1];
            int dx = Mathf.Abs(prev.x - t.x);
            int dy = Mathf.Abs(prev.y - t.y);
            if (!((dx <= 1) && (dy <= 1)) || (dx == 0 && dy == 0))
            {
                // not adjacent
                return;
            }
        }

        AddTileToSelection(t);
    }

    void AddTileToSelection(Tile t)
    {
        selected.Add(t);
        t.SetHighlighted(true);
    }

    void EndSelection()
    {
        if (selected.Count >= minChainLength)
        {
            StartCoroutine(ProcessChain(selected));
        }
        else
        {
            foreach (var tile in selected) tile.SetHighlighted(false);
            selected.Clear();
            selectionType = null;
        }
    }

    IEnumerator ProcessChain(List<Tile> chain)
    {
        inputLocked = true;

        var toRemove = new List<Tile>(chain);

        // ВАЖНО: вызываем метод ProcessPlayerChain у GridManager,
        // чтобы ход считался только за действие игрока. GridManager
        // сам позаботится о каскадах и не будет уменьшать шаги за них.
        if (gridManager != null)
        {
            gridManager.ProcessPlayerChain(toRemove);
            // дождёмся окончания обработки в GridManager: GridManager запускает корутину внутри себя,
            // но чтобы избежать дублирования ожидания здесь — можно либо ждать, либо просто короткая пауза.
            // Для простоты дадим небольшую паузу, пока сетка обновляется.
            yield return new WaitForSeconds(inputCooldown + 0.05f);
        }
        else
        {
            Debug.LogWarning("InputManager: gridManager is null when processing chain.");
        }

        // очистим текущий выделенный набор
        foreach (var tile in selected) if (tile != null) tile.SetHighlighted(false);
        selected.Clear();
        selectionType = null;

        yield return new WaitForSeconds(inputCooldown);
        inputLocked = false;
    }
}
