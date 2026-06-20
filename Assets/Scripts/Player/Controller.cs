using UnityEngine;

public class Controller : MonoBehaviour
{
    public Level level;

    void Awake() {
        level = GetComponent<Level>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            level.Rotate(-1);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            level.Rotate(1);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            level.Pause();
        }

        if (Input.GetKey(KeyCode.R))
        {
            level.Restart();
        }
    }
}