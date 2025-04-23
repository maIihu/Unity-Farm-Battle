using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class UIButton : MonoBehaviour
{
    private Animator _ani;
    public GameObject pausePanel;
    
    private void Awake()
    {
        _ani = GetComponentInChildren<Animator>();
    }
    
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GamePlayScene");
        
        AudioManager.Instance.UpdateMusic(AudioManager.Instance.gameMusic);
        AudioManager.Instance.PlayMusic();
        if(AudioManager.Instance.muteMusic)
            AudioManager.Instance.CheckMute(false);

        GameManager.Instance.ChangeState(GameState.Cutscene);
        GameManager.Instance.turnOffTutorial = false;
        
    }
    
    public void LoadMenuScene()
    {
        SceneManager.LoadScene("MenuScene");
        
        AudioManager.Instance.UpdateMusic(AudioManager.Instance.introMusic);
        AudioManager.Instance.PlayMusic();
        if(AudioManager.Instance.muteMusic)
            AudioManager.Instance.CheckMute(false);

        GameManager.Instance.ChangeState(GameState.Menu);
        
    }

    public void PlayAgain()
    {
        LoadGameScene();
    }

    public void ExitGame()
    {
        Debug.Log("Close Game");
        Application.Quit();
    }
    
    public void OpenPauseMenu()
    {
        // PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        // ppVolume.enabled = !ppVolume.enabled;
        pausePanel.SetActive(true);
        
        GameManager.Instance.ChangeState(GameState.Paused);
    }

    public void ContinueGame()
    {
        // PostProcessVolume ppVolume = Camera.main.gameObject.GetComponent<PostProcessVolume>();
        // ppVolume.enabled = !ppVolume.enabled;
        pausePanel.SetActive(false);
        
        GameManager.Instance.ChangeState(GameState.Playing);
    }
    
}
