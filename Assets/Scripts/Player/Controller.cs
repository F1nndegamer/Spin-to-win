using UnityEngine;

public class Controller : MonoBehaviour
{
    public Level level;

    private void Awake()
    {
        level = GetComponent<Level>();
        level.UpdateGravity(); // update the gravity immediately so we aren't floating as soon as we start -Sabrina
    }

    private void Update()
    {
        if (!GameManager.levelLoaded) return;

        Vector3Int input = new Vector3Int(0, 0, 0);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            input += new Vector3Int(-1, 0, 0);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            input += new Vector3Int(1, 0, 0);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            input += new Vector3Int(0, 1, 0);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            input += new Vector3Int(0, -1, 0);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            input += new Vector3Int(0, 0, -1);
        }

        if (Input.GetKey(KeyCode.E))
        {
            input += new Vector3Int(0, 0, 1);
        }
        
        level.PassDirectionalInput(input.x, input.y, input.z);

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            level.Pause();
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            level.Confirm();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            level.Restart();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            level.ShiftMode(true);
        }

        if (Input.GetKeyUp(KeyCode.RightShift))
        {
            level.ShiftMode(false);
        }
    }
}