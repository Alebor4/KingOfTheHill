using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// MatchFinder — универсальный помощник для поиска связанных (соседних) тайлов одинакового типа.
/// Поддерживает два механизма сравнения тайлов:
/// 1) Если у GameObject есть компонент "Tile" с публичным полем/свойством "type" — будет использован он;
/// 2) Иначе будет сравниваться SpriteRenderer.sprite (ссылка на Sprite).
///
/// Основные методы:
/// - FindAllMatches(grid, cols, rows, minMatchSize) -> возвращает список найденных кластеров (каждый кластер — List<GameObject>)
/// - GetConnectedChain(grid, cols, rows, startX, startY) -> возвращает одну связанную компонента, начиная со startX,startY
/// </summary>
public static class MatchFinder
{
    // Вспомогательная структура для обхода 8 направлений
    static readonly Vector2Int[] dirs8 = new Vector2Int[]
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1),
        new Vector2Int(1,1),
        new Vector2Int(1,-1),
        new Vector2Int(-1,1),
        new Vector2Int(-1,-1)
    };

    /// <summary>
    /// Пытается сравнить два GameObject'а как "одинаковые" (один тип).
    /// Сначала ищем компонент "Tile" и поле/properties "type" (любого типа) — сравниваем через Equals.
    /// Если Tile не найден или нет подходящего поля — сравниваем SpriteRenderer.sprite ссылки.
    /// </summary>
    static bool SameType(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;
        if (a == b) return true;

        // Попробуем получить компонент Tile (если он есть) и поле/property "type"
        var compA = a.GetComponent("Tile");
        var compB = b.GetComponent("Tile");
        if (compA != null && compB != null)
        {
            // Попробуем достать поле "type" или свойство "type"
            var typeFieldA = compA.GetType().GetField("type");
            var typeFieldB = compB.GetType().GetField("type");
            if (typeFieldA != null && typeFieldB != null)
            {
                var vA = typeFieldA.GetValue(compA);
                var vB = typeFieldB.GetValue(compB);
                if (vA != null && vB != null) return vA.Equals(vB);
            }
            else
            {
                var propA = compA.GetType().GetProperty("type");
                var propB = compB.GetType().GetProperty("type");
                if (propA != null && propB != null)
                {
                    var vA = propA.GetValue(compA, null);
                    var vB = propB.GetValue(compB, null);
                    if (vA != null && vB != null) return vA.Equals(vB);
                }
            }
            // если Tile есть, но не удалось прочитать type, пробуем сравнить спрайты ниже
        }

        // fallback: сравнение спрайтов
        var srA = a.GetComponent<SpriteRenderer>();
        var srB = b.GetComponent<SpriteRenderer>();
        if (srA != null && srB != null)
        {
            return srA.sprite == srB.sprite;
        }

        // если ничего нет — считать не равными
        return false;
    }

    /// <summary>
    /// Возвращает список связанных тайлов (connected components) одинакового типа, начиная от cell (startX,startY).
    /// Если cell пустой или невалидный — вернёт пустой список.
    /// </summary>
    public static List<GameObject> GetConnectedChain(GameObject[,] grid, int cols, int rows, int startX, int startY)
    {
        List<GameObject> result = new List<GameObject>();
        if (!IsValidCoord(startX, startY, cols, rows)) return result;
        if (grid == null) return result;

        GameObject start = grid[startX, startY];
        if (start == null) return result;

        bool[,] visited = new bool[cols, rows];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(new Vector2Int(startX, startY));
        visited[startX, startY] = true;

        while (q.Count > 0)
        {
            var cur = q.Dequeue();
            GameObject curGo = grid[cur.x, cur.y];
            if (curGo == null) continue;
            result.Add(curGo);

            foreach (var d in dirs8)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                if (!IsValidCoord(nx, ny, cols, rows)) continue;
                if (visited[nx, ny]) continue;
                var neighbor = grid[nx, ny];
                if (neighbor == null) continue;

                if (SameType(curGo, neighbor))
                {
                    visited[nx, ny] = true;
                    q.Enqueue(new Vector2Int(nx, ny));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Ищет все кластеры одинаковых тайлов в сетке. minMatchSize — минимальный размер кластера, который считается матчем.
    /// Возвращает список кластеров (каждый — List<GameObject>).
    /// </summary>
    public static List<List<GameObject>> FindAllMatches(GameObject[,] grid, int cols, int rows, int minMatchSize = 3)
    {
        List<List<GameObject>> matches = new List<List<GameObject>>();
        if (grid == null) return matches;

        bool[,] seen = new bool[cols, rows];
        for (int x = 0; x < cols; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (seen[x, y]) continue;
                var go = grid[x, y];
                if (go == null) { seen[x, y] = true; continue; }

                // получаем связанный компонент
                var cluster = GetConnectedChain(grid, cols, rows, x, y);
                // пометить все клетки этого кластера как просмотренные
                foreach (var g in cluster)
                {
                    // попытка получить координаты через имя Tile_x_y
                    if (TryGetCoordsFromName(g, out int gx, out int gy))
                    {
                        if (IsValidCoord(gx, gy, cols, rows))
                            seen[gx, gy] = true;
                    }
                    else
                    {
                        // fallback: искать в grid (медленно, но безопасно)
                        bool found = false;
                        for (int xi = 0; xi < cols && !found; xi++)
                            for (int yi = 0; yi < rows && !found; yi++)
                                if (grid[xi, yi] == g)
                                {
                                    seen[xi, yi] = true;
                                    found = true;
                                }
                    }
                }

                if (cluster.Count >= minMatchSize)
                    matches.Add(cluster);
            }
        }
        return matches;
    }

    // Helper: проверка диапазона
    static bool IsValidCoord(int x, int y, int cols, int rows)
    {
        return x >= 0 && x < cols && y >= 0 && y < rows;
    }

    // Helper: пытаемся распарсить файлом имя вида "Tile_x_y" чтобы получить координаты
    static bool TryGetCoordsFromName(GameObject go, out int gx, out int gy)
    {
        gx = gy = -1;
        if (go == null) return false;
        string n = go.name;
        if (string.IsNullOrEmpty(n)) return false;
        // ожидаем формат Tile_X_Y или похожий
        string[] parts = n.Split('_');
        if (parts.Length >= 3)
        {
            if (int.TryParse(parts[1], out gx) && int.TryParse(parts[2], out gy))
                return true;
        }
        return false;
    }
}
