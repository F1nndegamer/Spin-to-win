using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
public class GridManager : MonoBehaviour
{
    public static GridManager Instance;
    public float cellSize = 1f;
    public Vector2 gridOffset = new Vector2(0.5f, 0.5f);
    public Tilemap blockingTilemap;
    private Dictionary<Vector2Int, DraggableItem> occupied = new();
    private void Awake()
    {
        Instance = this;
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
    public bool CanPlace(Vector2Int origin, Vector2Int size)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (occupied.ContainsKey(cell))
                    return false;
                if (blockingTilemap != null)
                {
                    if (blockingTilemap.HasTile(new Vector3Int(cell.x, cell.y, 0)))
                        return false;
                }
            }
        }
        return true;
    }
    public void Register(DraggableItem item, Vector2Int origin)
    {
        for (int x = 0; x < item.size.x; x++)
        {
            for (int y = 0; y < item.size.y; y++)
            {
                occupied[origin + new Vector2Int(x, y)] = item;
            }
        }
    }
    public void Unregister(DraggableItem item, Vector2Int origin)
    {
        for (int x = 0; x < item.size.x; x++)
        {
            for (int y = 0; y < item.size.y; y++)
            {
                Vector2Int cell = origin + new Vector2Int(x, y);
                if (occupied.TryGetValue(cell, out var v) && v == item)
                    occupied.Remove(cell);
            }
        }
    }
}
