using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : GameBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private bool grounded = false;
    private int sinceSwitch = 5;
    private bool nextLevel = false;

    [SerializeField] private string TeleportTag = "Teleport";
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private Transform transition;
    [SerializeField] private CanvasGroup levelCompletePanel;
    [SerializeField] private TextMeshProUGUI movesText, timeText;
    [SerializeField] private Button nextLevelButton;

    private void Awake()
    {
        GameManager.player = this;
        transition.localScale = Vector3.one * 50f;
    }

    public override void GameStart()
    {
        StartCoroutine(TransitionInCoroutine());
        nextLevel = false;
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

    public static string FormatTime(float s) // Utiliy function so we can convert float amount of seconds to string
    {
        return s >= 3600 ? $"{(int)s / 3600}h {(int)s / 60 % 60}m {(int)s % 60}s" :
            (s >= 60 ? $"{(int)s / 60}m {(int)s % 60}s" :
                $"{(int)s}s");
    }

    public void NextLevel()
    {
        nextLevel = true;
    }

    public IEnumerator TransitionCoroutine() // Called and awaited by GameManager in LoadLevel coroutine
    {
        // Transition the box to cover the whole screen
        float t = 0;
        while (t < 50)
        {
            t += Time.unscaledDeltaTime * 30f;
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.one * 50;
        
        // Transition in the levelCompletePanel and show stats
        levelCompletePanel.gameObject.SetActive(true);
        t = 0;
        while (t < 1)
        {
            t += Time.unscaledDeltaTime;
            levelCompletePanel.alpha = t;
            movesText.text = $"Moves: {Mathf.CeilToInt(Mathf.Lerp(0, GameManager.movesThisLevel, t))}";
            timeText.text = $"Time taken: {FormatTime(GameManager.timeThisLevel * t)}";
            yield return null;
        }
        levelCompletePanel.alpha = 1;
        movesText.text = $"Moves: {GameManager.movesThisLevel}";
        timeText.text = $"Time taken: {FormatTime(GameManager.timeThisLevel)}";
        
        // THen we wait until the next level button is pressed
        yield return new WaitUntil(() => nextLevel);
        
        // Quickly hide the level complete panel
        t = 1;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime * 5;
            levelCompletePanel.alpha = t;
            yield return null;
        }
        levelCompletePanel.alpha = 0;
        movesText.text = timeText.text = "";
        levelCompletePanel.gameObject.SetActive(false);
    }

    private IEnumerator TransitionInCoroutine() // Called on level start by player
    {
        // Move the player to spawn while the box is covering the screen
        transform.position = GameManager.levelData.PlayerSpawnPos.transform.position;
        // Uncover the screen
        float t = 50;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime * 30f;
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.zero;
        GameManager.levelReady = true;
        GetComponent<Rigidbody2D>().gravityScale = 1; // Enable gravity only after everything is ready
    }
}
