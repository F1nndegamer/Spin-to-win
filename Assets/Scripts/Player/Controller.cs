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

        if (Input.GetKeyDown(KeyCode.R))
        {
            level.Restart();
        }
    }
}