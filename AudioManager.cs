using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;


public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources — Add 5 AudioSource components and drag them here")]
    public AudioSource musicSource; 
    public AudioSource sfxSource;     
    public AudioSource voiceSource; 
    public AudioSource ambienceSource;   
    public AudioSource windsSource; 
    public AudioClip footstepsClip;
    public AudioClip breathingClip;
    public AudioClip heartbeatClip;

    public AudioClip trafficCity;   // Zone A
    public AudioClip busStation;    // Zone B
    public AudioClip street;        // Zone C
    public AudioClip natureSound;   // Safe Zone
    public AudioClip getNegativeCard;  
    public AudioClip gameOver;
    public AudioClip gameWin;
    public AudioClip[] positiveCardAudios;
    public AudioClip[] negativeVoices;
    public AudioClip level2Music;   
    public AudioClip windsSound;    
    public AudioClip shadowSFX;
    public AudioClip lowEnergySFX;
    public AudioClip beaconHopeSFX; 
    public AudioClip hopeSparkSFX;
    public AudioClip[] memoryTokenVoices;

    // ── Volumes ──
    private float masterVolume = 1f;
    private float musicVolume  = 1f;
    private float sfxVolume    = 1f;

    private Dictionary<string, AudioClip> sceneMusicMap;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); return; }

        if (musicSource == null)    musicSource    = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)      sfxSource      = gameObject.AddComponent<AudioSource>();
        if (voiceSource == null)    voiceSource    = gameObject.AddComponent<AudioSource>();
        if (ambienceSource == null) ambienceSource = gameObject.AddComponent<AudioSource>();
        if (windsSource == null)    windsSource    = gameObject.AddComponent<AudioSource>();

        musicSource.loop    = true;
        ambienceSource.loop = true;
        windsSource.loop    = true;
        sceneMusicMap = new Dictionary<string, AudioClip>()
        {
            { "Zone A",    trafficCity },
            { "Zone B",    busStation  },
            { "Zone C",    street      },
            { "SAFE ZONE", natureSound },
            { "Level 2",   level2Music }
        };

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (sceneMusicMap.ContainsKey(scene.name) && sceneMusicMap[scene.name] != null)
            StartCoroutine(PlaySceneMusic(sceneMusicMap[scene.name]));

        // Wind ambience only in Level 2
        if (scene.name == "Level 2") PlayWinds(true);
        else                          PlayWinds(false);
    }

    public IEnumerator PlaySceneMusic(AudioClip clip, float fadeDuration = 2f)
    {
        if (musicSource.isPlaying)
            yield return StartCoroutine(FadeOut(musicSource, fadeDuration));
        musicSource.clip = clip;
        musicSource.Play();
        yield return StartCoroutine(FadeIn(musicSource, fadeDuration));
    }

    // ── Footsteps / Breathing / Heartbeat ──
    public void PlayFootsteps(bool play)
    {
        if (footstepsClip == null) return;
        if (play && !ambienceSource.isPlaying)
        {
            ambienceSource.clip = footstepsClip;
            ambienceSource.Play();
        }
        else if (!play && ambienceSource.isPlaying)
            ambienceSource.Stop();
    }

    public void PlayBreathing(bool play)
    {
        if (breathingClip == null) return;
        if (play) { ambienceSource.clip = breathingClip; ambienceSource.Play(); }
        else ambienceSource.Stop();
    }

    public void OnEnemyAppear()    => StartCoroutine(FadeHeartbeat(1f,   3f));
    public void OnEnemyDisappear() => StartCoroutine(FadeHeartbeat(0.2f, 3f));

    public IEnumerator FadeHeartbeat(float target, float duration)
    {
        if (heartbeatClip != null && !ambienceSource.isPlaying)
        {
            ambienceSource.clip = heartbeatClip;
            ambienceSource.Play();
        }
        float start = ambienceSource.volume, t = 0;
        while (t < duration)
        {
            ambienceSource.volume = Mathf.Lerp(start, target * masterVolume * sfxVolume, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        ambienceSource.volume = target * masterVolume * sfxVolume;
    }

    // ── Wind ambience (Level 2) ──
    public void PlayWinds(bool play)
    {
        if (windsSource == null) return;
        if (play && windsSound != null)
        {
            if (!windsSource.isPlaying)
            {
                windsSource.clip = windsSound;
                windsSource.volume = sfxVolume * masterVolume * 0.5f;
                windsSource.Play();
            }
        }
        else windsSource.Stop();
    }

    // ── Generic SFX / Voice ──
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    public void PlayVoiceClip(AudioClip clip)
    {
        if (clip == null) return;
        voiceSource.PlayOneShot(clip, sfxVolume * masterVolume);
    }

    // Same as PlayVoiceClip — added because other scripts call PlayVoice().
    public void PlayVoice(AudioClip clip) => PlayVoiceClip(clip);

    public void PlayVoice(VoiceType voiceType)
    {
        if (voiceType == VoiceType.Scary)
            PlayNegativeVoice(Random.Range(0, negativeVoices != null ? negativeVoices.Length : 0));
    }

    // ── LEVEL 1: positive card voice by index (matches the sentence shown) ──
    public void PlayPositiveCardByIndex(int index)
    {
        if (positiveCardAudios == null || positiveCardAudios.Length == 0) return;
        if (index < 0 || index >= positiveCardAudios.Length) return;
        if (positiveCardAudios[index] != null)
            voiceSource.PlayOneShot(positiveCardAudios[index], sfxVolume * masterVolume);
    }

    // Random positive card voice (fallback)
    public void PlayPositiveCardAudio()
    {
        if (positiveCardAudios == null || positiveCardAudios.Length == 0) return;
        int i = Random.Range(0, positiveCardAudios.Length);
        if (positiveCardAudios[i] != null)
            voiceSource.PlayOneShot(positiveCardAudios[i], sfxVolume * masterVolume);
    }

    // ── LEVEL 1: negative thought voice ──
    public void PlayNegativeVoice(int index)
    {
        if (negativeVoices == null || negativeVoices.Length == 0) return;
        if (index < 0 || index >= negativeVoices.Length) return;
        if (negativeVoices[index] != null)
            voiceSource.PlayOneShot(negativeVoices[index], sfxVolume * masterVolume);
    }

    // LEVEL 2 memory token voice
    public void PlayMemoryTokenVoice(int index)
    {
        if (memoryTokenVoices == null || memoryTokenVoices.Length == 0) return;
        if (index < 0 || index >= memoryTokenVoices.Length) return;
        if (memoryTokenVoices[index] != null)
            voiceSource.PlayOneShot(memoryTokenVoices[index], sfxVolume * masterVolume);
    }

    // ── LEVEL 2: other SFX ──
    public void PlayShadow()           { if (shadowSFX != null)     sfxSource.PlayOneShot(shadowSFX,      sfxVolume * masterVolume); }
    public void PlayLowEnergyWarning() { if (lowEnergySFX != null)  sfxSource.PlayOneShot(lowEnergySFX,   sfxVolume * masterVolume); }
    public void PlayBeaconHope()       { if (beaconHopeSFX != null) voiceSource.PlayOneShot(beaconHopeSFX, sfxVolume * masterVolume); }
    public void PlayHopeSpark()        { if (hopeSparkSFX != null)  sfxSource.PlayOneShot(hopeSparkSFX,   sfxVolume * masterVolume); }

    // ── Win / Lose ──
    public void PlayGameOver() => PlaySFX(gameOver);
    public void PlayGameWin()  => PlaySFX(gameWin);

    // ── Volume control ──
    public void SetMasterVolume(float v) { masterVolume = Mathf.Clamp01(v); UpdateVolumes(); }
    public void SetMusicVolume(float v)  { musicVolume  = Mathf.Clamp01(v); UpdateVolumes(); }
    public void SetSFXVolume(float v)    { sfxVolume    = Mathf.Clamp01(v); UpdateVolumes(); }

    private void UpdateVolumes()
    {
        musicSource.volume    = musicVolume * masterVolume;
        sfxSource.volume      = sfxVolume   * masterVolume;
        voiceSource.volume    = sfxVolume   * masterVolume;
        ambienceSource.volume = sfxVolume   * masterVolume;
    }

    // ── Fade helpers ──
    private IEnumerator FadeIn(AudioSource src, float duration)
    {
        src.volume = 0; float t = 0;
        while (t < duration) { src.volume = Mathf.Lerp(0, musicVolume * masterVolume, t / duration); t += Time.deltaTime; yield return null; }
        src.volume = musicVolume * masterVolume;
    }

    private IEnumerator FadeOut(AudioSource src, float duration)
    {
        float start = src.volume, t = 0;
        while (t < duration) { src.volume = Mathf.Lerp(start, 0, t / duration); t += Time.deltaTime; yield return null; }
        src.Stop(); src.volume = start;
    }
}