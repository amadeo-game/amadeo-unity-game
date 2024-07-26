using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using System;
using System.Linq;
using BridgePackage;
using UnityEngine.Serialization;

namespace UIToolkitDemo {
    [RequireComponent(typeof(UIDocument))]
    public class GameScreen : MonoBehaviour {
        const float k_DelayWinScreen = 2f;
        [SerializeField] bool _openInstructorPanelAtStart = false;

        // string IDs
        // references to functional UI elements (buttons and screens)
        VisualElement _settingsScreenRootElement;
        VisualElement _instructorScreenRootElement;
        VisualElement _instructorPanelRootElement;
        VisualElement _winScreenRootElement;
        VisualElement _loseScreenRootElement;

        [FormerlySerializedAs("_pauseScreen")] [SerializeField]
        UIDocument _settingsScreen;

        [SerializeField] UIDocument _instructorScreen;
        [SerializeField] UIDocument _instructorPanel;
        [SerializeField] UIDocument m_WinScreen;
        [SerializeField] UIDocument m_LoseScreen;

        Slider _musicSlider;
        Slider _sfxSlider;

        Button _settingsButton;
        Button _instructorButton;
        Button _settingsResumeButton;
        Button _settingsQuitButton;
        Button _settingsBackButton;
        Button _instructorBackButton;

        Button m_WinNextButton;
        Button m_LoseQuitButton;
        Button m_LoseRetryButton;

        private Label _instructorTimer;
        private Label _gameTimer;

        UIDocument m_GameScreen;

        bool m_IsGameOver;

        void OnEnable() {
            SetVisualElements();
            RegisterButtonCallbacks();

            // if (m_Volume == null)
            //     m_Volume = FindObjectOfType<Volume>();
    
            GameplayEvents.WinScreenShown += OnGameWon;
            GameplayEvents.LoseScreenShown += OnGameLost;
            BridgeEvents.OnTimeDurationChanged += UpdateTimeLabel;


            GameplayEvents.SettingsUpdated += OnSettingsUpdated;
        }
        
        private void UpdateTimeLabel(float newTime) {
            if (_gameTimer != null) {
                _gameTimer.text = newTime.ToString("F2"); // Format as needed
            } if(_instructorTimer != null) {
                _instructorTimer.text = newTime.ToString("F2");
            }
        }

        void OnDisable() {
            GameplayEvents.WinScreenShown -= OnGameWon;
            GameplayEvents.LoseScreenShown -= OnGameLost;
            BridgeEvents.OnTimeDurationChanged -= UpdateTimeLabel;

            GameplayEvents.SettingsUpdated -= OnSettingsUpdated;
        }

        void SetVisualElements() {
            m_GameScreen = GetComponent<UIDocument>();
            VisualElement gameScreenRootElement = m_GameScreen.rootVisualElement;

            _settingsScreenRootElement = _settingsScreen.rootVisualElement;
            _instructorScreenRootElement = _instructorScreen.rootVisualElement;
            _instructorPanelRootElement = _instructorPanel.rootVisualElement;
            _winScreenRootElement = m_WinScreen.rootVisualElement;
            _loseScreenRootElement = m_LoseScreen.rootVisualElement;

            _settingsButton = _instructorScreenRootElement.Q<Button>("settings__button");
            _instructorButton = _instructorScreenRootElement.Q<Button>("instructor_panel__button");

            _instructorTimer = _instructorScreenRootElement.Q<Label>("game-timer__label");
            _gameTimer = gameScreenRootElement.Q<Label>("game-timer__label");
            _settingsResumeButton = _settingsScreenRootElement.Q<Button>("settings__resume-button");
            _settingsQuitButton = _settingsScreenRootElement.Q<Button>("settings__quit-button");
            _settingsBackButton = _settingsScreenRootElement.Q<Button>("settings__back-button");
            _instructorBackButton = _instructorPanelRootElement.Q<Button>("instructor__back-button");

            m_WinNextButton = _winScreenRootElement.Q<Button>("game-win__next-button");
            m_LoseQuitButton = _loseScreenRootElement.Q<Button>("game-lose__quit-button");
            m_LoseRetryButton = _loseScreenRootElement.Q<Button>("game-lose__retry-button");

            _musicSlider = _settingsScreenRootElement.Q<Slider>("settings__music-slider");
            _sfxSlider = _settingsScreenRootElement.Q<Slider>("settings__sfx-slider");
        }

        void RegisterButtonCallbacks() {
            // set up buttons with RegisterCallback
            
            // HUD buttons
            _settingsButton.RegisterCallback<ClickEvent>(ShowPauseScreen);
            _instructorButton.RegisterCallback<ClickEvent>(env => ShowInstructorPanel(true));
            
            // PauseScreen buttons
            _settingsResumeButton.RegisterCallback<ClickEvent>(ResumeGame);
            _settingsQuitButton.RegisterCallback<ClickEvent>(QuitGame);
            
            // Close panels buttons
            _settingsBackButton.RegisterCallback<ClickEvent>(ResumeGame);
            _instructorBackButton.RegisterCallback<ClickEvent>(env => ShowInstructorPanel(false));

            // Win/Lose screen buttons
            m_WinNextButton.RegisterCallback<ClickEvent>(QuitGame);
            m_LoseQuitButton.RegisterCallback<ClickEvent>(QuitGame);
            m_LoseRetryButton.RegisterCallback<ClickEvent>(RestartGame);

            _musicSlider.RegisterValueChangedCallback(ChangeMusicVolume);
            _sfxSlider.RegisterValueChangedCallback(ChangeSfxVolume);

        }

        void ShowInstructorPanel(bool state) {
            AudioManager.PlayDefaultButtonSound();
            ShowVisualElement(_instructorPanelRootElement, state);
        }

        void Start() {
            ShowVisualElement(_settingsScreenRootElement, false);
            ShowVisualElement(_instructorPanelRootElement, _openInstructorPanelAtStart);
            ShowVisualElement(_winScreenRootElement, false);
            ShowVisualElement(_loseScreenRootElement, false);

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

            ShowVisualElement(_settingsScreenRootElement, true);
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
            ShowVisualElement(_settingsScreenRootElement, false);
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
            _settingsButton.style.display = DisplayStyle.None;

            AudioManager.PlayDefeatSound();
            ShowVisualElement(_loseScreenRootElement, true);
            BlurBackground(true);
        }

        IEnumerator GameWonRoutine() {
            Time.timeScale = 0.5f;
            yield return new WaitForSeconds(k_DelayWinScreen);

            // hide the UI
            _settingsButton.style.display = DisplayStyle.None;

            AudioManager.PlayVictorySound();
            ShowVisualElement(_winScreenRootElement, true);
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
            _musicSlider.value = gameData.musicVolume;
            _sfxSlider.value = gameData.sfxVolume;
        }
    }
}