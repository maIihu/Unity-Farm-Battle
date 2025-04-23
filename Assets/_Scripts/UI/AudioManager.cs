using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    
    public AudioClip introMusic;
    public AudioClip gameMusic;
    public AudioClip digSoundEffect;
    public AudioClip thunderSoundEffect;
    public AudioClip tsunamiSoundEffect;
    
    public bool muteMusic;
    public bool muteSfx;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateMusic(introMusic);
        PlayMusic();
    }

    public void UpdateMusic(AudioClip music)
    {
        musicSource.clip = music;
    }

    public void PlayMusic()
    {
        musicSource.Play();
    }
    
    public void CheckMute(bool soundOn)
    {
        if(!soundOn)
            musicSource.Pause();
        else 
            musicSource.UnPause();

        muteMusic = !soundOn;
    }
    
    public void PlaySfx(AudioClip clip)
    {
        if (muteSfx)
            return;
        sfxSource.PlayOneShot(clip);
    }
}
