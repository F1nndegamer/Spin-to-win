using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour
{
    readonly Vector2Int size = Vector2Int.one;
    public /* readonly */ List<Vector2Int> shapeCells = new(); // non-readonly since Teleporter.cs:40 tries to add to it apparently

    private Vector3 startPos;

    private bool isDragging;
    private bool placed;
    private Vector2Int currentCell;
    [HideInInspector] public int CloneAmount = 1;
    private Camera cam;
    private BoxCollider2D collider;
    [HideInInspector] public TextMeshPro text;

    private void Awake()
    {
        shapeCells.Add(new Vector2Int(0, 0));
        foreach (Transform child in transform)
        {
            if (!child.GetComponent<DragChild>()) continue;
            shapeCells.Add(Vector2Int.RoundToInt(child.transform.localPosition));
        }
        collider = GetComponent<BoxCollider2D>();
        cam = Camera.main;
    }
    void Update()
    {

    }
    void Start()
    {
        SetText();
    }
    void SetText()
    {
        if (CloneAmount != 1) text.text = CloneAmount.ToString();
        if (CloneAmount == 1) text.text = "";
    }
    public void OnMouseDown()
    {
        if (!GameManager.levelStarted)
            StartDrag();
    }

    public void OnMouseDrag()
    {
        if (!GameManager.levelStarted)
            Drag();
    }

    public void OnMouseUp()
    {
        EndDrag();
    }

    private void StartDrag()
    {
        if (CloneAmount != 1)
        {
            GameObject obj = Instantiate(gameObject, transform.position, transform.rotation);
            obj.GetComponent<DraggableItem>().CloneAmount--;
            obj.transform.SetParent(GameManager.inventory.objectParent, false);
            obj.transform.position = transform.position;
            SetText();
        }
        CloneAmount = 1;
        startPos = transform.position;

        collider.enabled = false;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BoxCollider2D childCollider))
            {
                childCollider.enabled = false;
            }
        }
        if (GameManager.placedObjects == null) GameManager.placedObjects = GameObject.Find("PlacedObjects").transform;
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
        Transform parent = transform.parent;

        if (parent == null)
        {
            transform.localScale = Vector3.one;
        }
        else
        {
            Vector3 p = parent.lossyScale;
            transform.localScale = new Vector3(
                1f / p.x,
                1f / p.y,
                1f / p.z
            );
        }
        Vector3 mouse = Input.mousePosition;
        if (cam == null)
        {
            cam = Level.Instance.camera;
        }
        mouse.z = Mathf.Abs(cam.transform.position.z - transform.position.z);

        Vector3 world = cam.ScreenToWorldPoint(mouse);
        world.z = transform.position.z;

        transform.position = world;
    }

    private void EndDrag()
    {
        isDragging = false;
        collider.enabled = true;
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out BoxCollider2D childCollider))
            {
                childCollider.enabled = true;
            }
        }
        Vector2Int cell = GridManager.Instance.WorldToCell(transform.position);

        if (GridManager.Instance.CanPlace(cell, this))
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
        transform.parent = GameManager.placedObjects;
        Teleporter t = GetComponent<Teleporter>();
        if (t != null)
        {
            if (!GameManager.teleporters.Contains(t))
            {
                GameManager.teleporters.Add(t);
            }
        }
        GridManager.Instance.Register(this, cell);
    }

    public IEnumerable<Vector2Int> GetOccupiedCells()
    {
        if (shapeCells is { Count: > 0 }) // same as (shapeCells != null && shapeCells.Count > 0) - Ali
        {
            return shapeCells;
        }

        List<Vector2Int> fallbackCells = new();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                fallbackCells.Add(new Vector2Int(x, y));
            }
        }

        return fallbackCells;
    }
}