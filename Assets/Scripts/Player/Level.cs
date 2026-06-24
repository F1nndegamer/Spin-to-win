using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class Level : GameBehaviour
{
    public Camera camera;
    public GravityHandler gravityHandler;
    public PlayerBox player;
    private bool _rotating = false;
    public float gravity = 20f;
    public float lerpSpeed = 4f;
    [Range(0f, 1f)]
    public float followSpeed = 0.8f;
    private int _unsyncedOffset = 0;
    private Vector3 targetPosition;
    public float accelerativeLerpSpeed = 0.9f;

    public Bounds bounds;

    [System.Serializable]
    public struct Bounds
    {
        public bool isSet;
        public Vector2 topLeft;
        public Vector2 bottomRight;
    }
    // The camera position has to be limited within these bounds, set in the LevelData (access at GameManager.levelData) 
    
    private bool shiftMode = false; // For faster movement, hold shift
    private Vector3Int moveInput = Vector3Int.zero; // Vector representation of movement input
    private float originalCameraSize = 7.89f;
    private float _targetOrthoSize = 7.89f;

    [Header("Mechanic")]
    public bool allowMidairSwitch = false;
    public bool preserveMomentum = false;

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
    void Awake()
    {
        Instance = this;
        targetPosition = camera.transform.position;
        originalCameraSize = _targetOrthoSize = camera.orthographicSize; // get this so we can restore it later
        //CacheLevelScene();
    }

    public override void GameStart()
    {
        bounds.isSet = true;
        bounds.topLeft = (Vector2)GameManager.levelData.topLeftBound.position;
        bounds.bottomRight = (Vector2)GameManager.levelData.bottomRightBound.position;
        targetPosition = new Vector3(GameManager.player.transform.position.x, GameManager.player.transform.position.y,
            targetPosition.z); // Center the camera on player anyways
        // Set new boundary on Level load
        // Note that LevelData instance is set in GameManager in Awake(), and GameStart runs after all Awake() calls have been run
        if (!GameManager.levelData.editingNeeded)
        {
            // exit edit mode
            Confirm();
        }
    }

    private void LateUpdate()
    {
        if (GameManager.levelStarted)
        {
            GameManager.timeThisLevel += Time.unscaledDeltaTime;
            GameManager.totalTimePlayed += Time.unscaledDeltaTime;
            Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, lerpSpeed * Time.deltaTime);
        }
        else
        {
            ManageMovement(); // Allow the pan and zoom kind of input method
        }
    }

    public void PassDirectionalInput(int x, int y, int z) // Passed from Controller, based on WASD and Q E
    {
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, _targetOrthoSize, 0.2f); // this is our zoom factor
        // change it here so we can set _targetOrthoSize to anything to lerp to, even after the edit mode ends
        if (GameManager.levelStarted)
        {
            // prevent spam calling Rotate if we're not pressing a key for it
            // this seems to fix randomly not being able to rotate - Sabrina
            if(x == 0) { return; }
            Rotate(x);
        }
        else
        {
            moveInput = new Vector3Int(x, y, z); // z is for zoom
        }
    }

    private void ManageMovement()
    {
        // camera zoom:
        _targetOrthoSize -= moveInput.z * Time.unscaledDeltaTime * 5f;
        _targetOrthoSize = Mathf.Clamp(_targetOrthoSize, 3f, 11f); // Clamp the zoom

        float speed = GameManager.controller.speed;
        float speedMult = shiftMode ? 3f : 1f;
        Vector3 posAdd = (Vector3)moveInput * (speed * speedMult * Time.unscaledDeltaTime); // unscaledDeltaTime since we call ManageMovement from LateUpdate
        posAdd = camera.transform.TransformDirection(posAdd); // make sure our move directions are always the same no matter how the camera is rotated
        posAdd.z = 0; // make sure we dont actually move our in and our else we could go too far in and stop rendering the level - Sabrina
        // I think what that means is that we dont let the camera move in the z axis because it would not affect anything apart from borking the clipping and maybe hiding everything from view - Ali 
        targetPosition += posAdd;
        targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.topLeft.x, bounds.bottomRight.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, bounds.bottomRight.y, bounds.topLeft.y);
        
        camera.transform.position = Vector3.Lerp(camera.transform.position, targetPosition, accelerativeLerpSpeed);
    }

    public void Rotate(int dir)
    {
        // Only rotate once the player is settled, this adds more flexibility to puzzle and level design - Ali
        // Or does it? - VSauce, Michael
        if (_rotating || (!IsGrounded() && !allowMidairSwitch)) return;
        
        // Increment moves counter
        GameManager.movesThisLevel++;
        GameManager.totalMoves++;

        // Using a coroutine for rotating the camera, otherwise the gravity switches while the camera is rotating; we cant wait for a coroutine to finish in a function -Sabrina
        StartCoroutine(RotateRoutine(dir));
    }

    public void ShiftMode(bool mode)
    {
        shiftMode = mode;
    }

    public void Confirm()
    {
        // this quits edit mode and enters game mode
        GameManager.levelStarted = true;
        GameManager.inventory.gameObject?.SetActive(false);
        _targetOrthoSize = originalCameraSize;
        Debug.Log("Exiting edit mode!");
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
        GameManager.Instance.Restart();
    }
    
    // We no longer use this, tho thanks sir F1nn for having a headache at midnight attempting to reset every little thing :salutecri: - Ali
    [Obsolete]
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

        if (GameManager.inventory.gameObject != null)
        {
            Destroy(GameManager.inventory.gameObject);
            GameManager.levelStarted = true;
            // This will either crash or run once
        }

        GameObject grid = GameObject.Find("Grid");
        if (grid != null)
            Destroy(grid);
        TilemapPlayerInterationHandler.instance.tilemap = null;
        yield return null;

        string sceneName = "Level" + GameManager.level.ToString();

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        loadOp.allowSceneActivation = true;

        yield return loadOp;

        _levelScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(_levelScene);

        yield return null;
        yield return new WaitForEndOfFrame();
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
        // GameManager now handles this
        //currentLevel += 1;
        //Restart();
    }
}
