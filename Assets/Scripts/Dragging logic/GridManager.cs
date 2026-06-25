using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class GridManager : GameBehaviour
{
    public static GridManager Instance;
    public float cellSize = 1f;
    public Vector2 gridOffset = new Vector2(0.5f, 0.5f);
    public Tilemap blockingTilemap;
    readonly Dictionary<Vector2Int, DraggableItem> occupied = new();
    private void Awake()
    {
        Instance = this;
    }

    public override void GameStart()
    {
        occupied.Clear();
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x / cellSize), Mathf.FloorToInt(worldPos.y / cellSize));
    }
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            cell.x * cellSize + gridOffset.x,
            cell.y * cellSize + gridOffset.y,
            0f
        );
    }
    public bool CanPlace(Vector2Int origin, DraggableItem item)
    {
        foreach (Vector2Int offset in item.GetOccupiedCells())
        {
            Vector2Int cell = origin + offset;
            if (occupied.ContainsKey(cell))
                return false;
            if (blockingTilemap == null)
                blockingTilemap = FindAnyObjectByType<Tilemap>();
            if (blockingTilemap != null)
            {
                if (blockingTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
                    return false;
            }
        }
        return true;
    }
    public void Register(DraggableItem item, Vector2Int origin)
    {
        foreach (Vector2Int offset in item.GetOccupiedCells())
        {
            occupied[origin + offset] = item;
        }
    }
    public void Unregister(DraggableItem item, Vector2Int origin)
    {
        foreach (Vector2Int offset in item.GetOccupiedCells())
        {
            Vector2Int cell = origin + offset;
            if (occupied.TryGetValue(cell, out var v) && v == item)
                occupied.Remove(cell);
        }
    }
}
