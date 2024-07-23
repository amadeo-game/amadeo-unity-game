using System.Collections.Generic;
using BridgePackage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class InstructorScreen : MonoBehaviour {
    // Reference to the SessionManager to access and update session data.
    [FormerlySerializedAs("sessionManager")] public BridgeDataManager BridgeDataManager;

    public LevelManager levelManager;

    private VisualElement preGameConfigs;


    private Button initializeSessionButton;
    private Button startSessionButton;
    private List<SliderInt> sliders = new List<SliderInt>(); // List to hold slider references


    private void Start() {
        // Obtain the root visual element of the UXML.
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        // Initialize UI elements and set up change listeners.
        InitializeAndListenUI(rootVisualElement);

        // Set up button callbacks
        SetupButtonCallbacks(rootVisualElement);


        SubscribeToGameStateEvents();
    }

    private void SetupButtonCallbacks(VisualElement root) {
        preGameConfigs = root.Q<VisualElement>("pre_game_configs");
        if (preGameConfigs == null) {
            Debug.LogError("Failed to find 'Pre-Game Configs' container.");
            return;
        }

        initializeSessionButton = root.Q<Button>("initialize_session_button");
        if (initializeSessionButton == null) {
            Debug.LogError("Failed to find 'Initialize Session' button.");
            return;
        }

        startSessionButton = root.Q<Button>("start_session_button");
        if (startSessionButton == null) {
            Debug.LogError("Failed to find 'Start Game' button.");
            return;
        }

        // initializeSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.InitializeSession());
        initializeSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.StartSession());

        startSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.StartSession());

        // Initially, the start button should not be interactable.
        startSessionButton.SetEnabled(false);
    }

    private void SubscribeToGameStateEvents() {
        GameStatesEvents.GameSessionInitialized += () => {
            initializeSessionButton.SetEnabled(false);
            startSessionButton.SetEnabled(true);
            SetInteractability(preGameConfigs, true);
        };

        GameStatesEvents.GameSessionStarted += () => {
            initializeSessionButton.SetEnabled(false);
            startSessionButton.SetEnabled(false);
            SetInteractability(preGameConfigs, false);
        };

        GameStatesEvents.GameSessionEnded += () => {
            // Both buttons are made interactable again when the game session ends
            initializeSessionButton.SetEnabled(true);
            startSessionButton.SetEnabled(true);
            SetInteractability(preGameConfigs, true);
        };
    }

    private void OnDestroy() {
        // Unsubscribe from events to prevent memory leaks
        GameStatesEvents.GameSessionInitialized -= () => initializeSessionButton.SetEnabled(false);
        GameStatesEvents.GameSessionStarted -= () => startSessionButton.SetEnabled(false);
        GameStatesEvents.GameSessionEnded -= () => {
            initializeSessionButton.SetEnabled(true);
            startSessionButton.SetEnabled(true);
        };
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
    /// Initializes all UI elements with values from SessionManager and sets up listeners to handle changes.
    /// </summary>
    /// <param name="root">The root visual element of the UI document.</param>
    private void InitializeAndListenUI(VisualElement root) {
        // Initialize sliders for height and register change listeners.
        for (int i = 0; i < 5; i++) {
            int localIndex = i; // Local copy of the loop variable

            var slider = root.Q<SliderInt>("height_" + (localIndex + 1));
            slider.value = BridgeDataManager.Heights[localIndex];
            slider.RegisterValueChangedCallback(evt => UpdateHeight(localIndex, evt.newValue));

            var graceField = root.Q<FloatField>("grace_" + (localIndex + 1));
            graceField.value = BridgeDataManager.UnitsGrace[localIndex];
            graceField.RegisterValueChangedCallback(evt => UpdateGrace(localIndex, evt.newValue));

            var mvcField = root.Q<IntegerField>("mvc_" + (localIndex + 1));
            mvcField.value = (int)BridgeDataManager.MvcValues[localIndex];
            mvcField.RegisterValueChangedCallback(evt => UpdateMvc(localIndex, evt.newValue));

            var toggle = root.Q<Toggle>("active_unit_" + (localIndex + 1));
            toggle.value = BridgeDataManager.PlayableUnits[localIndex];
            toggle.RegisterValueChangedCallback(evt => UpdateActiveUnit(localIndex, evt.newValue));
        }

        // Initialize and listen for changes in time duration.
        var timeField = root.Q<IntegerField>("time_int_field");
        timeField.value = (int)BridgeDataManager.TimeDuration;
        timeField.RegisterValueChangedCallback(evt => BridgeDataManager.SetTimeDuration(evt.newValue));

        // Initialize and listen for changes in toggle listeners

        InitializeAndListenToggle(root, "left_hand_toggle", BridgeDataManager.IsLeftHand, BridgeDataManager.SetIsLeftHand);
        InitializeAndListenToggle(root, "is_flexion_toggle", BridgeDataManager.IsFlexion, SetFlexion);

        InitializeAndListenToggle(root, "zero_f_toggle", BridgeDataManager.ZeroF, val => BridgeDataManager.SetZeroF(val));
        InitializeAndListenToggle(root, "auto_start_toggle", BridgeDataManager.AutoPlay,
            val => BridgeDataManager.SetAutoPlay(val));

        // Query and store references to all sliders
        for (int i = 1; i <= 5; i++) {
            SliderInt slider = root.Q<SliderInt>("height_" + i);
            if (slider != null) {
                sliders.Add(slider);
                SetSliderRotation(slider, BridgeDataManager.IsFlexion); // Set initial rotation based on the flexion state
            }
        }
    }

    /// <summary>
    /// Sets or unsets the flexion state and adjusts slider rotations accordingly.
    /// </summary>
    private void SetFlexion(bool isFlexion) {
        BridgeDataManager.SetIsFlexion(isFlexion);
        foreach (SliderInt slider in sliders) {
            SetSliderRotation(slider, isFlexion);
        }
    }

    /// <summary>
    /// Sets the rotation of a slider based on the flexion state.
    /// </summary>
    private void SetSliderRotation(SliderInt slider, bool isFlexion)
    {
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

    /// <summary>
    /// Initializes a toggle with a value and sets up a listener for changes.
    /// </summary>
    /// <param name="root">The root visual element.</param>
    /// <param name="toggleName">The name of the toggle element.</param>
    /// <param name="initialValue">Initial value of the toggle.</param>
    /// <param name="updateAction">Action to perform on value change.</param>
    private void InitializeAndListenToggle(VisualElement root, string toggleName, bool initialValue,
        System.Action<bool> updateAction) {
        var toggle = root.Q<Toggle>(toggleName);
        toggle.value = initialValue;
        toggle.RegisterValueChangedCallback(evt => updateAction(evt.newValue));
    }
}