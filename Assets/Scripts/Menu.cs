using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    // MWAAHAHHAHAH
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    public Camera currentCamera;

    public void OnPlay()
    {
        playButton.enabled = false;
        playButtonText.text = "Loading...";
        if(GameManager.logLevel >= GameManager.LogLevel.Info) Debug.Log("Meow");
        currentCamera.GetComponent<AudioListener>().enabled = false;
        EventSystem.current.enabled = false; // Disable menu event system so we dont get that "2 event systems" bug

        GameManager.Instance.LoadScene(2, 3);
        // Remember that:
        // 0 is the GameManager scene, it is the entry point
        // 1 is the Menu
        // 2 is the EssentialsScene
        // 3 and so on will be levels
    }
}
