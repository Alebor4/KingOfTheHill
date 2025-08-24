using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Универсальный GridManager — инициализация сетки, простое удаление цепочек и "падение" тайлов вниз с заполнением сверху.
/// Предназначен как минимально-полноценная реализация, чтобы убрать ошибки компиляции:
/// - публичные поля rows, cols, grid
/// - InitGrid()
/// - ProcessPlayerChain(List<GameObject> chain) (или List<Tile> поддерживается)
/// 
/// Настройте в инспекторе: Tile Prefab (припаб должен содержать SpriteRenderer или ваш Tile компонент),
/// Tile Sprites (массив), Rows, Cols, Tile Spacing.
/// </summary>
public class GridManager : MonoBehaviour
{
    [Header("Grid")]
    [Tooltip("Rows (строки)")]
    public int rows = 8;           // lowercase — MatchFinder и другие скрипты могут ждать такие имена
    [Tooltip("Cols (столбцы)")]
    public int cols = 8;

    [Tooltip("Prefab for a single tile. Should contain SpriteRenderer (or your Tile component).")]
    public GameObject tilePrefab;

    [Tooltip("Sprites used for random tile creation (order not critical).")]
    public Sprite[] tileSprites;

    [Tooltip("Spacing between tiles (world units).")]
    public float tileSpacing = 1f;

    [Header("Movement / Behaviour")]
    public float tileMoveSpeed = 8f;

    [Header("References (optional)")]
    public ScoreManager scoreManager;
    public UIManager uiManager;
    public GameManager gameManager;

    // Public grid storage (lowercase name so other scripts that expect 'grid' find it)
    public GameObject[,] grid;

    // used to parent created tiles
    GameObject gridParent;

    void Awake()
    {
        // optional: try auto-find managers if not set
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();
        if (uiManager == null)
            uiManager = FindObjectOfType<UIManager>();
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        // create parent container
        gridParent = new GameObject("GridTiles");
        gridParent.transform.SetParent(transform, false);
    }

