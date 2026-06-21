using System;
using UnityEngine;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : MonoBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private bool grounded = false;
    private int sinceSwitch = 5;

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
