using UnityEngine;

public class LevelData : MonoBehaviour
{
    //Here we will add things like Inventory etc.
    public Transform PlayerSpawnPos;
    [SerializeField] private int level = -1;
    void Awake()
    {
        GameManager.levelData = this;
        GameManager.level = level;
    }
}
