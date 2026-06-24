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
            obj.GetComponent<DraggableItem>().CloneAmount = data.solidBlocks;
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
                obj.GetComponent<Teleporter>().direction = Level.Direction.Up;
                obj.GetComponent<DraggableItem>().CloneAmount = ups;
                obj.transform.SetParent(transform);
            }
            if (rights != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionRight, Quaternion.Euler(0, 0, 90));
                obj.GetComponent<Teleporter>().direction = Level.Direction.Right;
                obj.GetComponent<DraggableItem>().CloneAmount = rights;
                obj.transform.SetParent(transform);
            }
            if (lefts != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionLeft, Quaternion.Euler(0, 0, -90));
                obj.GetComponent<Teleporter>().direction = Level.Direction.Left;
                obj.GetComponent<DraggableItem>().CloneAmount = lefts;
                obj.transform.SetParent(transform);
            }
            if (downs != 0)
            {
                GameObject obj = Instantiate(level.teleportObject, level.teleporObjectPositionDown, Quaternion.Euler(0, 0, 180));
                obj.GetComponent<Teleporter>().direction = Level.Direction.Down;
                obj.GetComponent<DraggableItem>().CloneAmount = downs;
                obj.transform.SetParent(transform);
            }
        }
    }
}
