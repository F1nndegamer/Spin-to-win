using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// This script measures whether or not the player is currently grounded - Ali

public class PlayerBox : GameBehaviour
{
    public int threshold = 5; // Number of FixedUpdates that player collision stays until it is marked as grounded
    public float velocityThreshold = 0.05f; // Minimum velocity to count the object being at rest
    private bool grounded;
    private int sinceSwitch;
    private bool nextLevel;

    [SerializeField] private string TeleportTag = "Teleport";
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private Transform transition;
    [SerializeField] private CanvasGroup levelCompletePanel;
    [SerializeField] private TextMeshProUGUI movesText, timeText;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private GameObject HitGroundVFX_Prefab;
    [SerializeField] public GameObject Star1;
    [SerializeField] public GameObject Star2;
    [SerializeField] public GameObject Star3;
    public Sprite HollowStar;
    public Sprite FilledStar;

    private Transform _particleThing;

    private void Awake()
    {
        GameManager.player = this;
        transition.localScale = Vector3.one * 50f;
    }

    public override void GameAwake()
    {
        // Move the player to spawn while the box is covering the screen
        transform.position = GameManager.levelData.PlayerSpawnPos.position;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        GetComponent<SpriteRenderer>().color = Color.clear;
        GetComponent<Rigidbody2D>().linearVelocity = Vector3.zero;
        GetComponent<Rigidbody2D>().gravityScale = 0;
        GetComponent<BoxCollider2D>().enabled = false;

        // Definitely not me forgetting the instantiation syntax :sob: - Ali
        _particleThing = GameObject.Instantiate(Resources.Load("Prefabs/SpawnGlow") as GameObject, GameManager.levelData.PlayerSpawnPos).transform;

        StartCoroutine(TransitionInCoroutine());
        nextLevel = false;
    }

    public void Confirm() // Called when Level.Confirm is called, exits edit mode, enters play mode
    {
        StartCoroutine(AlphaIn());
    }

