using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    public Camera currentCamera;

    public void OnPlay()
    {
        playButton.enabled = false;
        playButtonText.text = "Loading...";
        currentCamera.GetComponent<AudioListener>().enabled = false;

        GameManager.Instance.LoadScene(2, 3);
        // Remember that:
        // 0 is the GameManager scene, it is the entry point
        // 1 is the Menu
        // 2 is the EssentialsScene
        // 3 and so on will be levels
    }
}
