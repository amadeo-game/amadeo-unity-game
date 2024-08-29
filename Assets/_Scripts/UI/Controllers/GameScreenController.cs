using System;
using System.Collections;
using System.Collections.Generic;
using BridgePackage;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace UIToolkitDemo
{
    // non-UI logic for the GameScreen
    public class GameScreenController : MonoBehaviour
    {

        // temp storage to send back to GameDataManager
        GameData m_SettingsData;

        void OnEnable()
        {
            BridgeEvents.BridgeIsCompletedState += OnGameWon;
            BridgeEvents.BridgeCollapsingState += OnGameLost;

            GameplayEvents.GamePaused += OnGamePaused;
            GameplayEvents.GameResumed += OnGameResumed;
            GameplayEvents.GameQuit += OnGameQuit;
            GameplayEvents.MusicVolumeChanged += OnMusicVolumeChanged;
            GameplayEvents.SfxVolumeChanged += OnSfxVolumeChanged;
            
            SaveManager.GameDataLoaded += OnGameDataLoaded; 
        }

        void OnDisable()
        {
            BridgeEvents.BridgeIsCompletedState += OnGameWon;
            BridgeEvents.BridgeCollapsingState += OnGameLost;

            GameplayEvents.GamePaused -= OnGamePaused;
            GameplayEvents.GameResumed -= OnGameResumed;
            GameplayEvents.GameQuit -= OnGameQuit;
            GameplayEvents.MusicVolumeChanged -= OnMusicVolumeChanged;
            GameplayEvents.SfxVolumeChanged -= OnSfxVolumeChanged;
            
            SaveManager.GameDataLoaded -= OnGameDataLoaded;
        }

        IEnumerator PauseGameTime(float delay = 2f)
        {

            float pauseTime = Time.time + delay;
            float decrement = (delay > 0) ? Time.deltaTime / delay : Time.deltaTime;

            while (Time.timeScale > 0.1f || Time.time < pauseTime)
            {
                Time.timeScale = Mathf.Clamp(Time.timeScale - decrement, 0f, Time.timeScale - decrement);
                yield return null;
            }

            // ramp the timeScale down to 0
            Time.timeScale = 0f;
        }

        // scene-management methods
        void QuitGame()
        {
            Time.timeScale = 1f;
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                // quit application
                Application.Quit();
            // quit game in executable
            Application.Quit();

        }
        // event-handling methods
        void OnGameLost()
        {
            GameplayEvents.LoseScreenShown?.Invoke();
        }

        void OnGameWon()
        {
            GameplayEvents.WinScreenShown?.Invoke();
        }

        void OnGamePaused(float delay)
        {
            GameplayEvents.SettingsLoaded?.Invoke();
            StopAllCoroutines();
            StartCoroutine(PauseGameTime(delay));
        }

        void OnGameResumed()
        {
            GameplayEvents.SettingsUpdated?.Invoke(m_SettingsData);
            StopAllCoroutines();
            Time.timeScale = 1f;
        }

        void OnGameQuit()
        {
            QuitGame();
        }

        void OnSfxVolumeChanged(float sfxVolume)
        {
            m_SettingsData.musicVolume = sfxVolume;

            GameplayEvents.SettingsUpdated?.Invoke(m_SettingsData);
        }

        void OnMusicVolumeChanged(float musicVolume)
        {
            m_SettingsData.sfxVolume = musicVolume;

            GameplayEvents.SettingsUpdated?.Invoke(m_SettingsData);
        }

        void OnGameDataLoaded(GameData gameData)
        {
            if (gameData == null)
                return;

            m_SettingsData = gameData;

            m_SettingsData.musicVolume = gameData.musicVolume;
            m_SettingsData.sfxVolume = gameData.sfxVolume;

            GameplayEvents.SettingsUpdated?.Invoke(gameData);

        }
    }
}