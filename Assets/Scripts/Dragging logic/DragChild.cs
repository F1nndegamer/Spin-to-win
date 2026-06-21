using UnityEngine;

public class DragChild : MonoBehaviour
{
    private DraggableItem item;

    private void Awake()
    {
        item = GetComponentInParent<DraggableItem>();
    }

    void OnMouseDown()
    {
        item?.OnMouseDown();
    }

    void OnMouseDrag()
    {
        item?.OnMouseDrag();
    }

    void OnMouseUp()
    {
        item?.OnMouseUp();
    }
}
