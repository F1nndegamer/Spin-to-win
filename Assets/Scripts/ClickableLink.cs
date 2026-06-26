using UnityEngine;
using UnityEngine.Events;

public class ClickableLink : MonoBehaviour
{
    public UnityEvent action;
    private void OnMouseDown()
    {
        action?.Invoke();
    }
}
