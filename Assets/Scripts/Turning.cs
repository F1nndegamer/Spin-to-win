/*

This method is now depracated
Instead of updating the entire tilemap,
we rotate the camera and switch gravity

using UnityEngine;
using UnityEngine.Tilemaps;
public class Turning : MonoBehaviour
{
    public Tilemap sourceTilemap;
    public Tilemap targetTilemap;
    public Tilemap displayilemap;


    private int frameSize;
    public TileBase borderTile;
    public TileBase debugTile;
    public int size = 11;
    private Vector2 Center => new Vector2(size / 2f, size / 2f);
    public int rotation = 0; // 0,1,2,3 0, 90, 180, 270
    public bool Seeborder = true;
    private Vector3Int origin;
    private void Awake()
    {
        frameSize = size + 2;
    }

    private void Start()
    {
        DrawFrame();

        BoundsInt bounds = new BoundsInt(0, 0, 0, size, size, 1);
        origin = bounds.min;

        sourceTilemap.gameObject.SetActive(false);

        if (!Seeborder) displayilemap.gameObject.SetActive(false);
        Refresh();
    }
    public void RotateLeft()
    {
        rotation = (rotation + 3) % 4; // -1 mod 4
        Refresh();
    }

    public void RotateRight()
    {
        rotation = (rotation + 1) % 4;
        Refresh();
    }
    public void Refresh()
    {
        RotateTilemap();
    }
    public void DrawFrame()
    {
        displayilemap.ClearAllTiles();

        for (int x = 0; x < frameSize; x++)
        {
            for (int y = 0; y < frameSize; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);

                bool isBorder =
                    x == 0 || y == 0 ||
                    x == frameSize - 1 ||
                    y == frameSize - 1;

                if (isBorder)
                {
                    displayilemap.SetTile(pos, debugTile);
                }
            }
        }
        Vector2 frameCenter = new Vector2(
    (size / 2f) + 1,
    (size / 2f) + 1
);

        Vector3Int centerTilePos = new Vector3Int(
            Mathf.RoundToInt(frameCenter.x),
            Mathf.RoundToInt(frameCenter.y),
            0
        );

        displayilemap.SetTile(centerTilePos, borderTile);
    }
    public void RotateTilemap()
    {
        if (sourceTilemap == null || targetTilemap == null)
        {
            Debug.Log("NO ASSIGNMENTS!!");
            return;
        }
        BoundsInt bounds = sourceTilemap.cellBounds;

        targetTilemap.ClearAllTiles();

        foreach (var pos in bounds.allPositionsWithin)
        {
            TileBase tile = sourceTilemap.GetTile(pos);
            if (tile == null)
                continue;
            Vector3Int newPos = TransformPosition(pos);
            targetTilemap.SetTile(newPos, tile);
        }
    }
    public Vector3Int TransformPosition(Vector3Int pos)
    {
        Vector2 p = new Vector2(pos.x - origin.x, pos.y - origin.y);

        Vector2 center = new Vector2((size - 1) / 2f, (size - 1) / 2f);
        Vector2 rel = p - center;

        Vector2 rotated = rel;

        for (int i = 0; i < rotation; i++)
        {
            rotated = new Vector2(rotated.y, -rotated.x);
        }

        Vector2 result = rotated + center;

        return new Vector3Int(
            Mathf.RoundToInt(result.x + origin.x),
            Mathf.RoundToInt(result.y + origin.y),
            pos.z
        );
    }
}

*/
