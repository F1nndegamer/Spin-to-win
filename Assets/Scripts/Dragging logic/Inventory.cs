using Unity.VisualScripting;
using UnityEngine;

public class Inventory : GameBehaviour
{
    void Awake()
    {
        GameManager.inventory = this;
    }

    public override void GameStart()
    {
        GameObject mainCamera = Level.Instance.gameObject;
        transform.SetParent(mainCamera.transform);
        
        // make sure the inventory object isn't too far away from the screen - Sabrina
        float z = transform.position.z;
        transform.position = Level.Instance.camera.ViewportToWorldPoint(new Vector3(0.20f, 0.5f, Mathf.Abs(Camera.main.transform.position.z)));
        transform.position = new Vector3(transform.position.x, transform.position.y, z);

        // remove all children of the inventory, since we should be empty before filling it up, right? - Sabrina
        foreach (Transform t in transform) { Destroy(t.gameObject); }

        SpawnInventory();
    }
    public void SpawnInventory()
    {
        LevelData data = GameManager.levelData;
        Level level = Level.Instance;
        if (data.solidBlocks != 0)
        {
            GameObject obj = Instantiate(level.normalObject, level.normalObjectPosition, Quaternion.identity);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero; // make sure we dont spawn the block outside of our camera's range - Sabrina
            obj.GetComponent<DraggableItem>().CloneAmount = data.solidBlocks;
            obj.name = $"SolidBlocks_{data.solidBlocks}";
        }
        if (data.teleportBlocks.Length != 0)
        {
            int ups = 0;
            int rights = 0;
            int downs = 0;
            int lefts = 0;
            foreach (Level.Direction dir in data.teleportBlocks)
            {
                switch (dir)
                {
                    case Level.Direction.Up:
                        ups++;
                        break;

                    case Level.Direction.Right:
                        rights++;
                        break;

                    case Level.Direction.Down:
                        downs++;
                        break;

                    case Level.Direction.Left:
                        lefts++;
                        break;

                    case Level.Direction.Underflow:
                        Debug.LogWarning("Unexpected Underflow direction!");
                        break;
                }
            }
            if (ups != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionUp, Quaternion.identity);
                obj.GetComponentInChildren<Teleporter>().direction = Level.Direction.Up;
                obj.GetComponent<DraggableItem>().CloneAmount = ups;
                obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.right * 2;
            }
            if (rights != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionRight, Quaternion.Euler(0, 0, 90));
                obj.GetComponentInChildren<Teleporter>().direction = Level.Direction.Right;
                obj.AddComponent<DraggableItem>().CloneAmount = rights;
                obj.transform.SetParent(transform);
                obj.transform.localPosition = Vector3.right * 2; // spawn it under our solid blocks, even if we dont have any
            }
            if (lefts != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionLeft, Quaternion.Euler(0, 0, -90));
                obj.GetComponentInChildren<Teleporter>().direction = Level.Direction.Left;
                obj.GetComponent<DraggableItem>().CloneAmount = lefts;
                obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.right * 2;
            }
            if (downs != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionDown, Quaternion.Euler(0, 0, 180));
                obj.GetComponentInChildren<Teleporter>().direction = Level.Direction.Down;
                obj.GetComponent<DraggableItem>().CloneAmount = downs;
                obj.transform.SetParent(transform);
		obj.transform.localPosition = Vector3.right * 2;
            }
        }
    }
}
