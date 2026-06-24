using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPlayerInterationHandler : GameBehaviour
{
    public Tilemap tilemap;
    public GameObject player; // or just a general interactee
    public static TilemapPlayerInterationHandler instance;

    private bool readyForUpdate;
    // List of tiles for handling and stuff
    List<Vector3Int> fragileTilesToDestroy = new List<Vector3Int>();

    private void Awake()
    {
        instance = this;
    }
    private void findAndSetTilemap()
    {
        GameObject tilemaleGO = GameObject.Find("Tilemap");
        if (tilemaleGO == null){ return; } // we could not find a GameObject named "Tilemap" we will just return gracefully
        tilemap = tilemaleGO.GetComponent<Tilemap>();
    }

    public override void GameStart()
    {
        readyForUpdate = true;
    }

    void Update()
    {
        if (tilemap == null) {
            findAndSetTilemap();
            return; // we can afford to skip this frame, and if the function didn't assign a tilemap then we have just saved ourselves from crashing
        }

        if (!readyForUpdate) return;
        Vector3Int playerPos = positionToTilemapPos(player.transform.position);
        List<Vector3Int> adjacentTilePositions = new List<Vector3Int>() {
            positionToTilemapPos(player.transform.position + Vector3.up),
            positionToTilemapPos(player.transform.position + Vector3.down),
            positionToTilemapPos(player.transform.position + Vector3.left),
            positionToTilemapPos(player.transform.position + Vector3.right),
        };

        checkAndHandleTileCollisions(playerPos);
        checkFragileTiles(adjacentTilePositions);
    }

    private void checkAndHandleTileCollisions(Vector3Int playerPos)
    {
        TileBase tile = getTileAtPosition(playerPos);
        PlayerInteractionHandler playerInteractionHandler = player.GetComponent<PlayerInteractionHandler>();
        if (playerInteractionHandler == null) Debug.Log("wtf, why!!");
        // this switch statement is equivilent to `tile is SpikeTile`, it just looks cleaner like this imo -Sabrina
        switch (tile)
        {
            case SpikeTile:
                playerInteractionHandler.onSpikeInteration();
                break;
            case HoleTile:
                playerInteractionHandler.onHoleInteration();
                Debug.Log("hole tile touched!");
                break;
            case FragileWall:
                // this shouldn't happen... it should have a solid collider!
                Debug.Log("I shouldn't have been able to collide w/ FragileWall!");
                tilemap.SetTile(playerPos, null); // might as well destroy it now to free our player
                break;
            case WinTile:
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0;
                GameManager.Instance.Win();
                break;
        }
    }

    private void checkFragileTiles(List<Vector3Int> adjacentTilePositions)
    {
        for (int i = 0; i < fragileTilesToDestroy.Count; i++)
        {
            Vector3Int tilePos = fragileTilesToDestroy[i];
            if (adjacentTilePositions.Contains(tilePos)) { continue; } // We are still touching that tile, dont destroy it yet
            // destroy tile and remove it from the destroy queue
            tilemap.SetTile(tilePos, null);
            fragileTilesToDestroy.RemoveAt(i);
            i--;
        }

        for (int i = 0; i < adjacentTilePositions.Count; i++)
        {
            Vector3Int tilePos = adjacentTilePositions[i];
            if ((getTileAtPosition(tilePos) is FragileWall) == false) { continue; } // not a fragiel wall, don't care about it
            if (fragileTilesToDestroy.Contains(tilePos)) { continue; } // we've already queued for it's destruction
            fragileTilesToDestroy.Add(tilePos);
        }
    }


    Vector3Int positionToTilemapPos(Vector3 position) { return tilemap.WorldToCell(position); }
    TileBase getTileAtPosition(Vector3Int pos)
    {
        TileBase tile = tilemap.GetTile(pos);
        return tile;
    }
}
