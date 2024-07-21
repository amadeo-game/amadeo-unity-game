using UnityEngine;
using UnityEngine.UIElements;

public class InstructorScreen : MonoBehaviour
{
    // Reference to the SessionManager to access and update session data.
    public SessionManager sessionManager;
    
    public LevelManager levelManager;

    private Button initializeSessionButton;
    private Button startSessionButton;

    private void Start()
    {
        // Obtain the root visual element of the UXML.
        var rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        // Initialize UI elements and set up change listeners.
        InitializeAndListenUI(rootVisualElement);
        
        // Set up button callbacks
        SetupButtonCallbacks(rootVisualElement);
        
        SubscribeToGameStateEvents();


    }
    
    private void SetupButtonCallbacks(VisualElement root)
    {
        initializeSessionButton = root.Q<Button>("initialize_session_button");
        if (initializeSessionButton == null)
        {
            Debug.LogError("Failed to find 'Initialize Session' button.");
            return;
        }

        startSessionButton = root.Q<Button>("start_session_button");
        if (startSessionButton == null)
        {
            Debug.LogError("Failed to find 'Start Game' button.");
            return;
        }
        
        initializeSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.InitializeSession());
        startSessionButton.RegisterCallback<ClickEvent>(evt => levelManager.StartSession());

        // Initially, the start button should not be interactable.
        startSessionButton.SetEnabled(false);
    }

    private void SubscribeToGameStateEvents()
    {
        GameStatesEvents.GameSessionInitialized += () => 
        {
            initializeSessionButton.SetEnabled(false);
            startSessionButton.SetEnabled(true);
        };

        GameStatesEvents.GameSessionStarted += () =>
        {
            initializeSessionButton.SetEnabled(false);
            startSessionButton.SetEnabled(false);
        };

        GameStatesEvents.GameSessionEnded += () =>
        {
            // Both buttons are made interactable again when the game session ends
            initializeSessionButton.SetEnabled(true);
            startSessionButton.SetEnabled(true);
        };
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
        GameStatesEvents.GameSessionInitialized -= () => initializeSessionButton.SetEnabled(false);
        GameStatesEvents.GameSessionStarted -= () => startSessionButton.SetEnabled(false);
        GameStatesEvents.GameSessionEnded -= () => 
        {
            initializeSessionButton.SetEnabled(true);
            startSessionButton.SetEnabled(true);
        };
    }

    /// <summary>
    /// Initializes all UI elements with values from SessionManager and sets up listeners to handle changes.
    /// </summary>
    /// <param name="root">The root visual element of the UI document.</param>
    private void InitializeAndListenUI(VisualElement root)
    {
        // Initialize sliders for height and register change listeners.
        for (int i = 0; i < 5; i++)
        {
            int localIndex = i;  // Local copy of the loop variable

            var slider = root.Q<SliderInt>("height_" + (localIndex + 1));
            slider.value = sessionManager.Heights[localIndex];
            slider.RegisterValueChangedCallback(evt => UpdateHeight(localIndex, evt.newValue));

            var graceField = root.Q<FloatField>("grace_" + (localIndex + 1));
            graceField.value = sessionManager.UnitsGrace[localIndex];
            graceField.RegisterValueChangedCallback(evt => UpdateGrace(localIndex, evt.newValue));

            var mvcField = root.Q<IntegerField>("mvc_" + (localIndex + 1));
            mvcField.value = (int)sessionManager.MvcValues[localIndex];
            mvcField.RegisterValueChangedCallback(evt => UpdateMvc(localIndex, evt.newValue));

            var toggle = root.Q<Toggle>("active_unit_" + (localIndex + 1));
            toggle.value = sessionManager.PlayableUnits[localIndex];
            toggle.RegisterValueChangedCallback(evt => UpdateActiveUnit(localIndex, evt.newValue));
        }

        // Initialize and listen for changes in time duration.
        var timeField = root.Q<IntegerField>("time_int_field");
        timeField.value = (int)sessionManager.TimeDuration;
        timeField.RegisterValueChangedCallback(evt => sessionManager.SetTimeDuration(evt.newValue));

        // Initialize and listen for changes in left-hand and flexion toggles.
        InitializeAndListenToggle(root, "left_hand_toggle", sessionManager.IsLeftHand, sessionManager.SetIsLeftHand);
        InitializeAndListenToggle(root, "is_flexion_toggle", sessionManager.IsFlexion, sessionManager.SetIsFlexion);
    }

    /// <summary>
    /// Updates the specified height in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the height to update.</param>
    /// <param name="newValue">New value for the height.</param>
    private void UpdateHeight(int index, int newValue)
    {
        int[] heights = sessionManager.Heights;
        heights[index] = newValue;
        sessionManager.SetHeights(heights);
    }

    /// <summary>
    /// Updates the grace period for a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newValue">New grace value.</param>
    private void UpdateGrace(int index, float newValue)
    {
        float[] graces = sessionManager.UnitsGrace;
        graces[index] = newValue;
        sessionManager.SetUnitsGrace(graces);
    }

    /// <summary>
    /// Updates the MVC value for a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newValue">New MVC value.</param>
    private void UpdateMvc(int index, int newValue)
    {
        float[] mvcValues = sessionManager.MvcValues;
        mvcValues[index] = newValue;
        sessionManager.SetMvcValues(mvcValues);
    }

    /// <summary>
    /// Updates the active state of a unit in the SessionManager.
    /// </summary>
    /// <param name="index">Index of the unit to update.</param>
    /// <param name="newState">New active state.</param>
    private void UpdateActiveUnit(int index, bool newState)
    {
        bool[] units = sessionManager.PlayableUnits;
        units[index] = newState;
        sessionManager.SetPlayableUnits(units);
    }

    /// <summary>
    /// Initializes a toggle with a value and sets up a listener for changes.
    /// </summary>
    /// <param name="root">The root visual element.</param>
    /// <param name="toggleName">The name of the toggle element.</param>
    /// <param name="initialValue">Initial value of the toggle.</param>
    /// <param name="updateAction">Action to perform on value change.</param>
    private void InitializeAndListenToggle(VisualElement root, string toggleName, bool initialValue, System.Action<bool> updateAction)
    {
        var toggle = root.Q<Toggle>(toggleName);
        toggle.value = initialValue;
        toggle.RegisterValueChangedCallback(evt => updateAction(evt.newValue));
    }
}
