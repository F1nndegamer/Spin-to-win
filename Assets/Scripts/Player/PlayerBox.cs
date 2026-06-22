using System;
using UnityEngine;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : MonoBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private bool grounded = false;
    private int sinceSwitch = 5;

    [SerializeField] private string TeleportTag = "Teleport";
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask checkLayer;

    public void GameStart()
    {
        
    }

    public bool WillTeleport(Level.Direction direction)
    {
        Vector2 origin = transform.position;
        Vector2 dir = Vector2.down;
        switch (direction)
        {
            case Level.Direction.Left:
                dir = Vector2.left;
                break;
            case Level.Direction.Right:
                dir = Vector2.right;
                break;
            case Level.Direction.Up:
                dir = Vector2.up;
                break;
            case Level.Direction.Down:
                dir = Vector2.down;
                break;
        }
        
        RaycastHit2D hit = Physics2D.Raycast(origin, dir, maxDistance, checkLayer);
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag(TeleportTag))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsGrounded()
    {
        if(grounded && sinceSwitch == 5 && (GetComponent<Rigidbody2D>().linearVelocity.magnitude < velocityThreshold)) { grounded = false;
            sinceSwitch = 0; return true; }
        return false;
    }

    private void FixedUpdate()
    {
        if (sinceSwitch < 5) sinceSwitch++;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground")
        {
            grounded = true;
        }
    }
}
