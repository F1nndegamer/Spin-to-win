using System.Collections;
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
        LoadScene(1);
        // Load the menu after the Setup is done
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
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
        AsyncOperation loadScene = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Additive);
        while (!loadScene.isDone) yield return null;
        if (additiveSceneIndex != -1) // Load extra scene only when needed
        {
            AsyncOperation loadAdditiveScene = SceneManager.LoadSceneAsync(additiveSceneIndex, LoadSceneMode.Additive);
            while (!loadAdditiveScene.isDone) yield return null;
        }
        Scene baseScene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        if (baseScene.IsValid())
        {
            SceneManager.SetActiveScene(baseScene);
        }
        SceneManager.UnloadSceneAsync(SceneManager.GetSceneByBuildIndex(activeSceneIndex)); 
    }
    #endregion
}
