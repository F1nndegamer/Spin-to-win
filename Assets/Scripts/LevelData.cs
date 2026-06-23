using UnityEngine;

public class LevelData : MonoBehaviour
{
    //Here we will add things like Inventory etc.
    public Transform PlayerSpawnPos;
    // [SerializeField] private int level = -1; We no longer store level number
    void Awake()
    {
        GameManager.levelData = this;
    }
}
