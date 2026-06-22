using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Initiation
    // This is the entry point of our game, it will persist throughout the game and manage stuff too
    // The GameManager must always exist
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
        // Load the menu after the Setup is done, only if this scene was opened directly
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            LoadScene(1);
        }
        else
        {
            // The scene was loaded indirectly, only unload the scene and execute GameRegistry
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
        levelReady = false;
        levelStarted = false;
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        while (!loadScene.isDone) yield return null;
        if (additiveSceneIndex != -1) // Load extra scene only when needed
        {
            if(additiveSceneIndex >= 3) currentLevel = SceneManager.GetSceneByBuildIndex(additiveSceneIndex);
            // The current level scene
            AsyncOperation loadAdditiveScene = SceneManager.LoadSceneAsync(additiveSceneIndex, LoadSceneMode.Additive);
            while (!loadAdditiveScene.isDone) yield return null;
        }
        Scene baseScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (baseScene.IsValid())
        {
            SceneManager.SetActiveScene(baseScene);
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(activeSceneIndex)); 
        GameRegistry.Execute();
        levelReady = true;
    }
    #endregion

    #region LevelLoading

    public void LoadLevel(int levelIndex)
    {
        int sceneIndex = levelIndex + 2; // Levels start at index 3, so level 1 is index 3
        if (SceneManager.GetSceneByBuildIndex(sceneIndex) != null) // Check if next scene exists
        {
            if (SceneManager.GetSceneByBuildIndex(sceneIndex).name.ToLower().StartsWith("level")) // Check if next scene is a level
            {
                StartCoroutine(LoadLevelCoroutine(sceneIndex));
            }
            else
            {
                LoadScene(sceneIndex); // The next scene is prolly a credits scene or smth, load it
            }
            return;
        }
        Debug.LogError("Next scene not found!");
    }

    private IEnumerator LoadLevelCoroutine(int sceneIndex) // Different from LoadSceneCoroutine as we are not unloading the active scene
    {
        levelReady = false;
        AsyncOperation nextLevel = SceneManager.LoadSceneAsync(sceneIndex);
        while (!nextLevel.isDone) yield return null;
        AsyncOperation unload = SceneManager.UnloadSceneAsync(currentLevel);
        while (!unload.isDone) yield return null;
        GameRegistry.Execute();
        levelReady = true;
    }

    public void Win()
    {
        if (!levelReady) return; 
        // Dont win or oad the next scene twice
        levelReady = false;
        levelStarted = false;
        teleporters = new List<Teleporter>(); // Flush teleporter list
        level++;
        LoadLevel(level);
    }

    #endregion

    #region State

    public static int level = -1;
    public static Inventory inventory;
    public static LevelData levelData;
    public static Scene currentLevel;
    public static bool levelReady = false; // Level is initiated and GameAwake and GameReady have all been called
    public static bool levelStarted = false; // The user has changed gravity at least once
    public static List<Teleporter> teleporters = new List<Teleporter>();

    #endregion
}
