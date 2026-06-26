using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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
    [SerializeField] private TextMeshProUGUI bottomText;

    private void Awake()
    {
        StartCoroutine(Fade());
    }

    private IEnumerator IAmAlive()
    {
        if (GameManager.state.postProcessing)
        {
            bottomText.text = "Optimized. Does it work on potatoes? Maybe let the results speak...";
        }
        if (GameManager.state.volume == 0.67f)
        {
            bottomText.text = "The level of precision- :skulk:";
        }
        if (GameManager.state.lost > 100)
        {
            bottomText.text = "Consistent!";
        }
        if (GameManager.state.secretThing)
        {
            bottomText.text = "Wow, you've been driving out of bounds lately";
        }
        if ((DateTime.Now - new DateTime(GameManager.state.firstPlayedTicks)).TotalDays > 365)
        {
            bottomText.text = "Nostalgic";
        }
        if ((DateTime.Now - GameManager.state.lastPlayed).TotalDays > 60)
        {
            bottomText.text = "Long time no see!";
        }
        using (UnityWebRequest webRequest = UnityWebRequest.Get("https://live.alimad.co/ping?app=spin-to-win"))
        {
            yield return webRequest.SendWebRequest();
            if (webRequest.result != UnityWebRequest.Result.ConnectionError && webRequest.result != UnityWebRequest.Result.ProtocolError)
            {
                int t;
                System.Int32.TryParse(webRequest.downloadHandler.text, out t);
                if (t > 1)
                {
                    if(bottomText.text == "")
                        bottomText.text = t.ToString() + " pplz are playing this game rn!";
                }
            }
        }
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

    public void Quit()
    {
        Application.Quit(0);
    }

    public override void GameAwake()
    {
        volume.value = GameManager.state.volume;
        postProcessing.isOn = GameManager.state.postProcessing;
        StartCoroutine(IAmAlive());
    }

    public void Credits()
    {
        GameManager.Instance.SaveState();
        currentCamera.GetComponent<AudioListener>().enabled = false;
        SceneManager.LoadScene("Credits");
    }
}
