using UnityEngine;

public class DraggableItem : MonoBehaviour
{
    public Vector2Int size = Vector2Int.one;

    private Vector3 startPos;
    private Transform startParent;

    private bool isDragging;
    private bool placed;
    private Vector2Int currentCell;

    private Camera cam;
    private BoxCollider2D collider;
    public GameObject PlacedParent;

    private void Awake()
    {
        collider = GetComponent<BoxCollider2D>();
        cam = Camera.main;
        if (PlacedParent == null)
        {
            PlacedParent = GameObject.Find("PlacedObjects");
        }
    }

    private void OnMouseDown()
    {
        StartDrag();
    }

    private void OnMouseDrag()
    {
        if(!Level.Instance.StartedLevel)
        Drag();
    }

    private void OnMouseUp()
    {
        EndDrag();
    }

    private void StartDrag()
    {
        startPos = transform.position;
        startParent = transform.parent;

        collider.enabled = false;

        isDragging = true;

        if (placed)
        {
            GridManager.Instance.Unregister(this, currentCell);
            placed = false;
        }
    }

    private void Drag()
    {
        if (!isDragging) return;

        Vector3 mouse = Input.mousePosition;
        mouse.z = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        transform.position = world;
    }

    private void EndDrag()
    {
        isDragging = false;
        collider.enabled = true;
        Vector2Int cell = GridManager.Instance.WorldToCell(transform.position);

        if (GridManager.Instance.CanPlace(cell, size))
        {
            Place(cell);
        }
        else
        {
            transform.position = startPos;
        }
    }

    private void Place(Vector2Int cell)
    {
        placed = true;
        currentCell = cell;

        transform.position = GridManager.Instance.CellToWorld(cell);
        transform.parent = PlacedParent.transform;
        GridManager.Instance.Register(this, cell);
    }
}