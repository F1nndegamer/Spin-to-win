using System;
using UnityEngine;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : MonoBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private int sinceGrounded = 0;

    public bool IsGrounded()
    {
        return sinceGrounded > threshold;
    }

    void FixedUpdate()
    {
        if(sinceGrounded > 0) sinceGrounded--; // Fixed update runs first
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Ground" && (GetComponent<Rigidbody2D>().linearVelocity.magnitude < velocityThreshold))
        {
            sinceGrounded += 2; // This runs after fixed update, hence a net increment in value
        }
    }
}