    private IEnumerator AlphaIn() // Fade in the player opacity on level start (start == entering play mode)
    {
        float t = 0;
        _particleThing.GetComponent<ParticleSystem>().Stop();
        while (t < 1)
        {
            t += Time.unscaledDeltaTime * 3f; // Takes 0.333 s for the loop to complete
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, t);
            _particleThing.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1f - t);
            yield return null;
        }
        GetComponent<SpriteRenderer>().color = Color.white;
        _particleThing.GetComponent<SpriteRenderer>().color = Color.clear;
        GetComponent<BoxCollider2D>().enabled = true;
        GetComponent<Rigidbody2D>().gravityScale = 1; // Enable gravity only after level is started
        yield return new WaitForSecondsRealtime(0.7f); // Wait an extra 0.7 seconds to ensure all particles have disentegrated
        Destroy(_particleThing.gameObject);
    }
    private IEnumerator AnimateStars(int stars)
    {
        Image[] starImages =
        {
        Star1.GetComponent<Image>(),
        Star2.GetComponent<Image>(),
        Star3.GetComponent<Image>()
    };

        Transform[] starTransforms =
        {
        Star1.transform,
        Star2.transform,
        Star3.transform
    };

        for (int i = 0; i < 3; i++)
        {
            starImages[i].sprite = HollowStar;
            starTransforms[i].localScale = Vector3.zero;
        }

        yield return new WaitForSecondsRealtime(0.25f);

        for (int i = 0; i < 3; i++)
        {
            bool earned = i < stars;

            if (earned)
                starImages[i].sprite = FilledStar;

            float t = 0;

            while (t < 1)
            {
                t += Time.unscaledDeltaTime * 8f;

                float scale = Mathf.LerpUnclamped(
                    0f,
                    1f,
                    Mathf.Sin(t * Mathf.PI * 0.5f) * 1.25f
                );

                starTransforms[i].localScale = Vector3.one * scale;

                yield return null;
            }

            // lil boingg
            t = 0;
            while (t < 1)
            {
                t += Time.unscaledDeltaTime * 10f;

                float scale = Mathf.Lerp(1.25f, 1f, t);

                starTransforms[i].localScale = Vector3.one * scale;

                yield return null;
            }

            starTransforms[i].localScale = Vector3.one;

            yield return new WaitForSecondsRealtime(0.15f);
        }
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
        if (grounded && sinceSwitch == 5 && (GetComponent<Rigidbody2D>().linearVelocity.magnitude < velocityThreshold))
        {
            grounded = false;
            sinceSwitch = 0; return true;
        }
        return false;
    }

    private bool lastFrameGrounded = false;
    private void FixedUpdate()
    {
        if (sinceSwitch < 5) sinceSwitch++;

        // make hit ground vfx
        bool isOnGround = IsGrounded();
        if (isOnGround)
        {
            grounded = true;
            sinceSwitch = 5;
            if (lastFrameGrounded == false) Instantiate(HitGroundVFX_Prefab, transform.position, Level.Instance.camera.transform.rotation, transform);
        }
        lastFrameGrounded = isOnGround;
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Ground"))
        {
            grounded = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Win"))
        {
            // i've made a win tile so ive moved the code from here to TilemapPlayerInterationHandler.cs
        }
    }

    private string FormatTime(float s) // Utiliy function so we can convert float amount of seconds to string
    {
        return s >= 3600 ? $"{(int)s / 3600}h {(int)s / 60 % 60}m {(int)s % 60}s" :
            (s >= 60 ? $"{(int)s / 60}m {(int)s % 60}s" :
                $"{(int)s}s");
    }

    public void NextLevel()
    {
        nextLevel = true;
    }

    public IEnumerator TransitionCoroutine(bool RestartMode) // Called and awaited by GameManager in LoadLevel coroutine
    {
        int stars = 1; // todo: @F1nn display these stars!
        if (GameManager.movesThisLevel <= GameManager.levelData.minMoves) stars++;
        if (GameManager.timeThisLevel <= GameManager.levelData.minTime) stars++;
        stars++;
        Debug.Log($"Level: {GameManager.level}");
        Debug.Log($"Stars length: {GameManager.state.levelStars.Length}");
        Debug.Log($"Moves length: {GameManager.state.levelMoves.Length}");
        Debug.Log($"Times length: {GameManager.state.levelTimes.Length}");

        GameManager.state.levelStars[GameManager.level - 1] = stars;
        GameManager.state.levelMoves[GameManager.level - 1] = GameManager.movesThisLevel;
        GameManager.state.levelTimes[GameManager.level - 1] = GameManager.timeThisLevel;
        // I have no idea why we didnt set this in game manager itself-Ali

        // Transition the box to cover the whole screen
        float t = 0;
        while (t < 50)
        {
            t += Time.unscaledDeltaTime * 30f * (RestartMode ? 5f : 1f);
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.one * 50;

        if (!RestartMode)
        {
            // Deal with the levelCompletePanel only when moving to next level
            // Transition in the levelCompletePanel and show stats
            levelCompletePanel.gameObject.SetActive(true);
            t = 0;
            while (t < 1)
            {
                t += Time.unscaledDeltaTime;
                levelCompletePanel.alpha = t;
                movesText.text = $"Moves: {Mathf.CeilToInt(Mathf.Lerp(0, GameManager.movesThisLevel, t))}";
                timeText.text = $"Time taken: {FormatTime(GameManager.timeThisLevel * t)}";
                yield return AnimateStars(stars);
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
    }

    private IEnumerator TransitionInCoroutine() // Called on level start by player
    {
        // Uncover the screen
        float t = 50;
        while (t > 0)
        {
            t -= Time.unscaledDeltaTime * 30f * (GameManager.levelWon ? 1f : 5f); // Transition is slower for moving to next level, faster for restarting a level
            transition.localScale = Vector3.one * t;
            yield return null;
        }
        transition.localScale = Vector3.zero;
        GameManager.levelReady = true;
    }
}
