using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{

    public void onSpikeInteration()
    {
        AudioManager.Instance.playDeathSFX();
        GameManager.Instance.Lose();
    }

    public void onHoleInteration()
    {
        AudioManager.Instance.playDeathSFX();
        GameManager.Instance.Lose();
        // trigger game over, unless a hole will do something else or we just don't use it
    }
}
