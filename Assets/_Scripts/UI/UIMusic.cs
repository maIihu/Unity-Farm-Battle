using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIMusic : MonoBehaviour
{
    [SerializeField] private Sprite[] soundSprite;
    [SerializeField] private Sprite[] sfxSprite;
    private bool _isSoundOn;
    private bool _isSfxOn;

    private GameObject _sound;
    private GameObject _sfx;

    private static UIMusic _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }


    private void Start()
    {
        _sound = transform.GetChild(0).gameObject;
        _sfx = transform.GetChild(1).gameObject;
        _isSoundOn = true;
        _isSfxOn = true;
    }
    
    public void ButtonSoundClick()
    {
        _isSoundOn = !_isSoundOn;
        if (_isSoundOn)
        {
            _sound.GetComponent<Image>().sprite = soundSprite[0];
        }
        else
        {
            _sound.GetComponent<Image>().sprite = soundSprite[1];
        }
        AudioManager.Instance.CheckMute(_isSoundOn);

    }

    public void ButtonVfxClick()
    {
        _isSfxOn = !_isSfxOn;
        if (_isSfxOn)
        {
            _sfx.GetComponent<Image>().sprite = sfxSprite[0];
        }
        else
        {
            _sfx.GetComponent<Image>().sprite = sfxSprite[1];
        }

        AudioManager.Instance.muteSfx = !_isSfxOn;
    }
}
