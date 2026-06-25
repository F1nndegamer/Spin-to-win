#if UNITY_EDITOR
// Only works in the editor, do not compile to build
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
public class ThisScriptLoadsTheGameManagerInCaseYouLoadAnySceneOtherThanSceneIndex0AtStart : MonoBehaviour
{
    void Awake()
    {
        if (GameManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log("[Loading GameManager from within scene]");
        bool hasLevelLoaded = Enumerable.Range(0, SceneManager.sceneCount)
    .Select(SceneManager.GetSceneAt)
    .Any(s => s.buildIndex >= 3);

        SceneManager.LoadScene(
            0,
            hasLevelLoaded ? LoadSceneMode.Additive : LoadSceneMode.Single
        );
        Destroy(gameObject);
    }
}
#endif
