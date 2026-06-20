using System.Collections;
using UnityEditor.Search;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public int waveID;
    public Direction direction;
    private static Teleporter[] teleporters;
    public bool showAnnoyingLogs;
    public void AnnoyingLog(string text)
    {
        if (!showAnnoyingLogs) return;
        Debug.Log(text);
    }
    private void Awake()
    {
        teleporters = FindObjectsByType<Teleporter>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AnnoyingLog("bLOCK COLLIDE WITH Something");
        if (!other.CompareTag("Player"))
            return;
        AnnoyingLog("THAT SOMETHIGN WAS A PLAYER");
        Teleport(other);
    }
    private void Teleport(Collider2D player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        foreach (Teleporter tp in teleporters)
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
                case Direction.left:
                    {
                        offset = Vector2.left;
                        break;
                    }
                case Direction.right:
                    {
                        offset = Vector2.right;
                        break;
                    }
                case Direction.up:
                    {
                        offset = Vector2.up;
                        break;
                    }
                case Direction.down:
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
                    Direction.left => Vector2.left * speed,
                    Direction.right => Vector2.right * speed,
                    Direction.up => Vector2.up * speed,
                    Direction.down => Vector2.down * speed,
                    _ => velocity
                };
                rb.linearVelocity = newVelocity;
            }
            GravityHandler gravityHandler = FindAnyObjectByType<GravityHandler>();
            Level level = FindAnyObjectByType<Level>();
            if (gravityHandler != null)
            {
                switch (tp.direction)
                {
                    case Direction.left:
                        {
                            gravityHandler.setGravityLeft(level.gravity);
                            break;
                        }
                    case Direction.right:
                        {
                            gravityHandler.setGravityRight(level.gravity);
                            break;
                        }
                    case Direction.up:
                        {
                            gravityHandler.setGravityUp(level.gravity);
                            break;
                        }
                    case Direction.down:
                        {
                            gravityHandler.setGravityDown(level.gravity);
                            break;
                        }

                }
            }
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

public enum Direction { left, right, up, down };
