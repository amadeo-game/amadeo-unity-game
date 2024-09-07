using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using BridgePackage;
using UnityEngine.Serialization;

[RequireComponent(typeof(UIDocument))]
public class GameScreen : MonoBehaviour {
    const float k_DelayWinScreen = 0.5f;
    [SerializeField] bool _openInstructorPanelAtStart = false;

    // string IDs
    // references to functional UI elements (buttons and screens)
    VisualElement _settingsScreenRootElement;
    VisualElement _instructorScreenRootElement;
    VisualElement _instructorPanelRootElement;
    VisualElement _winScreenRootElement;
    VisualElement _loseScreenRootElement;
    VisualElement _zerofScreenRootElement;
    VisualElement _startingGameCountdownVisualElement;


    [FormerlySerializedAs("_pauseScreen")] [SerializeField]
    UIDocument _settingsScreen;

    [SerializeField] UIDocument _instructorScreen;
    [SerializeField] UIDocument _instructorPanel;

    [SerializeField] UIDocument m_WinLoseScreen;

    [SerializeField] UIDocument _zerofScreen;
    [SerializeField] UIDocument _startingGameCountdown;

    private int _countDownTime = 3;


    Slider _musicSlider;
    Slider _sfxSlider;

    Button _settingsButton;
    Button _instructorButton;
    Button _settingsResumeButton;
    Button _settingsQuitButton;
    Button _settingsBackButton;
    Button _instructorBackButton;


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
        GameEvents.GameBuilding += SetIdleStateScreen;
        GameplayEvents.SettingsUpdated += OnSettingsUpdated;

        GameEvents.GameInZeroF += ShowZeroFScreen;
        GameEvents.GameStarting += OnStartingGameState;
        GameEvents.GameIsRunning += OnInGameState;
        GameEvents.GameIdle += SetIdleStateScreen;

        GameConfigEvents.OnTimeDurationChanged += UpdateTimeLabel;


