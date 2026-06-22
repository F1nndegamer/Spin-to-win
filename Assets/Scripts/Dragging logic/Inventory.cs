using UnityEngine;

public class Inventory : GameBehaviour
{
    void Awake()
    {
        GameManager.inventory = this;
    }

    public override void GameStart()
    {
        GameObject mainCamera = Camera.current.gameObject;
        transform.SetParent(mainCamera.transform);
    }
}
