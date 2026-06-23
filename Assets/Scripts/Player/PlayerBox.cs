using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : GameBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private bool grounded = false;
    private int sinceSwitch = 5;

    [SerializeField] private string TeleportTag = "Teleport";
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private Transform transition;

    private void Awake()
    {
        GameManager.player = this;
        transition.localScale = Vector3.one * 50f;
    }

    public override void GameStart()
    {
        StartCoroutine(TransitionInCoroutine());
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

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Win")
        {
            GetComponent<Rigidbody2D>().gravityScale = 0;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            GetComponent<Rigidbody2D>().angularVelocity = 0;
            GameManager.Instance.Win();
        }
    }

    public IEnumerator TransitionCoroutine() // Called and awaited by GameManager in LoadLevel coroutine
    {
        float t = 0;
        while (t < 50)
        {
            t += Time.unscaledDeltaTime * 30f;
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.one * 50;
    }

    private IEnumerator TransitionInCoroutine() // Called on level start by player
    {
        transform.position = GameManager.levelData.PlayerSpawnPos.transform.position;
        float t = 50;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime * 30f;
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.zero;
        GameManager.levelReady = true;
        GetComponent<Rigidbody2D>().gravityScale = 1; // Enable gravity only after scene is loaded successfully
    }
}
