using System.Collections.Generic;

// U prolly dont have to deal with this one - Ali
// GameRegistry keeps track of currently available GameBehaviours, and calls GameStart and GameAwake on them
// These two functions are called when the scene and additive scene are completely loaded, to avoid conflict in search!

public static class GameRegistry
{
    private static readonly List<GameBehaviour> ActiveObjects = new List<GameBehaviour>();

    public static bool executed = false;

    public static void Register(GameBehaviour obj)
    {
        ActiveObjects.Add(obj);
        if (executed)
        {
            obj.GameAwake();
            obj.GameStart();
        }
    }

    public static void Unregister(GameBehaviour obj) => ActiveObjects.Remove(obj);

    public static void Execute()
    {
        if (!GameManager.stateLoaded) GameManager.Instance.LoadState();
        List<GameBehaviour> objectsToProcess = new List<GameBehaviour>(ActiveObjects);

        foreach (GameBehaviour obj in objectsToProcess)
        {
            if(obj != null) obj.GameAwake(); // This runs independent of object being inactive
        }

        foreach (GameBehaviour obj in objectsToProcess)
        {
            if(obj != null) obj.GameStart();
        }

        executed = true;
    }
}
