using System.Collections;
using UnityEngine;

public class Level : MonoBehaviour
{
    public Camera camera;
    public GravityHandler gravityHandler;
    public PlayerBox player;
    private bool rotating = false;
    public float gravity = 9.8f;
    
    [Header("Mechanic")]
    public bool AllowMidairSwitch = false;
    public bool PreserveMomentum = false;

    public enum Direction {
        Underflow = -1, // for some weird reason `-1 % 4 = -1` So i'll just add a case for this and manually correct it -Sabrina

        Down = 0,
        Right = 1,
        Up = 2,
        Left = 3
    };

    private Direction gravityDirection;
    
    private void LateUpdate() {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
    }

    public void Rotate(int dir)
    {
        // Only rotate once the player is settled, this adds more flexibility to puzzle and level design - Ali
        // Or does it? - VSauce, Michael
        if (rotating || (!IsGrounded() || AllowMidairSwitch)) return;
        
        // Using a coroutine for rotating the camera, otherwise the gravity switches while the camera is rotating; we cant wait for a coroutine to finish in a function -Sabrina
        StartCoroutine(RotateRoutine(dir));
    }


    IEnumerator RotateRoutine(int dir)
    {
        if(!PreserveMomentum) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Momentum be set to zero
        SetPhysicsEnabled(false);
        yield return StartCoroutine(RotateCamera(dir)); // wait for the rotate coroutine to finish before changing gravity -Sabrina
        gravityDirection = (Direction)(((int)gravityDirection + dir) % 4);
        if(gravityDirection == Direction.Underflow) { gravityDirection = Direction.Left; }
        UpdateGravity();

        SetPhysicsEnabled(true);
    }


    float lerpAngleFormula(float t)
    {
        // if we want a bezier curve, or ease in/out we can modify our formula/return here -Sabrina
        return t;
    }
    IEnumerator RotateCamera(int dir)
    {
        rotating = true;

        Vector3 startingRotation = camera.transform.rotation.eulerAngles;
        float endRotationZ = startingRotation.z + (90f * dir);
        float duration = 1f;
        float t = 0f;
        while(t < duration)
        {
            t += Time.deltaTime;
            float newZAngle = Mathf.LerpAngle(startingRotation.z, endRotationZ, lerpAngleFormula(t / duration));
            camera.transform.rotation = Quaternion.Euler(0, 0, newZAngle);
            yield return null;
        }

        rotating = false;
        yield return null;
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

    private bool IsGrounded()
    {
        return player.IsGrounded();
    }

    private void SetPhysicsEnabled(bool enabled)
    {
        SimulationMode2D mode = enabled ? SimulationMode2D.FixedUpdate : SimulationMode2D.Script;
        Physics2D.simulationMode = mode;
    }

    // Implement later after the rest of game is setup
    public void Pause() {}
    public void Restart() {}
}
