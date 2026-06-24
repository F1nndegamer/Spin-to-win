using UnityEngine;

public class LevelData : MonoBehaviour
{
    //Here we will add things like Inventory etc.
    public Transform PlayerSpawnPos;

    [Tooltip("Whether or not the level is completable without editing")]
    public bool editingNeeded
    {
        get
        {
            return solidBlocks > 0 || teleportBlocks.Length > 0; // If inventory is not empty, the level should be editable
        }
    }
    
    public Transform topLeftBound, bottomRightBound;
    
    [Header("Inventory")] 
    public int solidBlocks = 0;
    public Level.Direction[] teleportBlocks;
    // Add more block types if levels use them 
    // [SerializeField] private int level = -1; We no longer store level number
    void Awake()
    {
        GameManager.levelData = this;
    }
}
