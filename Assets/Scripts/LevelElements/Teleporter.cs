using System.Collections;
using UnityEngine;

public class Teleporter : GameBehaviour
{
    public int waveID;
    public Level.Direction direction;
    public bool showAnnoyingLogs;

    public void AnnoyingLog(string text)
    {
        if (!showAnnoyingLogs) return;
        Debug.Log(text);
    }

    public override void GameAwake() // Use GameAwake instead of Awake so that we dont crash out when game is started without loading from menu
    {
        GameManager.teleporters.Add(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!GameManager.levelStarted) return;
        AnnoyingLog("bLOCK COLLIDE WITH Something");
        if (!other.CompareTag("Player"))
            return;
        AnnoyingLog("THAT SOMETHIGN WAS A PLAYER");
        Teleport(other);
    }
    private void Teleport(Collider2D player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

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
            ;

            AnnoyingLog("tpdirection set!!");
            player.transform.position = tp.transform.parent.position + (Vector3)offset;
            Vector2 newVelocity = velocity;
            if (rb != null)
            {
                float speed = velocity.magnitude;

                newVelocity = tp.direction switch
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
            level.Rotate((Level.Direction)tp.direction);
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
