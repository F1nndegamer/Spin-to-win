using UnityEngine;

public class Level : MonoBehaviour
{
    public Camera camera;
    public GravityHandler gravityHandler;
    private bool rotating = false;
    public float gravity = 1f;

    public enum Direction {
        Down = 0,
        Right = 1,
        Up = 2,
        Left = 3
    };

    private Direction gravityDirection;

    public void Rotate(int dir)
    {
        if(rotating) return;
        gravityDirection = (Direction)(((int)gravityDirection + dir) % 4);
        UpdateGravity();
    }

    public void UpdateGravity(){
        switch (gravityDirection) {
            case Direction.Down:
                gravityHandler.setGravityDown(gravity);
                break;
            case Direction.Right:
                gravityHandler.setGravityRight(gravity);
                break;
            case Direction.Up:
                gravityHandler.setGravityUp(gravity);
                break;
            case Direction.Left:
                gravityHandler.setGravityLeft(gravity);
                break;
        }
    }

    // Implement later after the rest of game is setup
    public void Pause() {}
    public void Restart() {}
}