    /// <summary>
    /// Инициализация сетки. Вызывает заполнение случайными тайлами.
    /// </summary>
    public void InitGrid()
    {
        // guard
        if (tilePrefab == null)
        {
            Debug.LogError("GridManager.InitGrid: tilePrefab is null. Assign a prefab with a SpriteRenderer or Tile script.");
            return;
        }

        // create array
        grid = new GameObject[cols, rows];

        // clear old children
        for (int i = gridParent.transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(gridParent.transform.GetChild(i).gameObject);

        // fill
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject t = CreateTileAt(x, y);
                grid[x, y] = t;
            }
        }
    }

    /// <summary>
    /// Создаёт один тайл в позиции (gridX, gridY) и возвращает GameObject.
    /// Позиционирует по tileSpacing и делает родителем gridParent.
    /// </summary>
    GameObject CreateTileAt(int gridX, int gridY)
    {
        GameObject go = Instantiate(tilePrefab, gridParent.transform);
        go.name = $"Tile_{gridX}_{gridY}";

        // position in world/local space (origin at GridManager transform)
        Vector3 localPos = new Vector3(gridX * tileSpacing, -gridY * tileSpacing, 0f);
        go.transform.localPosition = localPos;

        // try to set sprite if tile has SpriteRenderer and we have sprites
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && tileSprites != null && tileSprites.Length > 0)
        {
            sr.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
        }
        else
        {
            // if there's a custom Tile component, try to set it via a common property if exists.
            var tileComp = go.GetComponent("Tile");
            if (tileComp != null)
            {
                // best effort - we don't know your Tile API; but many tile scripts expose SpriteRenderer or SetSprite
                var srField = tileComp.GetType().GetField("sr");
                if (srField != null && tileSprites != null && tileSprites.Length > 0)
                {
                    var srVal = srField.GetValue(tileComp) as SpriteRenderer;
                    if (srVal != null) srVal.sprite = tileSprites[Random.Range(0, tileSprites.Length)];
                }
            }
        }

        return go;
    }

    /// <summary>
    /// Обрабатывает удаление цепочки (список GameObject тайлов) — уничтожает их, опускает столбцы и заполняет сверху.
    /// Этот метод вызывается, когда у игрока образовалась цепочка.
    /// Поддерживает список GameObject или список компонентов Tile (best effort).
    /// </summary>
    public void ProcessPlayerChain(IList chain)
    {
        // Сначала преобразуем список chain в список координат (x,y) относительно grid, если это возможно.
        List<Vector2Int> coords = new List<Vector2Int>();

        foreach (var item in chain)
        {
            if (item == null) continue;

            if (item is GameObject go)
            {
                // попытаемся распарсить имя "Tile_x_y"
                var n = go.name;
                if (n.StartsWith("Tile_"))
                {
                    string[] parts = n.Split('_');
                    if (parts.Length >= 3 && int.TryParse(parts[1], out int gx) && int.TryParse(parts[2], out int gy))
                        coords.Add(new Vector2Int(gx, gy));
                    else
                    {
                        // попытка найти в grid ссылку
                        bool found = false;
                        for (int xi = 0; xi < cols && !found; xi++)
                            for (int yi = 0; yi < rows && !found; yi++)
                                if (grid[xi, yi] == go)
                                { coords.Add(new Vector2Int(xi, yi)); found = true; }
                    }
                }
                else
                {
                    // попытка найти позицию в массиве grid
                    bool found = false;
                    for (int xi = 0; xi < cols && !found; xi++)
                        for (int yi = 0; yi < rows && !found; yi++)
                            if (grid[xi, yi] == go)
                            { coords.Add(new Vector2Int(xi, yi)); found = true; }
                }
            }
            else
            {
                // если это Tile компонент (рефлексивно)
                var type = item.GetType();
                var goProp = type.GetProperty("gameObject");
                if (goProp != null)
                {
                    var goVal = goProp.GetValue(item) as GameObject;
                    if (goVal != null)
                    {
                        // поиск по grid
                        for (int xi = 0; xi < cols; xi++)
                            for (int yi = 0; yi < rows; yi++)
                                if (grid[xi, yi] == goVal)
                                    coords.Add(new Vector2Int(xi, yi));
                    }
                }
            }
        }

        // если нет координат — выходим
        if (coords.Count == 0)
        {
            Debug.LogWarning("GridManager.ProcessPlayerChain: no coords resolved from chain.");
            return;
        }

        // Удаляем все указанные тайлы (Destroy)
        foreach (var c in coords)
        {
            int x = c.x, y = c.y;
            if (x >= 0 && x < cols && y >= 0 && y < rows)
            {
                if (grid[x, y] != null)
                {
                    Destroy(grid[x, y]);
                    grid[x, y] = null;
                }
            }
        }

        // Поднять все элементы в каждом столбце
        StartCoroutine(CollapseAndFillRoutine());
    }

    IEnumerator CollapseAndFillRoutine()
    {
        // Для каждой колонки: соберём все существующие в ней объекты (снизу вверх), переместим их, создадим недостающие сверху
        for (int x = 0; x < cols; x++)
        {
            List<GameObject> column = new List<GameObject>();
            for (int y = 0; y < rows; y++)
            {
                if (grid[x, y] != null)
                    column.Add(grid[x, y]);
            }

            int missing = rows - column.Count;

            // очистим колонку
            for (int y = 0; y < rows; y++) grid[x, y] = null;

            // переместим существующие вниз (y от rows-1 вниз to 0)
            int writeY = rows - 1;
            for (int i = column.Count - 1; i >= 0; i--)
            {
                var go = column[i];
                grid[x, writeY] = go;
                StartCoroutine(MoveTileToGridPosition(go, x, writeY));
                writeY--;
            }

            // создадим недостающие сверху (наверху -> y от 0 до missing-1)
            for (int newY = 0; newY < missing; newY++)
            {
                int fillY = newY;
                GameObject newTile = CreateTileAt(x, -(missing - newY)); // временно за экраном (отрицательный y)
                // immediately place in world above and then animate to proper grid pos
                // set grid cell target index
                grid[x, fillY] = newTile;
                StartCoroutine(MoveTileToGridPosition(newTile, x, fillY));
            }
        }

        // ждём немного (даём анимации завершиться)
        yield return new WaitForSeconds(0.15f);

        // уведомим других менеджеров — best effort: если есть у gameManager метод OnChainProcessed, вызываем его.
        if (gameManager != null)
        {
            var gmType = gameManager.GetType();
            var method = gmType.GetMethod("OnChainProcessed");
            if (method != null)
                method.Invoke(gameManager, null);
        }

        yield break;
    }

    IEnumerator MoveTileToGridPosition(GameObject tile, int gridX, int gridY)
    {
        if (tile == null) yield break;

        Vector3 start = tile.transform.localPosition;
        Vector3 target = new Vector3(gridX * tileSpacing, -gridY * tileSpacing, 0f);

        float t = 0f;
        float duration = Mathf.Max(0.01f, Vector3.Distance(start, target) / Mathf.Max(1f, tileMoveSpeed));
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            tile.transform.localPosition = Vector3.Lerp(start, target, p);
            yield return null;
        }
        tile.transform.localPosition = target;
    }

    // Overload: удобный обёртка, если другие скрипты передают List<GameObject>
    public void ProcessPlayerChain(List<GameObject> chain)
    {
        ProcessPlayerChain((IList)chain);
    }

    // Overload: если передают массив
    public void ProcessPlayerChain(GameObject[] chain)
    {
        ProcessPlayerChain((IList)chain);
    }

    // Utility: get GameObject at grid coords (safe)
    public GameObject GetTileAt(int x, int y)
    {
        if (grid == null) return null;
        if (x < 0 || x >= cols || y < 0 || y >= rows) return null;
        return grid[x, y];
    }

#if UNITY_EDITOR
    // редакторская вспомогательная кнопка
    [ContextMenu("Rebuild Grid (Editor)")]
    public void EditorRebuildGrid()
    {
        InitGrid();
    }
#endif
}
