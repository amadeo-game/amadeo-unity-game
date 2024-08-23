using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System;


// note: this uses JsonUtility for demo purposes only; for production work, consider a more performant solution like MessagePack (https://msgpack.org/index.html) 
// or Protocol Buffers (https://developers.google.com/protocol-buffers)
// 

[RequireComponent(typeof(GameDataManager))]
public class SaveManager : MonoBehaviour {
    public static event Action<GameData> GameDataLoaded;

    [Tooltip("Filename to save game and settings data")] [SerializeField]
    string m_SaveFilename = "gamesettings.dat";

    [Tooltip("Show Debug messages.")] [SerializeField]
    bool m_Debug;

    GameDataManager m_GameDataManager;

    void Awake() {
        m_GameDataManager = GetComponent<GameDataManager>();
    }

    void OnApplicationQuit() {
        SaveGame();
    }

    void OnEnable() {
        // SettingsEvents.SettingsShown += OnSettingsShown;
        GameplayEvents.SettingsLoaded += OnSettingsShown;
        // SettingsEvents.SettingsUpdated += OnSettingsUpdated;

        GameplayEvents.SettingsUpdated += OnSettingsUpdated;
    }

    void OnDisable() {
        // SettingsEvents.SettingsShown -= OnSettingsShown;
        GameplayEvents.SettingsLoaded -= OnSettingsShown;
        // SettingsEvents.SettingsUpdated -= OnSettingsUpdated;

        GameplayEvents.SettingsUpdated -= OnSettingsUpdated;
    }

    public GameData NewGame() {
        return new GameData();
    }

    public void LoadGame() {
        // load saved data from FileDataHandler

        if (m_GameDataManager.GameData != null) {
            if (m_Debug) {
                Debug.Log("GAME DATA MANAGER LoadGame: Initializing game data.");
            }

            m_GameDataManager.GameData = NewGame();
        }
        else if (FileManager.LoadFromFile(m_SaveFilename, out var jsonString)) {
            m_GameDataManager.GameData.LoadJson(jsonString);

            if (m_Debug) {
                Debug.Log("SaveManager.LoadGame: " + m_SaveFilename + " json string: " + jsonString);
            }
        }

        // notify other game objects 
        if (m_GameDataManager.GameData != null) {
            GameDataLoaded?.Invoke(m_GameDataManager.GameData);
        }
    }

    public void SaveGame() {
        // string jsonFile = m_GameDataManager.GameData.ToJson();
        GameData gameData = m_GameDataManager.GameData;
        string jsonFile = gameData.ToJson();


        // save to disk with FileDataHandler
        if (FileManager.WriteToFile(m_SaveFilename, jsonFile) && m_Debug) {
            Debug.Log("SaveManager.SaveGame: " + m_SaveFilename + " json string: " + jsonFile);
        }
    }

    // Load the saved GameData and display on the Settings Screen
    void OnSettingsShown() {
        Debug.Log("SaveManager.OnSettingsShown: LoadGame() called.");

        if (m_GameDataManager.GameData != null) {
            GameDataLoaded?.Invoke(m_GameDataManager.GameData);
        }
        else {
            Debug.Log("GameDataManager.GameData is null.");
        }
    }

    // Update the GameDataManager data and save
    void OnSettingsUpdated(GameData gameData) {
        Debug.Log("SaveManager.OnSettingsUpdated: GameData updated.");

        m_GameDataManager.GameData = gameData;
        SaveGame();
    }
}