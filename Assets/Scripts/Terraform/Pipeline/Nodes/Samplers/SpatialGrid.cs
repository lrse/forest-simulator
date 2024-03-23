using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialGrid {
    private int width;
    private int height;
    private int gridWidth;
    private int gridHeight;
    private float minRadius;
    private float cellSize;
    private List<SpatialItem> items;
    private List<int>[,] grid;

    public SpatialGrid(int width, int height, float minRadius) {
        this.width = width;
        this.height = height;
        this.minRadius = minRadius;
        this.cellSize = minRadius / Mathf.Sqrt(2);
        this.gridWidth = Mathf.CeilToInt(width / cellSize);
        this.gridHeight = Mathf.CeilToInt(height / cellSize);
        Clear();
    }

    public bool IsValidPos(Vector2 pos, float radius) {

        if (!InBounds(pos)) {
            return false;
        }

        (Vector2Int rowSpan, Vector2Int colSpan) hood = GetNeighbours(pos, radius);

        for (int i = hood.rowSpan.x; i <= hood.rowSpan.y; i++) {
            for (int j = hood.colSpan.x; j <= hood.colSpan.y; j++) {
                List<int> points = grid[i, j];

                foreach (int idx in points) {
                    SpatialItem item = items[idx];
                    float minRadiusBetweenPoints = Mathf.Min(item.radius, radius);

                    if ((pos - item.pos).sqrMagnitude < (minRadiusBetweenPoints * minRadiusBetweenPoints)) {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public void AddItem(Vector2 pos, float radius) {
        int itemIdx = items.Count;
        SpatialItem newItem = new SpatialItem(itemIdx, pos, radius);
        items.Add(newItem);

        (Vector2Int rowSpan, Vector2Int colSpan) hood = GetNeighbours(pos, radius);

        for (int i = hood.rowSpan.x; i <= hood.rowSpan.y; i++) {
            for (int j = hood.colSpan.x; j <= hood.colSpan.y; j++) {
                grid[i, j].Add(itemIdx);
            }
        }
    }

    (Vector2Int rowSpan, Vector2Int colSpan) GetNeighbours(Vector2 pos, float radius) {
        Vector2Int cell = GetGridIndex(pos);
        int neighbourhood = Mathf.CeilToInt(radius / cellSize);
        int startX = Mathf.Max(0, cell.x - neighbourhood);
        int endX = Mathf.Min(cell.x + neighbourhood, gridWidth - 1);
        int startY = Mathf.Max(0, cell.y - neighbourhood);
        int endY = Mathf.Min(cell.y + neighbourhood, gridHeight - 1);
        return (rowSpan: new Vector2Int(startX, endX), colSpan: new Vector2Int(startY, endY));
    }

    Vector2Int GetGridIndex(Vector2 pos) {
        int cellX = (int)(pos.x / cellSize);
        int cellY = (int)(pos.y / cellSize);
        return new Vector2Int(cellX, cellY);
    }

    public bool InBounds(Vector2 pos) {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public void Clear() {
        this.grid = new List<int>[gridWidth, gridHeight];
        for (int i = 0; i < gridWidth; i++) {
            for (int j = 0; j < gridHeight; j++) {
                grid[i, j] = new List<int>();
            }
        }
        this.items = new List<SpatialItem>();
    }
}

public struct SpatialItem {
    public int index;
    public Vector2 pos;
    public float radius;

    public SpatialItem(int index, Vector2 pos, float radius) {
        this.index = index;
        this.pos = pos;
        this.radius = radius;
    }
}
