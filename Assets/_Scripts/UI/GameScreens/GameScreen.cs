using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using System;
using System.Linq;

namespace UIToolkitDemo {
    [RequireComponent(typeof(UIDocument))]
    public class GameScreen : MonoBehaviour {
        // [Header("Menu Screen elements")][Tooltip("String IDs to query Visual Elements")]
        // [SerializeField] string m_PauseScreenName = "PauseScreen";
        // [SerializeField] string m_WinScreenName = "GameWinScreen";
        // [SerializeField] string m_LoseScreenName = "GameLoseScreen";

        // [Header("Blur")]
        // [SerializeField] Volume m_Volume;

        const float k_DelayWinScreen = 2f;

        // string IDs
        // references to functional UI elements (buttons and screens)
        VisualElement pauseScreenRootElement;
        VisualElement winScreenRootElement;
        VisualElement loseScreenRootElement;

        [SerializeField] UIDocument m_PauseScreen;
        [SerializeField] UIDocument m_WinScreen;
        [SerializeField] UIDocument m_LoseScreen;

        Slider m_MusicSlider;
        Slider m_SfxSlider;

        Button m_PauseButton;
        Button m_PauseResumeButton;
        Button m_PauseQuitButton;
        Button m_PauseBackButton;

        Button m_WinNextButton;
        Button m_LoseQuitButton;
        Button m_LoseRetryButton;

        UIDocument m_GameScreen;

        bool m_IsGameOver;

        void OnEnable() {
            SetVisualElements();
            RegisterButtonCallbacks();

            // if (m_Volume == null)
            //     m_Volume = FindObjectOfType<Volume>();

            GameplayEvents.WinScreenShown += OnGameWon;
            GameplayEvents.LoseScreenShown += OnGameLost;

            GameplayEvents.SettingsUpdated += OnSettingsUpdated;
        }

        void OnDisable() {
            GameplayEvents.WinScreenShown -= OnGameWon;
            GameplayEvents.LoseScreenShown -= OnGameLost;

            GameplayEvents.SettingsUpdated -= OnSettingsUpdated;
        }

        void SetVisualElements() {
            m_GameScreen = GetComponent<UIDocument>();
            VisualElement gameScreenRootElement = m_GameScreen.rootVisualElement;

            pauseScreenRootElement = m_PauseScreen.rootVisualElement;
            winScreenRootElement = m_WinScreen.rootVisualElement;
            loseScreenRootElement = m_LoseScreen.rootVisualElement;

            m_PauseButton = gameScreenRootElement.Q<Button>("pause__button");

            m_PauseResumeButton = pauseScreenRootElement.Q<Button>("pause__resume-button");
            m_PauseQuitButton = pauseScreenRootElement.Q<Button>("pause__quit-button");
            m_PauseBackButton = pauseScreenRootElement.Q<Button>("pause__back-button");

            m_WinNextButton = winScreenRootElement.Q<Button>("game-win__next-button");
            m_LoseQuitButton = loseScreenRootElement.Q<Button>("game-lose__quit-button");
            m_LoseRetryButton = loseScreenRootElement.Q<Button>("game-lose__retry-button");

            m_MusicSlider = pauseScreenRootElement.Q<Slider>("pause__music-slider");
            m_SfxSlider = pauseScreenRootElement.Q<Slider>("pause__sfx-slider");
        }

        void RegisterButtonCallbacks() {
            // set up buttons with RegisterCallback
            m_PauseButton.RegisterCallback<ClickEvent>(ShowPauseScreen);
            m_PauseResumeButton.RegisterCallback<ClickEvent>(ResumeGame);
            m_PauseBackButton.RegisterCallback<ClickEvent>(ResumeGame);
            m_PauseQuitButton.RegisterCallback<ClickEvent>(QuitGame);

            m_WinNextButton.RegisterCallback<ClickEvent>(QuitGame);
            m_LoseQuitButton.RegisterCallback<ClickEvent>(QuitGame);
            m_LoseRetryButton.RegisterCallback<ClickEvent>(RestartGame);

            m_MusicSlider.RegisterValueChangedCallback(ChangeMusicVolume);
            m_SfxSlider.RegisterValueChangedCallback(ChangeSfxVolume);
        }

