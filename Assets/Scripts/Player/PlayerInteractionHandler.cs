using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{

    public void onSpikeInteration()
    {
        GameManager.Instance.LoseAndRestart();
    }

    public void onHoleInteration()
    {
        GameManager.Instance.LoseAndRestart();
        // trigger game over, unless a hole will do something else or we just don't use it
    }
}
