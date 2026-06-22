using UnityEngine;

public class LevelData : MonoBehaviour
{
    //Here we will add things like Inventory etc.
    public Transform PlayerSpawnPos;
    void Awake()
    {
        GameManager.levelData = this;
    }
}
