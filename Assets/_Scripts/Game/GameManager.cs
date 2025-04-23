using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public enum GameState{ Menu, Cutscene, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameState currentState;
    public bool turnOffTutorial;
    public GameObject audio;
    
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
        currentState = GameState.Menu;
    }

    private void Update()
    {
        if (!turnOffTutorial)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S)
                                            || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W)
                                            || Input.GetKeyDown(KeyCode.Space))
            {
                GameObject tutorial = GameObject.FindGameObjectWithTag("Tutorial");
                if (tutorial != null)
                {
                    tutorial.SetActive(false);
                    turnOffTutorial = true;
                    ChangeState(GameState.Playing);
                }
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;
        ActiveState(currentState);
    }
    
    private void ActiveState(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Menu:
                Time.timeScale = 0f;
                audio.SetActive(true);
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                audio.SetActive(true);
                break;
            case GameState.Playing:
                Time.timeScale = 1f;
                audio.SetActive(false);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                audio.SetActive(false);
                break;
            case GameState.Cutscene:
                Time.timeScale = 1f;
                audio.SetActive(false);
                break;
        }
    }
}
