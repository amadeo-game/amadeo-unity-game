using UnityEngine;
using System;


[RequireComponent(typeof(SaveManager))]
public class GameDataManager : MonoBehaviour {
    [SerializeField] GameData m_GameData;

    public GameData GameData {
        set => m_GameData = value;
        get => m_GameData;
    }

    SaveManager m_SaveManager;
    bool m_IsGameDataInitialized;

    void OnEnable() {
        SettingsEvents.SettingsUpdated += OnSettingsUpdated;
    }

    void OnDisable() {
        SettingsEvents.SettingsUpdated -= OnSettingsUpdated;
    }

    void Awake() {
        m_SaveManager = GetComponent<SaveManager>();
    }

    void Start() {
        //if saved data exists, load saved data
        m_SaveManager.LoadGame();

        // flag that GameData is loaded the first time
        m_IsGameDataInitialized = true;
    }

    // update values from SettingsScreen
    void OnSettingsUpdated(GameData gameData) {
        if (gameData == null)
            return;

        m_GameData.sfxVolume = gameData.sfxVolume;
        m_GameData.musicVolume = gameData.musicVolume;
        m_GameData.dropdownSelection = gameData.dropdownSelection;
        m_GameData.isSlideToggled = gameData.isSlideToggled;
        m_GameData.isToggled = gameData.isToggled;
        m_GameData.buttonSelection = gameData.buttonSelection;
    }
}