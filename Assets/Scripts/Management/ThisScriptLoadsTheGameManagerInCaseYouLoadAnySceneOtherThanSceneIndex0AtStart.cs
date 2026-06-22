#if UNITY_EDITOR
// Only works in the editor, do not compile to build
using UnityEngine;
using UnityEngine.SceneManagement;
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
        SceneManager.LoadScene(0, LoadSceneMode.Additive);
        Destroy(gameObject);
    }
}
#endif
