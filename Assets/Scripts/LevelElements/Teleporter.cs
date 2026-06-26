using System.Collections;
using UnityEngine;

public class Teleporter : GameBehaviour
{
    public int waveID;
    public Level.Direction direction;
    public bool showAnnoyingLogs;

    private void AnnoyingLog(string text)
    {
        if (!showAnnoyingLogs) return;
        if(GameManager.logLevel >= GameManager.LogLevel.Info) Debug.Log(text);
    }

    public override void GameAwake() // Use GameAwake instead of Awake so that we dont crash out when game is started without loading from menu
    {
        GameManager.teleporters.Add(this);
    }
    private void Start()
    {
        if(!GameManager.teleporters.Contains(this)) GameManager.teleporters.Add(this); // This teleporter was initiated mid-game, register it at GameManager
        Vector3 targetpos = direction switch
        {
            Level.Direction.Right => Vector3.left,
            Level.Direction.Left => Vector3.right,
            Level.Direction.Up => Vector3.down,
            Level.Direction.Down => Vector3.up,
            _ => Vector3.zero
        };
        DraggableItem item = transform.parent.GetComponent<DraggableItem>();
        if (item == null)
        {
            Debug.Log("uhhhhh");
        }
        else
        {
            Debug.Log("ahh");
            Debug.Log(direction);
            Debug.Log(targetpos);
            Debug.Log(Vector2Int.RoundToInt(targetpos));
        }
        transform.parent.GetComponent<DraggableItem>()?.shapeCells.Add(Vector2Int.RoundToInt(-targetpos));
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.levelStarted)
        {
            AnnoyingLog("Haha cant teleport yet!");
            return;
        }
        AnnoyingLog("bLOCK COLLIDE WITH Something");
        if (!other.CompareTag("Player"))
            return;
        AnnoyingLog("THAT SOMETHIGN WAS A PLAYER");
        Teleport(other);
    }
    private void Teleport(Collider2D player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        AnnoyingLog(GameManager.teleporters.Count.ToString() + " teleporters available");
        foreach (Teleporter tp in GameManager.teleporters) // Use gamemanager to keep track of teleporters
        {
            if (tp == this)
                continue;
            AnnoyingLog("NO INTERACT WITH SELF");
            if (tp.waveID != waveID)
                continue;
            AnnoyingLog("WOW SAME WAVE ID");
            Vector2 velocity = rb != null ? rb.linearVelocity : Vector2.zero;

            Vector2 offset = Vector2.zero;
            switch (tp.direction)
            {
                case Level.Direction.Left:
                    {
                        offset = Vector2.left;
                        break;
                    }
                case Level.Direction.Right:
                    {
                        offset = Vector2.right;
                        break;
                    }
                case Level.Direction.Up:
                    {
                        offset = Vector2.up;
                        break;
                    }
                case Level.Direction.Down:
                    {
                        offset = Vector2.down;
                        break;
                    }
            }

            AnnoyingLog("tpdirection set!!");
            player.transform.position = tp.transform.parent.position + (Vector3)offset;
            if (rb != null)
            {
                float speed = velocity.magnitude;
                Vector2 newVelocity = tp.direction switch
                {
                    Level.Direction.Left => Vector2.left * speed,
                    Level.Direction.Right => Vector2.right * speed,
                    Level.Direction.Up => Vector2.up * speed,
                    Level.Direction.Down => Vector2.down * speed,
                    _ => velocity
                };
                rb.linearVelocity = newVelocity;
            }
            Level level = Level.Instance;
            level.Rotate(tp.direction);
            tp.StartCoroutine(tp.TeleportCooldown(player));

            break;
        }
    }

    private IEnumerator TeleportCooldown(Collider2D player)
    {
        Physics2D.IgnoreCollision(
            player.GetComponent<Collider2D>(),
            GetComponent<Collider2D>(),
            true);
        yield return new WaitForSeconds(0.2f);

        Physics2D.IgnoreCollision(
            player.GetComponent<Collider2D>(),
            GetComponent<Collider2D>(),
            false);
    }
}