        GameConfigEvents.CountDown += UpdateCountDownLabel;
    }

    private void OnInGameState() {
        ShowVisualElement(_startingGameCountdownVisualElement, false);
    }

    private void UpdateCountDownLabel(int timeText) {
        if (timeText > _countDownTime) {
            // TODO : Wierd solution but for now its okay for now, need to find a better solution
            _countDownTime = timeText;
        }

        if (_startingGameCountdownVisualElement != null) {
            _startingGameCountdownVisualElement.Q<Label>("game_starting_countdown_label").text = timeText.ToString();
        }
    }

    private void OnStartingGameState() {
        ShowVisualElement(_zerofScreenRootElement, false);
        ShowVisualElement(_startingGameCountdownVisualElement, true);
        _startingGameCountdownVisualElement.Q<Label>("game_starting_countdown_label").text = _countDownTime.ToString();
    }

    private void ShowZeroFScreen() {
        ShowVisualElement(_zerofScreenRootElement, true);
    }

    private void UpdateTimeLabel(float newTime) {
        if (_gameTimer != null) {
            _gameTimer.text = newTime.ToString("F0"); // Format as needed
        }
    
        if (_instructorTimer != null) {
            _instructorTimer.text = newTime.ToString("F0");
        }
    }

    void OnDisable() {
        GameplayEvents.WinScreenShown -= OnGameWon;
        GameplayEvents.LoseScreenShown -= OnGameLost;
        GameEvents.GameBuilding -= SetIdleStateScreen;
        GameplayEvents.SettingsUpdated -= OnSettingsUpdated;

        GameEvents.GameInZeroF -= ShowZeroFScreen;
        GameEvents.GameStarting -= OnStartingGameState;
        GameEvents.GameIsRunning -= OnInGameState;
        GameEvents.GameIdle -= SetIdleStateScreen;

        GameConfigEvents.OnTimeDurationChanged -= UpdateTimeLabel;


        GameConfigEvents.CountDown -= UpdateCountDownLabel;
    }

    void SetVisualElements() {
        m_GameScreen = GetComponent<UIDocument>();
        VisualElement gameScreenRootElement = m_GameScreen.rootVisualElement;

        _settingsScreenRootElement = _settingsScreen.rootVisualElement;
        _instructorScreenRootElement = _instructorScreen.rootVisualElement;
        _instructorPanelRootElement = _instructorPanel.rootVisualElement;

        _winScreenRootElement = m_WinLoseScreen.rootVisualElement.Q<VisualElement>("game-win__screen");
        _loseScreenRootElement = m_WinLoseScreen.rootVisualElement.Q<VisualElement>("game-lose__screen");
        _zerofScreenRootElement = _zerofScreen.rootVisualElement.Q<VisualElement>("zero_f__screen");
        _startingGameCountdownVisualElement =
            _startingGameCountdown.rootVisualElement.Q<VisualElement>("game_starting_countdown_screen");

        // set visible false to _winScreenRootElement and _loseScreenRootElement
        ShowVisualElement(_winScreenRootElement, false);
        ShowVisualElement(_loseScreenRootElement, false);
        ShowVisualElement(_zerofScreenRootElement, false);
        ShowVisualElement(_startingGameCountdownVisualElement, false);

        _settingsButton = _instructorScreenRootElement.Q<Button>("settings__button");
        _instructorButton = _instructorScreenRootElement.Q<Button>("instructor_panel__button");

        _instructorTimer = _instructorScreenRootElement.Q<Label>("game-timer__label");
        _gameTimer = gameScreenRootElement.Q<Label>("game-timer__label");
        _settingsResumeButton = _settingsScreenRootElement.Q<Button>("settings__resume-button");
        _settingsQuitButton = _settingsScreenRootElement.Q<Button>("settings__quit-button");
        _settingsBackButton = _settingsScreenRootElement.Q<Button>("settings__back-button");
        _instructorBackButton = _instructorPanelRootElement.Q<Button>("instructor__back-button");

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
        // m_WinNextButton.RegisterCallback<ClickEvent>(QuitGame);
        // m_LoseQuitButton.RegisterCallback<ClickEvent>(QuitGame);
        // m_LoseRetryButton.RegisterCallback<ClickEvent>(RestartGame);

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
        ShowVisualElement(_zerofScreenRootElement, false);
        ShowVisualElement(_startingGameCountdownVisualElement, false);
        BlurBackground(false);
    }

    void ShowVisualElement(VisualElement visualElement, bool state) {
        if (visualElement == null) {
            Debug.Log("VisualElement is null");
            return;
        }

        Debug.Log("GameScreen: ShowVisualElement" + " " + visualElement.name + " " + state);

        visualElement.style.display = (state) ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void ShowPauseScreen(ClickEvent evt) {
        AudioManager.PlayDefaultButtonSound();

        GameplayEvents.GamePaused?.Invoke(1f);

        ShowVisualElement(_settingsScreenRootElement, true);
        // ShowVisualElement(m_PauseButton, false);

        BlurBackground(true);
    }

    void SetIdleStateScreen() {
        m_IsGameOver = false;
        // set visible false to _winScreenRootElement and _loseScreenRootElement
        ShowVisualElement(_winScreenRootElement, false);
        ShowVisualElement(_loseScreenRootElement, false);
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
        //yield return new WaitForSeconds(k_DelayWinScreen);
        yield return new WaitForSeconds(0f);

        // hide UI
        _settingsButton.style.display = DisplayStyle.None;

        AudioManager.PlayDefeatSound();
        if (m_IsGameOver) {
            ShowVisualElement(_loseScreenRootElement, true);
        }

        BlurBackground(true);
    }

    IEnumerator GameWonRoutine() {
        Time.timeScale = 0.5f;
        //yield return new WaitForSeconds(k_DelayWinScreen);
        yield return new WaitForSeconds(0f);

        // hide the UI
        _settingsButton.style.display = DisplayStyle.None;

        AudioManager.PlayVictorySound();
        Debug.Log("Showing Win Screen");
        if (m_IsGameOver) {
            ShowVisualElement(_winScreenRootElement, true);
        }
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
        m_IsGameOver = true;
        StartCoroutine(GameWonRoutine());
    }

    void OnGameLost() {
        m_IsGameOver = true;
        StartCoroutine(GameLostRoutine());
    }


    void OnSettingsUpdated(GameData gameData) {
        _musicSlider.value = gameData.musicVolume;
        _sfxSlider.value = gameData.sfxVolume;
    }
}