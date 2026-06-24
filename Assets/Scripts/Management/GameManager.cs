using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Debug.LogWarning("The GameManager was reinitialized due to multiple instances being present");
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        if(transform.parent != null) transform.SetParent(null); // Make sure we are root hehe
        Object.DontDestroyOnLoad(gameObject);
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
        totalMoves = 0;
        timeThisLevel = totalTimePlayed = 0f;
        
        // Load the menu after the Setup is done, only if this scene was opened directly
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            LoadScene(1);
        }
        else
        {
            // The scene was loaded indirectly, update level and currentLevel and unload the scene and execute GameRegistry
            level = System.Int32.Parse(SceneManager.GetSceneAt(1).name.Replace("Level", ""));
            currentLevel = SceneManager.GetSceneAt(1);
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
        while (!loadScene.isDone) yield return null;
        if (additiveSceneIndex != -1) // Load extra scene only when needed
        {
            // The current level scene
            AsyncOperation loadAdditiveScene = SceneManager.LoadSceneAsync(additiveSceneIndex, LoadSceneMode.Additive);
            while (!loadAdditiveScene.isDone) yield return null;

            // set the current level AFTER we have loaded the scene because if we try to do so before it returns a Scene object that has empty fields!
            if (additiveSceneIndex >= 3) currentLevel = SceneManager.GetSceneByBuildIndex(additiveSceneIndex);
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
                LoadScene(sceneIndex); // The next scene is probably a credits scene or smth, load it
            }
            return;
        }

        Debug.LogError("Next scene not found!");
    }

    private IEnumerator LoadLevelCoroutine(int sceneIndex) // Different from LoadSceneCoroutine as we are not unloading the active scene
    {
        levelLoaded = false;
        levelReady = false;
        if (level + 2 != sceneIndex)
        {
            yield return StartCoroutine(player.TransitionCoroutine()); // Transition before "next" loading level
        }

        AsyncOperation nextLevel = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        while (!nextLevel.isDone) yield return null;
        AsyncOperation unload = SceneManager.UnloadSceneAsync(currentLevel);
        while (!unload.isDone) yield return null;
        currentLevel = SceneManager.GetSceneByBuildIndex(sceneIndex); // we have to update currentLevel after loading the level
        levelStarted = false;
        level = sceneIndex - 2; // update level at the end here, for various reasons - Ali
        timeThisLevel = 0f;
        movesThisLevel = 0;
        GameRegistry.Execute();
        levelLoaded = true;
    }

    public void Win()
    {
        if (!levelLoaded) return;
        // Dont win or oad the next scene twice
        LoadLevel(level + 1);
    }

    #endregion

    #region State

    public static PlayerBox player;
    public static Inventory inventory;
    public static LevelData levelData;
    public static Scene currentLevel;
    public static Controller controller;
    public static List<Teleporter> teleporters = new List<Teleporter>();
    
    public static int level = 1; // Just go 1 to N, so we can reorder levels easily through build settings
    public static bool levelLoaded = false; // Level is initiated and GameAwake and GameReady have all been called
    public static bool levelReady = false; // The entry transition has been completed
    public static bool levelStarted = false; // The user has changed gravity at least once
    public static int movesThisLevel = 0;
    public static float timeThisLevel = 0f;
    public static int totalMoves = 0;
    public static float totalTimePlayed = 0f;

    #endregion
}
