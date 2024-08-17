using System;
using System.Collections.Generic;
using System.Linq;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class InstructorPanel : MonoBehaviour {
    public LevelManager LevelManager;

    // Containers for UI elements
    private VisualElement _preGameConfigs;
    private VisualElement _endPauseButtons;

    // UI action buttons
    private Button _endSessionButton;
    private Button _pauseSessionButton;
    private Button _resumeSessionButton;
    private Button _startSessionButton;

    private List<SliderInt> _sliders = new List<SliderInt>(); // List to hold slider references
    private DropdownField _dropdownField;
    private List<string> levels;


    private void Start() {
        // Obtain the root visual element of the UXML.
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        levels = Enumerable.Range(1, BridgeDataManager.NumberOfLevels - 1).Select(i => i.ToString()).ToList();

        // Initialize UI elements and set up change listeners.
        UpdateUIWithCurrentValues(rootVisualElement);
        SetupUIChangeListeners(rootVisualElement);
        // Set up button callbacks
        SetupButtonCallbacks(rootVisualElement);

        SetVisibility(_endPauseButtons, false);
        SetVisibility(_resumeSessionButton, false);
        SetVisibility(_startSessionButton, true);
    }

    private void SetupButtonCallbacks(VisualElement root) {
        _preGameConfigs = root.Q<VisualElement>("pre_game_configs");
        if (_preGameConfigs == null) {
            Debug.LogError("Failed to find 'Pre-Game Configs' container.");
            return;
        }

        _endPauseButtons = root.Q<VisualElement>("end_pause_buttons");
        if (_endPauseButtons == null) {
            Debug.LogError("Failed to find 'Action Buttons' container.");
            return;
        }

        _endSessionButton = root.Q<Button>("end_session_button");
        if (_endSessionButton == null) {
            Debug.LogError("Failed to find 'Initialize Session' button.");
            return;
        }

        _pauseSessionButton = root.Q<Button>("pause_session_button");
        if (_pauseSessionButton == null) {
            Debug.LogError("Failed to find 'Pause Session' button.");
            return;
        }

        _resumeSessionButton = root.Q<Button>("resume_session_button");
        if (_resumeSessionButton == null) {
            Debug.LogError("Failed to find 'Resume Session' button.");
            return;
        }

        _startSessionButton = root.Q<Button>("start_session_button");
        if (_startSessionButton == null) {
            Debug.LogError("Failed to find 'Start Game' button.");
            return;
        }

        _endSessionButton.RegisterCallback<ClickEvent>(evt => LevelManager.ForceEndSession());

        _pauseSessionButton.RegisterCallback<ClickEvent>(evt => OnPausePressed());

        _resumeSessionButton.RegisterCallback<ClickEvent>(evt => LevelManager.ResumeSession());

        _startSessionButton.RegisterCallback<ClickEvent>(evt => LevelManager.StartSession());
    }

    private void OnPausePressed() {
        SetInteractability(_pauseSessionButton, false);
        LevelManager.PauseSession();
    }

    private void OnEnable() {
        // BridgeEvents.BridgeStateChanged += OnBridgeStateChange;
        BridgeEvents.IdleState += OnIdleState;
        BridgeEvents.BuildingState += OnBuildingState;
        BridgeEvents.GamePausedState += OnPausedState;
        BridgeEvents.StartingGameState += OnStartingGameState;
        BridgeEvents.InGameState += OnInGameState;
        BridgeEvents.BridgeCollapsingState += OnBridgeCollapsingState;
        BridgeEvents.BridgeCompletingState += OnBridgeCompletingState;
        BridgeEvents.GameFailedState += OnGameFailedState;
        BridgeEvents.GameWonState += OnGameWonState;
    }

    private void OnGameFailedState() {
        SetVisibility(_endPauseButtons, false);
        SetVisibility(_startSessionButton, true);
        SetInteractability(_preGameConfigs, true);
        SetInteractability(_startSessionButton, true);
    }

    private void OnGameWonState()
    {
        SetVisibility(_endPauseButtons, false);
        SetVisibility(_startSessionButton, true);
        SetInteractability(_preGameConfigs, true);
        SetInteractability(_startSessionButton, true);
    }

    private void OnDisable() {
        BridgeEvents.IdleState -= OnIdleState;
        BridgeEvents.BuildingState -= OnBuildingState;
        BridgeEvents.GamePausedState -= OnPausedState;
        BridgeEvents.StartingGameState -= OnStartingGameState;
        BridgeEvents.InGameState -= OnInGameState;
        BridgeEvents.BridgeCollapsingState -= OnBridgeCollapsingState;
        BridgeEvents.BridgeCompletingState -= OnBridgeCompletingState;
        BridgeEvents.GameFailedState -= OnGameFailedState;
        BridgeEvents.GameWonState -= OnGameWonState;
    }
    
    private void OnIdleState() {
        SetInteractability(_preGameConfigs, true);
        SetVisibility(_endPauseButtons, false);
        SetVisibility(_startSessionButton, true);
        SetInteractability(_startSessionButton, true);
    }

    private void OnBuildingState() {
        SetInteractability(_preGameConfigs, false);
        SetInteractability(_startSessionButton, false);
    }

    private void OnPausedState() {
        SetVisibility(_pauseSessionButton, false);
        SetVisibility(_resumeSessionButton, true);
        SetInteractability(_resumeSessionButton, true);
        SetVisibility(_startSessionButton, false);
    }

    private void OnStartingGameState() {
        SetInteractability(_resumeSessionButton, false);
    }

    private void OnInGameState() {
        SetVisibility(_startSessionButton, false);
        SetInteractability(_endPauseButtons, true);
        SetVisibility(_pauseSessionButton, true);
        SetVisibility(_resumeSessionButton, false);
        SetVisibility(_endSessionButton, true);
    }

    private void OnBridgeCollapsingState() {
        SetInteractability(_endPauseButtons, false);
    }

    private void OnBridgeCompletingState() {
        SetInteractability(_endPauseButtons, false);
    }

    /// <summary>
    /// Sets the interactability of all UI elements within a given visual element.
    /// </summary>
    /// <param name="container">The container whose children's interactability will be set.</param>
    /// <param name="active">Whether the elements should be enabled (true) or disabled (false).</param>
    private void SetInteractability(VisualElement container, bool active) {
        if (container == null) return;
        container.SetEnabled(active); // Directly set the enabled state of the container
        foreach (var child in container.Children()) {
            SetInteractability(child, active); // Recursively disable/enable child elements
        }
    }

    private void SetVisibility(VisualElement container, bool visible) {
        if (container == null) return;

        container.visible = visible; // Directly set the enabled state of the container
        foreach (var child in container.Children()) {
            SetVisibility(child, visible); // Recursively disable/enable child elements
        }
    }

    /// <summary>
    /// Updates all UI elements with values from BridgeDataManager.
    /// </summary>
    /// <param name="root">The root visual element of the UI document.</param>
    private void UpdateUIWithCurrentValues(VisualElement root) {
        // Update the height sliders, grace fields, MVC fields, and active unit toggles
        for (int i = 0; i < 5; i++) {
            var slider = root.Q<SliderInt>("height_" + (i + 1));
            if (slider != null) {
                slider.value = BridgeDataManager.Heights[i];
                _sliders.Add(slider); // Update the slider reference in the list
                SetSliderRotation(slider,
                    BridgeDataManager.IsFlexion); // Set initial rotation based on the flexion state
            }

            var graceField = root.Q<FloatField>("grace_" + (i + 1));
            if (graceField != null) {
                graceField.value = BridgeDataManager.UnitsGrace[i];
            }

            var mvcField = root.Q<IntegerField>("mvc_e_" + (i + 1));
            if (mvcField != null) {
                mvcField.value = (int)BridgeDataManager.MvcValuesExtension[i];
            }

            var mvcFlexField = root.Q<IntegerField>("mvc_f_" + (i + 1));
            if (mvcFlexField != null) {
                mvcFlexField.value = (int)BridgeDataManager.MvcValuesFlexion[i];
            }

            var toggle = root.Q<Toggle>("active_unit_" + (i + 1));
            if (toggle != null) {
                toggle.value = BridgeDataManager.PlayableUnits[i];
            }
        }

        // Update the time field
        var timeField = root.Q<IntegerField>("time_int_field");
        if (timeField == null) {
            Debug.LogWarning("Failed to find integer field element with name: time_int_field");
        }
        else {
            timeField.value = (int)BridgeDataManager.TimeDuration;
        }

        // Update the left hand toggle
        var leftHandToggle = root.Q<Toggle>("left_hand_toggle");
        if (leftHandToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name: left_hand_toggle");
        }
        else {
            leftHandToggle.value = BridgeDataManager.IsLeftHand;
        }

        // Update the flexion toggle
        var isFlexionToggle = root.Q<Toggle>("is_flexion_toggle");
        if (isFlexionToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name: is_flexion_toggle");
        }
        else {
            isFlexionToggle.value = BridgeDataManager.IsFlexion;
        }

        // Update the zeroF toggle
        var zeroFToggle = root.Q<Toggle>("zero_f_toggle");
        if (zeroFToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name: zero_f_toggle");
        }
        else {
            zeroFToggle.value = BridgeDataManager.ZeroF;
        }

        // Update the autoPlay toggle
        var autoPlayToggle = root.Q<Toggle>("auto_start_toggle");
        if (autoPlayToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name: auto_start_toggle");
        }
        else {
            autoPlayToggle.value = BridgeDataManager.AutoStart;
        }


        // Update the level dropdown
        var dropdown = root.Q<DropdownField>("level_picker");
        if (dropdown == null) {
            Debug.LogWarning("Failed to find dropdown element with name: level_picker");
        }
        else {
            dropdown.value = BridgeDataManager.Level.ToString();
        }
    }

    /// <summary>
    /// Sets up listeners on UI elements to update data manager when UI changes.
    /// </summary>
    /// <param name="root">The root visual element of the UI document.</param>
    private void SetupUIChangeListeners(VisualElement root) {
        for (int i = 0; i < 5; i++) {
            int localIndex = i; // Create a local copy of the loop variable

            if (_sliders[i] != null) {
                _sliders[i].RegisterValueChangedCallback(evt => UpdateHeight(localIndex, evt.newValue));
            }

            var graceField = root.Q<FloatField>("grace_" + (localIndex + 1));
            if (graceField != null) {
                graceField.RegisterValueChangedCallback(evt => UpdateGrace(localIndex, evt.newValue));
            }

            var mvcExtField = root.Q<IntegerField>("mvc_e_" + (localIndex + 1));
            if (mvcExtField != null) {
                mvcExtField.RegisterValueChangedCallback(evt => UpdateMvc(localIndex, evt.newValue, flexion: false));
            }

            var mvcFlexField = root.Q<IntegerField>("mvc_f_" + (localIndex + 1));
            if (mvcFlexField != null) {
                mvcFlexField.RegisterValueChangedCallback(evt => UpdateMvc(localIndex, evt.newValue, flexion: true));
            }

            var toggle = root.Q<Toggle>("active_unit_" + (localIndex + 1));
            if (toggle != null) {
                toggle.RegisterValueChangedCallback(evt => UpdateActiveUnit(localIndex, evt.newValue));
            }
        }


        var timeField = root.Q<IntegerField>("time_int_field");
        if (timeField != null) {
            timeField.RegisterValueChangedCallback(evt => BridgeDataManager.SetTimeDuration(evt.newValue));
        }

        var leftHandToggle = root.Q<Toggle>("left_hand_toggle");
        if (leftHandToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name:  + left_hand_toggle");
        }

        leftHandToggle.RegisterValueChangedCallback(evt => BridgeDataManager.SetIsLeftHand(evt.newValue));

        var isFlexionToggle = root.Q<Toggle>("is_flexion_toggle");
        if (isFlexionToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name:  + is_flexion_toggle");
        }

        isFlexionToggle.RegisterValueChangedCallback(evt => SetFlexion(evt.newValue));

        var zeroFToggle = root.Q<Toggle>("zero_f_toggle");
        if (zeroFToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name:  + zero_f_toggle");
        }

        zeroFToggle.RegisterValueChangedCallback(evt => BridgeDataManager.SetZeroF(evt.newValue));

        var autoPlayToggle = root.Q<Toggle>("auto_start_toggle");
        if (autoPlayToggle == null) {
            Debug.LogWarning("Failed to find toggle element with name:  + auto_start_toggle");
        }

        autoPlayToggle.RegisterValueChangedCallback(evt => BridgeDataManager.SetAutoStart(evt.newValue));

        var dropdown = root.Q<DropdownField>("level_picker");
        if (dropdown != null) {
            dropdown.bindingPath = "level";
            dropdown.choices = levels;
            dropdown.RegisterValueChangedCallback(evt => BridgeDataManager.SetLevel(int.Parse(evt.newValue)));
        }
    }

    /// <summary>
    /// Sets or unsets the flexion state and adjusts slider rotations accordingly.
    /// </summary>
    private void SetFlexion(bool isFlexion) {
        BridgeDataManager.SetIsFlexion(isFlexion);
        foreach (SliderInt slider in _sliders) {
            SetSliderRotation(slider, isFlexion);
        }
    }

    /// <summary>
    /// Sets the rotation of a slider based on the flexion state.
    /// </summary>
    private void SetSliderRotation(SliderInt slider, bool isFlexion) {
        // Set rotation to 90 degrees if flexion is true, otherwise set to 270 degrees for inverted vertical
        var rotationAngle = isFlexion ? 90f : 270f;
        slider.style.rotate = new Rotate(rotationAngle);
    }


    /// <summary>
    /// Updates the specified height in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the height to update.</param>
    /// <param name="newValue">New value for the height.</param>
    private void UpdateHeight(int index, int newValue) {
        int[] heights = BridgeDataManager.Heights;
        heights[index] = newValue;
        BridgeDataManager.SetHeights(heights);
    }

    /// <summary>
    /// Updates the grace period for a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newValue">New grace value.</param>
    private void UpdateGrace(int index, float newValue) {
        BridgeDataManager.SetUnitsGrace(index, newValue);
    }

    /// <summary>
    /// Updates the MVC value for a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newValue">New MVC value.</param>
    private void UpdateMvc(int index, int newValue, bool flexion) {
        if (flexion) {
            BridgeDataManager.SetMvcValuesFlexion(index, newValue);
        }
        else {
            BridgeDataManager.SetMvcValuesExtension(index, newValue);
        }
    }

    /// <summary>
    /// Updates the active state of a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newState">New active state.</param>
    private void UpdateActiveUnit(int index, bool newState) {
        BridgeDataManager.SetPlayableUnit(index: index, value: newState);
    }
}