        void Start() {
            ShowVisualElement(pauseScreenRootElement, false);
            ShowVisualElement(winScreenRootElement, false);
            ShowVisualElement(loseScreenRootElement, false);

            BlurBackground(false);
        }

        void ShowVisualElement(VisualElement visualElement, bool state) {
            if (visualElement == null) {
                Debug.Log("VisualElement is null");
                return;
            }

            visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void ShowPauseScreen(ClickEvent evt) {
            AudioManager.PlayDefaultButtonSound();

            GameplayEvents.GamePaused?.Invoke(1f);

            ShowVisualElement(pauseScreenRootElement, true);
            // ShowVisualElement(m_PauseButton, false);

            BlurBackground(true);
        }

        void RestartGame(ClickEvent evt) {
            AudioManager.PlayDefaultButtonSound();
            GameplayEvents.GameRestarted?.Invoke();
        }

        void QuitGame(ClickEvent evt) {
            AudioManager.PlayDefaultButtonSound();
            GameplayEvents.GameQuit?.Invoke();
        }

        void ResumeGame(ClickEvent evt) {
            GameplayEvents.GameResumed?.Invoke();
            AudioManager.PlayDefaultButtonSound();
            ShowVisualElement(pauseScreenRootElement, false);
            // ShowVisualElement(m_PauseButton, true);
            BlurBackground(false);
        }

        // use Volume to blur the background GameObjects
        void BlurBackground(bool state) {
            // if (m_Volume == null)
            //     return;
            //
            // DepthOfField blurDOF;
            // if (m_Volume.profile.TryGet<DepthOfField>(out blurDOF))
            // {
            //     blurDOF.active = state;
            // }
        }


        // frame fx for special abilities
        void EnableFrameFX(VisualElement card, bool state) {
            if (card == null)
                return;

            VisualElement frameFx = card.Q<VisualElement>("game-char__fx-frame");
            ShowVisualElement(frameFx, state);
        }

        IEnumerator GameLostRoutine() {
            // wait, then show lose screen and blur bg
            yield return new WaitForSeconds(k_DelayWinScreen);

            // hide UI
            m_PauseButton.style.display = DisplayStyle.None;

            AudioManager.PlayDefeatSound();
            ShowVisualElement(loseScreenRootElement, true);
            BlurBackground(true);
        }

        IEnumerator GameWonRoutine() {
            Time.timeScale = 0.5f;
            yield return new WaitForSeconds(k_DelayWinScreen);

            // hide the UI
            m_PauseButton.style.display = DisplayStyle.None;

            AudioManager.PlayVictorySound();
            ShowVisualElement(winScreenRootElement, true);
        }

        // volume settings
        void ChangeSfxVolume(ChangeEvent<float> evt) {
            GameplayEvents.MusicVolumeChanged?.Invoke(evt.newValue);
        }

        void ChangeMusicVolume(ChangeEvent<float> evt) {
            GameplayEvents.SfxVolumeChanged?.Invoke(evt.newValue);
        }

        // event-handling methods
        void OnGameWon() {
            if (m_IsGameOver)
                return;

            m_IsGameOver = true;
            StartCoroutine(GameWonRoutine());
        }

        void OnGameLost() {
            if (m_IsGameOver)
                return;

            m_IsGameOver = true;
            StartCoroutine(GameLostRoutine());
        }


        void OnSettingsUpdated(GameData gameData) {
            m_MusicSlider.value = gameData.musicVolume;
            m_SfxSlider.value = gameData.sfxVolume;
        }
    }
}