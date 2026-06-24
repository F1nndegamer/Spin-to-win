using UnityEngine;

public class GameBehaviour : MonoBehaviour
{
    public virtual void GameAwake() {}
    public virtual void GameStart() {}

    protected virtual void OnEnable()
    {
        GameRegistry.Register(this); // GameRegistry tracks the currently available GameBehaviour objects !!
    }

    protected void OnDisable()
    {
        GameRegistry.Unregister(this);
    }
}
