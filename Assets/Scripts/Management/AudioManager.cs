using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<AudioManager>(); }
            return _instance;
        }
    }

    [SerializeField] private GameObject deathSFXPrefab;
    [SerializeField] private GameObject teleportSFXPrefab;
    [SerializeField] private GameObject winSFXPrefab;
    [SerializeField] private GameObject hitGroundSFXPrefab;
    [SerializeField] private GameObject rotateSFXPrefab;
    [SerializeField] private GameObject fragileBlockBreakSFXPrefab;
    [SerializeField] private GameObject placeBlockSFXPrefab;

    [SerializeField] private GameObject musicPrefab;

    private List<AudioSource> musicList = new List<AudioSource>();
    private void spawnAudioPrefab(GameObject prefab)
    {
        if(prefab == null)
        {
            if (GameManager.logLevel >= GameManager.LogLevel.Error) Debug.LogWarning("Trying to play sound but prefab is null!");
            return;
        }
        AudioSource audio = Instantiate(prefab, transform).GetComponent<AudioSource>();
        audio.Play();
        musicList.Add(audio);
    }

    public void playDeathSFX() { spawnAudioPrefab(deathSFXPrefab); }
    public void playTeleportSFX() { spawnAudioPrefab(teleportSFXPrefab); }
    public void playWinSFX() { spawnAudioPrefab(winSFXPrefab); }
    public void playHitGroundSFX() { spawnAudioPrefab(hitGroundSFXPrefab); }
    public void playRotateSFX() { spawnAudioPrefab(rotateSFXPrefab); }
    public void playFragileBlockBreakSFX() { spawnAudioPrefab(fragileBlockBreakSFXPrefab); }
    public void playPlaceBlockSFX() { spawnAudioPrefab(placeBlockSFXPrefab); }



    private void Awake()
    {
        Object.DontDestroyOnLoad(gameObject); // keep this alive
    }

    private void FixedUpdate()
    {
        for(int i=0; i<musicList.Count; i++)
        {
            AudioSource audio = musicList[i];
            if(audio.loop) { continue; } // this audio just loops, we will never remove it based on time
            if(audio.isPlaying) { continue; } // still playing, ignore it
            
            // audio is done playing, get rid of it
            Destroy(audio.gameObject);
            musicList.RemoveAt(i);
            i--;
        }
    }
}
