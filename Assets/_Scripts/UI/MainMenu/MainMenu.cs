using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum MainMenuState {
    MainMenu,
    Settings,
    PrepareSession,
    SelectTrials
}
public class MainMenu : MonoBehaviour {
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject prepareSubjectPanel;
    [SerializeField] private GameObject selectTrialsPanel;


    // Implement the state machine routes based on the enum and current state change method,

    private MainMenuState currentState;
    public void BackToPrepareSession() {
        ChangeState(MainMenuState.PrepareSession);
    }
    public void ChangeState(MainMenuState newState) {
        currentState = newState;
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
        prepareSubjectPanel.SetActive(false);
        selectTrialsPanel.SetActive(false);

        switch (currentState) {
            case MainMenuState.MainMenu:
                mainMenuPanel.SetActive(true);
                break;
            case MainMenuState.Settings:
                settingsPanel.SetActive(true);
                break;
            case MainMenuState.PrepareSession:
                prepareSubjectPanel.SetActive(true);
                break;
            case MainMenuState.SelectTrials:
                selectTrialsPanel.SetActive(true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }


    // Singleton pattern
    public static MainMenu Instance;


    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    public void StartGame() {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame() {
        Application.Quit();
        Debug.Log("Exit Game Pressed");
    }
}