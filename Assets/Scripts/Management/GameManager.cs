using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class GameManager : MonoBehaviour
{
    #region Initiation
    // This is the entry point of our game, it will persist throughout the game and manage stuff too
    // The GameManager must always exist!!
    // Always use GameManager.LoadScene instead of SceneManager
    // - Ali
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)  { _instance = FindAnyObjectByType<GameManager>(); }
            return _instance;
        }
    }
    private void Awake()
    {
        Setup();
    }

    private void Setup()
    {
        if (_instance != null && _instance != this)
        {
            if(logLevel >= LogLevel.Warn) Debug.LogWarning("The GameManager was reinitialized due to multiple instances being present");
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        if(transform.parent != null) transform.SetParent(null); // Make sure we are root hehe
        Object.DontDestroyOnLoad(gameObject);
    }

    private void LoadMenu()
    {
        LoadScene(1);
    }

    private void Start()
    {
        // For ppl who like to disable "Reload domain" on play
        levelStarted = false;
        levelReady = true;
        levelLoaded = true;
        teleporters = new List<Teleporter>();
        level = 1;
        movesThisLevel = 0; // idk why but it seems to carry over from last time i played,,, so ill fix it here ig -Sabrina
        timeThisLevel = 0f;
        
        LoadState(); // This is where we load state! Its shrimple as loading like that
        InvokeRepeating(nameof(SaveState), 1f, 10f); // 1 second after this, save the state every 10 seconds
        
        // Load the menu after the Setup is done, only if this scene was opened directly
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Invoke(nameof(LoadMenu), 4.2f);
        }
        else
        {
            // we gonna do something silly here, here goes:
            // The scene was loaded indirectly, update level and currentLevel and unload the scene and execute GameRegistry
            // We cannot use GetActiveScene, as it is almost always the EssentialScene, not any Level* scene.
            // Level* scenes will be loaded after the EssentialScene, thereby at index 1.
            // If the Level* scene is not at index 1, try find the scene in all loaded scenes
            // and if none of the scenes is a level scene, not a very sensible man has pulled this repo to test
            Scene levelScene = SceneManager.GetSceneAt(1);
            bool foundLevel = false;
            if (!levelScene.name.StartsWith("Level"))
            {
                int t = SceneManager.loadedSceneCount;
                for (int i = 0; i < t; i++)
                {
                    if (i == 1) continue;
                    levelScene = SceneManager.GetSceneAt(i);
                    if (levelScene.name.StartsWith("Level"))
                    {
                        foundLevel = true;
                        goto SetLevel;
                    }
                }
                if(logLevel >= LogLevel.Warn) Debug.LogWarning("Menu?");
            }
            SetLevel: // Labels are awesome. - Ali
            if (foundLevel)
            {
                level = System.Int32.Parse(levelScene.name.Replace("Level",
                    "")); // Parse level number from current scene's name
                currentLevel = levelScene;
            }
            else
            {
                level = -1; //  We prolly in the menu! lol
            }

            SceneManager.UnloadSceneAsync(0); 
            GameRegistry.Execute();
        }
    }
    #endregion

    #region Loading
    // 4 overloads for the LoadScene function for convenience!
    public void LoadScene(int sceneIndex)
    { StartCoroutine(LoadSceneCoroutine(sceneIndex, -1)); }

    public void LoadScene(int sceneIndex, int additiveSceneIndex)
    { StartCoroutine(LoadSceneCoroutine(sceneIndex, additiveSceneIndex)); }
    
    public void LoadScene(string sceneName) 
    { StartCoroutine(LoadSceneCoroutine(SceneManager.GetSceneByName(sceneName).buildIndex, -1)); }
    
    public void LoadScene(string sceneName, string additiveSceneName) 
    { StartCoroutine(LoadSceneCoroutine(SceneManager.GetSceneByName(sceneName).buildIndex, SceneManager.GetSceneByName(additiveSceneName).buildIndex)); }
    
    private IEnumerator LoadSceneCoroutine (int sceneIndex, int additiveSceneIndex)
    {
        levelLoaded = false;
        levelReady = false;
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        if (loadScene != null)
        {
            while (!loadScene.isDone) yield return null;
            if (additiveSceneIndex != -1) // Load extra scene only when needed
            {
                // The current level scene
                AsyncOperation loadAdditiveScene =
                    SceneManager.LoadSceneAsync(additiveSceneIndex, LoadSceneMode.Additive);
                if (loadAdditiveScene != null)
                {
                    while (!loadAdditiveScene.isDone) yield return null;

                    // set the current level AFTER we have loaded the scene because if we try to do so before it returns a Scene object that has empty fields!
                    if (additiveSceneIndex >= 3) currentLevel = SceneManager.GetSceneByBuildIndex(additiveSceneIndex);
                } else { if(logLevel >= LogLevel.Error) Debug.LogError("Failed to load additive scene"); }
            }

            Scene baseScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
            if (baseScene.IsValid())
            {
                SceneManager.SetActiveScene(baseScene);
            }

            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(activeSceneIndex));
            levelStarted = false; // Set this here coz yea
            GameRegistry.Execute();
            levelLoaded = true;
        }
        else { if (logLevel >= LogLevel.Error) Debug.LogError("Failed to load scene"); }
    }
    #endregion

    #region LevelLoading

    public void LoadLevel(int levelIndex)
    {
        teleporters = new List<Teleporter>(); // Flush teleporter list
        int sceneIndex = levelIndex + 2; // Levels start at index 3, so level 1 is index 3
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneName.ToLower().StartsWith("level")) // Check if next scene is a level
            {
                StartCoroutine(LoadLevelCoroutine(sceneIndex));
            }
            else
            {
                StartCoroutine(LastLevelCompleted(sceneIndex)); // The next scene is probably a credits scene or smth, load it
            }
            return;
        }

        if(logLevel >= LogLevel.Error) Debug.LogError("Next scene not found!");
    }

    private IEnumerator LastLevelCompleted(int sceneIndex) // This loads them credits
    {
        yield return StartCoroutine(player.TransitionCoroutine(false));
        Level.Instance.GetComponent<AudioListener>().enabled = false; // Only disable the AudioListener, there is no EventSystem in credits scene
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(1)); // Unload level scene before doing anything
        LoadScene(sceneIndex);
    }

    private IEnumerator LoadLevelCoroutine(int sceneIndex) // Different from LoadSceneCoroutine as we are not unloading the active scene
    {
        levelLoaded = false;
        levelReady = false;
        //GameObject grid = GameObject.Find("Grid");
        //if (grid != null)
        //    Destroy(grid);
        yield return StartCoroutine(player.TransitionCoroutine(level + 2 == sceneIndex)); // Transition is different for restarting and moving to next level
        AsyncOperation nextLevel = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        if (nextLevel != null)
        {
            while (!nextLevel.isDone) yield return null;
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentLevel);
            if (unload != null)
            {
                while (!unload.isDone) yield return null;
            } else { if(logLevel >= LogLevel.Warn) Debug.LogWarning("Unload failed"); }
            currentLevel =
                SceneManager.GetSceneByBuildIndex(sceneIndex); // we have to update currentLevel after loading the level
            inventory.gameObject.SetActive(true);
            levelStarted = false;
            level = sceneIndex - 2; // update level at the end here, for various reasons - Ali
            timeThisLevel = 0f;
            movesThisLevel = 0;
            GameRegistry.Execute();
            levelLoaded = true;
        } else { if(logLevel >= LogLevel.Error) Debug.LogError("Load failed"); }
    }

    public void Win()
    {
        if (!levelLoaded) return;
        // Dont win or oad the next scene twice
        levelWon = true;
        state.wins++;
        LoadLevel(level + 1);
    }

    public void Lose()
    {
        if(levelReady){ // Only allow restart when level is fully loaded
            levelWon = false;
            state.lost++;
            LoadLevel(level); // Load the current level again == Reload level
        }
        else
        {
            if(logLevel >= LogLevel.Error) Debug.LogError("Just");
            if (logLevel >= LogLevel.Warn)
            {
                Debug.LogWarning("how");
                Debug.LogWarning("did");
                Debug.LogWarning("we");
                Debug.LogWarning("get");
                Debug.LogWarning("here?");
                // - Ali
            }
        }
    }

    public void Restart()
    {
        if (!levelReady) return;
        // Only allow restart when level is fully loaded
        levelWon = false;
        state.retries++;
        LoadLevel(level); // Load the current level again == Reload level
    }

    #endregion

    #region State

    public int totalLevels = 10;

    private static int _totalLevels
    {
        get => Instance.totalLevels;
    }
    
    public static PlayerBox player;
    public static Inventory inventory;
    public static LevelData levelData;
    public static Scene currentLevel;
    public static Controller controller;
    public static List<Teleporter> teleporters = new List<Teleporter>();
    public static Transform placedObjects;
    
    public static int level = 1; // Just go 1 to N, so we can reorder levels easily through build settings
    public static bool levelLoaded; // Level is initiated and GameAwake and GameReady have all been called
    public static bool levelReady; // The entry transition has been completed
    public static bool levelStarted; // The user has changed gravity at least once
    public static int movesThisLevel;
    public static float timeThisLevel;
    public static bool levelWon = true; // Ignore the naming but, this variable is meant to store whether we reached the current level by winning or by restarting
    public static bool stateLoaded = false;

    public static Save state;
    
    [System.Serializable]
    public struct Save
    {
        public bool postProcessing;
        public float volume;

        public int nextLevel;
        public float totalTime;
        public int totalMoves;
        public int wins, retries, lost;
        public string username;
        [SerializeField] private long lastPlayedTicks;
        // Save lastPlayed as a long, return it as a DateTime
        public DateTime lastPlayed
        {
            get => new DateTime(lastPlayedTicks);
            set => lastPlayedTicks = value.Ticks;
        }

        public bool[] levelsCompleted;
        public int[] levelMoves;
        public int[] levelTimes;
        public int[] levelStars;
        public int[] levelAttempts;
    }

    private void SaveState()
    {
        state.lastPlayed = DateTime.Now;
        string stateJson = JsonUtility.ToJson(state);
        if (PlayerPrefs.GetString("state") == stateJson) return; // We do this to avoid certain crash situations, at the cost of mild performance
        PlayerPrefs.SetString("state", stateJson);
        PlayerPrefs.Save();
    }

    public void LoadState()
    {
        Initialize:
        if (!PlayerPrefs.HasKey("state"))
        {
            state = new Save
            {
                postProcessing = true, volume = 1f, nextLevel = 1, totalMoves = 0, totalTime = 0f, username = "Player1", 
                wins = 0, retries = 0, lost = 0,
                lastPlayed = DateTime.Now, levelsCompleted = new bool[_totalLevels], levelAttempts = new int[_totalLevels],
                levelMoves = new int[_totalLevels], levelTimes = new int[_totalLevels], levelStars = new int[_totalLevels]
            };
            // Initially, fill with 0, false, or default values
            StateToVars();
            stateLoaded = true;
            return;
        }
        string json = PlayerPrefs.GetString("state");
        try
        {
            state = JsonUtility.FromJson<Save>(json);
        }
        catch (System.Exception)
        {
            PlayerPrefs.DeleteKey("state"); // If the JSON is corrupted, clear it out and start fresh
            goto Initialize;
        }

        StateToVars();
        stateLoaded = true;
    }

    private void StateToVars()
    {
        level = state.nextLevel;
        
    }

    #endregion

    #region Logging 
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warn = 2,
        Info = 3
    }

    public static LogLevel logLevel
    {
        get
        {
            return Instance._logLevel;
        }
    }
    public LogLevel _logLevel = LogLevel.Info;

    #endregion
}
