using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector2Int size = Vector2Int.one;

    private RectTransform rect;
    private Canvas canvas;
    private CanvasGroup group;

    private Vector3 startPos;
    private Transform startParent;

    private bool placed;
    private Vector2Int currentCell;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();

        if (!group)
            group = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = rect.position;
        startParent = transform.parent;

        transform.SetParent(canvas.transform);
        group.blocksRaycasts = false;

        rect.localScale = Vector3.one * 1.1f;

        if (placed)
        {
            GridManager.Instance.Unregister(this, currentCell);
            placed = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rect.position += (Vector3)eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        group.blocksRaycasts = true;
        rect.localScale = Vector3.one;

        Vector2Int cell = GridManager.Instance.WorldToCell(rect.position);

        if (GridManager.Instance.CanPlace(cell, size))
        {
            Place(cell);
        }
        else
        {
            rect.position = startPos;
        }
    }

    private void Place(Vector2Int cell)
    {
        placed = true;
        currentCell = cell;

        rect.position = GridManager.Instance.CellToWorld(cell);

        GridManager.Instance.Register(this, cell);
    }
}