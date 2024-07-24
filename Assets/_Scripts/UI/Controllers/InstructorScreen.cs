using System;
using System.Collections.Generic;
using System.Linq;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class InstructorScreen : MonoBehaviour {
    public LevelManager LevelManager;

    private VisualElement _preGameConfigs;


    private Button _initializeSessionButton;
    private Button _startSessionButton;
    private List<SliderInt> _sliders = new List<SliderInt>(); // List to hold slider references
    private DropdownField _dropdownField;
    private List<string> levels;


    private void Start() {
        // Obtain the root visual element of the UXML.
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;
        // get the BridgeDataManager.Level and built an int array that have all the level numbers from 1 up to the BridgeDataManager.Level
        //
        // levels = new int[BridgeDataManager.Level];
        // for (int i = 0; i < BridgeDataManager.Level; i++) {
        //     levels[i] = i + 1;
        // }
        // do the same using LinQ
        levels = Enumerable.Range(1, BridgeDataManager.Level).Select(i => i.ToString()).ToList();
        // levels = Enumerable.Range(1, BridgeDataManager.Level).ToArray();

        // Initialize UI elements and set up change listeners.
        UpdateUIWithCurrentValues(rootVisualElement);
        SetupUIChangeListeners(rootVisualElement);
        // Set up button callbacks
        SetupButtonCallbacks(rootVisualElement);
    }

    private void SetupButtonCallbacks(VisualElement root) {
        _preGameConfigs = root.Q<VisualElement>("pre_game_configs");
        if (_preGameConfigs == null) {
            Debug.LogError("Failed to find 'Pre-Game Configs' container.");
            return;
        }

        _initializeSessionButton = root.Q<Button>("initialize_session_button");
        if (_initializeSessionButton == null) {
            Debug.LogError("Failed to find 'Initialize Session' button.");
            return;
        }

        _startSessionButton = root.Q<Button>("start_session_button");
        if (_startSessionButton == null) {
            Debug.LogError("Failed to find 'Start Game' button.");
            return;
        }

        // initializeSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.InitializeSession());
        _initializeSessionButton.RegisterCallback<ClickEvent>(evt => LevelManager.StartSession());

        _startSessionButton.RegisterCallback<ClickEvent>(evt => LevelManager.StartSession());

        // Initially, the start button should not be interactable.
        _startSessionButton.SetEnabled(false);
    }

    private void OnEnable() {
        BridgeEvents.BridgeStateChanged += OnBridgeStateChange;
    }

    private void OnDisable() {
        BridgeEvents.BridgeStateChanged -= OnBridgeStateChange;
    }

    // provide documentation

    /// <summary>
    /// Handles changes in the state of the bridge, update the UI accordingly.
    /// </summary>
    ///  <param name="state">The new state of the bridge.</param>
    private void OnBridgeStateChange(BridgeStates state) {
        if (state is BridgeStates.Building || state is BridgeStates.BridgeReady ||
            state is BridgeStates.InZeroF || state is BridgeStates.InGame) {
            _initializeSessionButton.SetEnabled(false);
            _startSessionButton.SetEnabled(false);
            SetInteractability(_preGameConfigs, false);
        }
        else {
            _initializeSessionButton.SetEnabled(true);
            _startSessionButton.SetEnabled(true);
            SetInteractability(_preGameConfigs, true);
        }
    }

    /// <summary>
    /// Sets the interactability of all UI elements within a given visual element.
    /// </summary>
    /// <param name="container">The container whose children's interactability will be set.</param>
    /// <param name="enabled">Whether the elements should be enabled (true) or disabled (false).</param>
    private void SetInteractability(VisualElement container, bool enabled) {
        if (container == null) return;

        container.SetEnabled(enabled); // Directly set the enabled state of the container
        foreach (var child in container.Children()) {
            SetInteractability(child, enabled); // Recursively disable/enable child elements
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

            var mvcField = root.Q<IntegerField>("mvc_" + (i + 1));
            if (mvcField != null) {
                mvcField.value = (int)BridgeDataManager.MvcValues[i];
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
            int localIndex = i;  // Create a local copy of the loop variable

            if (_sliders[i] != null) {
                _sliders[i].RegisterValueChangedCallback(evt => UpdateHeight(localIndex, evt.newValue));
            }

            var graceField = root.Q<FloatField>("grace_" + (localIndex + 1));
            if (graceField != null) {
                graceField.RegisterValueChangedCallback(evt => UpdateGrace(localIndex, evt.newValue));
            }

            var mvcField = root.Q<IntegerField>("mvc_" + (localIndex + 1));
            if (mvcField != null) {
                mvcField.RegisterValueChangedCallback(evt => UpdateMvc(localIndex, evt.newValue));
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

        var dropdown = root.Q<DropdownField>("level_picker");
        if (dropdown != null) {
            dropdown.bindingPath = "level";
            dropdown.choices = levels;
            // dropdown.RegisterValueChangedCallback(evt => BridgeDataManager.SetLevel(int.Parse(evt.newValue)));
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
        float[] graces = BridgeDataManager.UnitsGrace;
        graces[index] = newValue;
        BridgeDataManager.SetUnitsGrace(graces);
    }

    /// <summary>
    /// Updates the MVC value for a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newValue">New MVC value.</param>
    private void UpdateMvc(int index, int newValue) {
        float[] mvcValues = BridgeDataManager.MvcValues;
        mvcValues[index] = newValue;
        BridgeDataManager.SetMvcValues(mvcValues);
    }

    /// <summary>
    /// Updates the active state of a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newState">New active state.</param>
    private void UpdateActiveUnit(int index, bool newState) {
        bool[] units = BridgeDataManager.PlayableUnits;
        units[index] = newState;
        BridgeDataManager.SetPlayableUnits(units);
    }
}