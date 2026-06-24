using UnityEngine;

public class GravityHandler : MonoBehaviour
{
    readonly Vector2 gravityDown = new Vector2(0, -1f);
    readonly Vector2 gravityUp = new Vector2(0, 1f);
    readonly Vector2 gravityLeft = new Vector2(-1f, 0);
    readonly Vector2 gravityRight = new Vector2(1f, 0);

    public void setGravityAngle(float angleDegrees, float magnitude)
    {
        Vector2 g = new Vector2(Mathf.Cos(angleDegrees * Mathf.Deg2Rad) * magnitude, Mathf.Sin(angleDegrees * Mathf.Deg2Rad) * magnitude);
        setGravityVector(g);
    }
    public void setGravityUp(float magnitude) { setGravityVector(gravityUp * magnitude); } // 90deg
    public void setGravityDown(float magnitude) { setGravityVector(gravityDown * magnitude); } // 270deg or -90deg
    public void setGravityLeft(float magnitude) { setGravityVector(gravityLeft * magnitude); } // 180deg
    public void setGravityRight(float magnitude) { setGravityVector(gravityRight * magnitude); } // 0deg
    private void setGravityVector(Vector2 newGravity) { Physics2D.gravity = newGravity; }
}
