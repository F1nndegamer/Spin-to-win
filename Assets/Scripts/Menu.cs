using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Menu : GameBehaviour
{
    // MWAAHAHHAHAH
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    public Camera currentCamera;
    public Slider volume;
    public Toggle postProcessing;
    public Image fade;

    private void Awake()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.deltaTime * 2;
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, t);
            yield return null;
            
        }
        fade.color = Color.clear;
        fade.gameObject.SetActive(false);
    }
    
    public void OnPlay()
    {
        playButton.enabled = false;
        playButtonText.text = "Loading...";
        if(GameManager.logLevel >= GameManager.LogLevel.Info) Debug.Log("Meow");
        currentCamera.GetComponent<AudioListener>().enabled = false;
        EventSystem.current.enabled = false; // Disable menu event system so we dont get that "2 event systems" bug

        GameManager.Instance.LoadScene(2, 2 + GameManager.level);
        // Remember that:
        // 0 is the GameManager scene, it is the entry point
        // 1 is the Menu
        // 2 is the EssentialsScene
        // 3 and so on will be levels
    }

    public void SetVolume(float t)
    {
        GameManager.state.volume = t;
        AudioListener.volume = t;
    }

    public void SetPostProcessing(bool enable)
    {
        GameManager.state.postProcessing = enable;
    }

    public override void GameAwake()
    {
        volume.value = GameManager.state.volume;
        postProcessing.isOn = GameManager.state.postProcessing;
    }
}
