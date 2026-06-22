using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Level : GameBehaviour
{
    public Camera camera;
    public GravityHandler gravityHandler;
    public bool startedLevel;
    public PlayerBox player;
    private bool _rotating = false;
    public float gravity = 9.8f;
    public float lerpSpeed = 3f;
    public int currentLevel = 0;
    [Range(0f, 1f)]
    public float followSpeed = 0.5f;
    private int _unsyncedOffset = 0;

    [Header("Mechanic")]
    public bool allowMidairSwitch = false;
    public bool preserveMomentum = false;

    private GameObject _inventory;
    public static Level Instance;
    private Scene _levelScene;
    public enum Direction
    {
        Underflow = -1, // for some weird reason `-1 % 4 = -1` So i'll just add a case for this and manually correct it -Sabrina

        Down = 0,
        Right = 1,
        Up = 2,
        Left = 3
    };

    private Direction gravityDirection;
    public override void GameAwake()
    {
        _inventory = GameManager.inventory.gameObject;
    }
    void Awake()
    {
        Instance = this;
        //CacheLevelScene();
    }
    private void LateUpdate()
    {
        Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, target, lerpSpeed * Time.deltaTime);
    }

    public void Rotate(int dir)
    {
        startedLevel = true;
        if (_inventory == null)
        {
            _inventory = GameManager.inventory.gameObject;
        }
        _inventory?.SetActive(false);
        // Only rotate once the player is settled, this adds more flexibility to puzzle and level design - Ali
        // Or does it? - VSauce, Michael
        if (_rotating || (!IsGrounded() || allowMidairSwitch)) return;

        // Using a coroutine for rotating the camera, otherwise the gravity switches while the camera is rotating; we cant wait for a coroutine to finish in a function -Sabrina
        StartCoroutine(RotateRoutine(dir));
    }

    public void Rotate(Direction targetDirection)
    {
        int currentDir = (int)gravityDirection;
        int targetDir = (int)targetDirection;
        int offset = (targetDir - currentDir + _unsyncedOffset) % 4;
        if (offset < 0) offset = 4 + offset;
        StartCoroutine(RotateRoutine(offset));
    }

    IEnumerator RotateRoutine(int dir)
    {
        gravityDirection = (Direction)(((int)gravityDirection + dir - _unsyncedOffset) % 4);
        if (gravityDirection == Direction.Underflow) { gravityDirection = Direction.Left; }
        if (!preserveMomentum) player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; // Momentum be set to zero
        _unsyncedOffset = 0;
        SetPhysicsEnabled(false);
        if (!player.WillTeleport(gravityDirection)) // Do not coroutine when the player teleports next step, instead, initiate the coroutine when player teleports 
        {
            StartCoroutine(RotateCamera(dir)); // wait for the rotate coroutine to finish before changing gravity -Sabrina
            yield return new WaitForSecondsRealtime(0.1f); // needs to be fixed in order to not collide with the teleport coroutine - Ali
        }
        else
        {
            _unsyncedOffset += dir;
        }
        UpdateGravity();
        SetPhysicsEnabled(true);
    }


    float lerpAngleFormula(float t)
    {
        // if we want a bezier curve, or ease in/out we can modify our formula/return here -Sabrina
        // Use quadratic ease out MWHAHWHAWHH - Ali
        return t * (2f - t);
    }
    IEnumerator RotateCamera(int dir)
    {
        _rotating = true;

        Vector3 startingRotation = camera.transform.rotation.eulerAngles;
        float endRotationZ = startingRotation.z + (90f * dir);
        endRotationZ = Mathf.Round(endRotationZ / 90f) * 90f; // Make sure it snaps to nearest 90
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * lerpSpeed;
            float newZAngle = Mathf.LerpAngle(startingRotation.z, endRotationZ, lerpAngleFormula(t));
            camera.transform.rotation = Quaternion.Euler(0, 0, newZAngle);
            yield return null;
        }
        camera.transform.rotation = Quaternion.Euler(0, 0, endRotationZ);

        _rotating = false;
        yield return null;
    }

    public void UpdateGravity()
    {
        switch (gravityDirection)
        {
            case Direction.Down:
                gravityHandler.setGravityDown(gravity);
                break;
            case Direction.Right:
                gravityHandler.setGravityRight(gravity);
                break;
            case Direction.Up:
                gravityHandler.setGravityUp(gravity);
                break;
            case Direction.Left:
                gravityHandler.setGravityLeft(gravity);
                break;
        }
    }

    private bool IsGrounded()
    {
        return player.IsGrounded();
    }

    private void SetPhysicsEnabled(bool enabled)
    {
        SimulationMode2D mode = enabled ? SimulationMode2D.FixedUpdate : SimulationMode2D.Script;
        Physics2D.simulationMode = mode;
    }
    private void CacheLevelScene()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.name.StartsWith("Level"))
            {
                _levelScene = scene;
                SceneManager.SetActiveScene(scene);
                return;
            }
        }

        Debug.LogWarning("No level scene found at startup!");
    }

    // Implement later after the rest of game is setup
    public void Pause() { }
    public void Restart()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        Debug.Log("Restarting...");
        Rotate((Level.Direction)0);
        
        yield return null;

        if (_levelScene.IsValid())
        {
            yield return SceneManager.UnloadSceneAsync(_levelScene);
        }

        yield return null;

        if (_inventory != null)
            Destroy(_inventory);

        GameObject grid = GameObject.Find("Grid");
        if (grid != null)
            Destroy(grid);
        TilemapPlayerInterationHandler.instance.tilemap = null;
        yield return null;

        string sceneName = "Level" + currentLevel.ToString();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;

        yield return loadOp;

        _levelScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(_levelScene);

        yield return null;
        yield return new WaitForEndOfFrame();
        _inventory = GameManager.inventory.gameObject;
        TilemapPlayerInterationHandler.instance.tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>();

        GameObject dataObj = GameObject.Find("LevelData");
        LevelData data = dataObj.GetComponent<LevelData>();

        if (data == null)
        {
            Debug.LogError("LevelData component missing on LevelData object!");
            yield break;
        }

        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
            yield break;
        }

        player.transform.position = data.PlayerSpawnPos.position;
    }
    public void NextLevel()
    {
        currentLevel += 1;
        Restart();
    }
}
