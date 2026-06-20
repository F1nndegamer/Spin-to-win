using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapPlayerInterationHandler : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject player; // or just a general interactee

    void Start()
    {
        // if we forget to assign the tilemap then a fallback is that we placed the script directly on the tilemap
        if(tilemap == null) { tilemap = transform.GetComponent<Tilemap>(); }
    }

    void Update()
    {
        Vector3Int cell = tilemap.WorldToCell(player.transform.position);
        TileBase tile = tilemap.GetTile(cell);

        // this switch statement is equivilent to `tile is SpikeTile`, it just looks cleaner like this imo -Sabrina
        switch (tile)
        {
            case SpikeTile:
                // idk something like this we should call
                // player.getComponent<TileInterationTaker>().onSpikeInteration();
                Debug.Log("hey hey i just touched a spike tile!");
                break;
            case HoleTile:
                // idk something like this we should call
                // player.getComponent<TileInterationTaker>().onHoleInteration();
                Debug.Log("hole tile touched!");
                break;
        }
    }
}
