using System;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.Serialization;

public class SubjectSelectionManager : MonoBehaviour {
    public enum SubjectSelectionState {
        Initial,
        CheckId,
        NewSubject
    };
    
    public enum HandSelection {
        Left,
        Right
    }

    [SerializeField] private SubjectSelectionState currentState;

    

    // TextMeshPro button Type
    [SerializeField] private TMP_Text panelTitle;

    [Header("Subject ID check fields")] [Tooltip("Reference to the Subject ID input field in the UI")] [SerializeField]
    private TMP_InputField subjectIDInput;

    [SerializeField] private Button checkButton;
    [SerializeField] private TMP_Text subjectIdFeedbackMessage;

    [Header("Session details fields")] [SerializeField]
    private GameObject subjectSessionDetails; // Reference to entire SubjectSessionDetails parent

    [SerializeField] private TMP_InputField sessionIDInput;
    [SerializeField] private TMP_Text sessionIdFeedbackMessage;
    [SerializeField]  private TMP_Dropdown handSelectionDropdown;
    [SerializeField] private Button acceptButton;

    [Header("Back button")] [SerializeField]
    private Button backButton;
    
    private HandSelection currentHandSelection = HandSelection.Left;
    void Start() {
        currentState = SubjectSelectionState.Initial;
        checkButton.interactable = false;
        subjectSessionDetails.SetActive(false); // Initially hide session details
        subjectIdFeedbackMessage.text = "";
        sessionIdFeedbackMessage.text = "";
    }

    public void OnInputFieldChanged() {
        string subjectID = subjectIDInput.text;

        if (string.IsNullOrEmpty(subjectID) || subjectID.Length != 4 || !int.TryParse(subjectID, out _)) {
            subjectIdFeedbackMessage.text = "Invalid Subject ID: Must be 4 digits";
            checkButton.interactable = false;
            return;
        }

        bool subjectExists = CheckSubjectExists(subjectID);

        if (subjectExists) {
            currentState = SubjectSelectionState.CheckId;
            subjectIdFeedbackMessage.text = "Subject Found (ID: " + subjectID + ")";
        }
        else {
            currentState = SubjectSelectionState.NewSubject;
            subjectIdFeedbackMessage.text = "New Subject (ID: " + subjectID + ")";
        }

        checkButton.interactable = true;
    }

    public void OnCheckButtonClicked() {
        ShowSessionDetails(true); // Enable session details for existing subject
        subjectIDInput.interactable = false;
        checkButton.interactable = false;
    }

    private bool CheckSubjectExists(string subjectID) {
        string subjectFolderPath = Path.Combine(Application.dataPath, "Model/Subjects", subjectID);
        return Directory.Exists(subjectFolderPath);
    }

    public void OnAcceptButtonClicked() {
        // Check if the session ID is valid
        string sessionID = sessionIDInput.text;
        if (string.IsNullOrEmpty(sessionID)) {
            sessionIdFeedbackMessage.text = "Please enter a Session ID";
            return;
        }

        // TODO: Replace with dropdown logic here
        string handSelection =
            currentHandSelection == HandSelection.Left ? "Left" : "Right"; // Get hand selection

        // Create subject folder and session subfolder
        var createdSubjectFolder = CreateSubjectFolder(subjectIDInput.text, sessionID);
        if (!createdSubjectFolder.Item1) {
            sessionIdFeedbackMessage.text = createdSubjectFolder.Item2;
            return;
        }

        Debug.Log("Starting session for subject: " + subjectIDInput.text + ", Session: " + sessionID +
                  ", Hand: " + handSelection);

        // Transition to trial management state
        MainMenu.Instance.ChangeState(MainMenuState.SelectTrials);
    }

    private Tuple<bool, string> CreateSubjectFolder(string subjectID, string sessionID) {
        string subjectFolderPath = Path.Combine(Application.dataPath, "Model/Subjects", subjectID);
        string sessionFolderPath = Path.Combine(subjectFolderPath, sessionID);

        if (!Directory.Exists(subjectFolderPath)) {
            Directory.CreateDirectory(subjectFolderPath);
        }

        if (Directory.Exists(sessionFolderPath)) {
            return Tuple.Create(false, "Session folder already exists for this subject");
        }

        DirectoryInfo directoryInfo = Directory.CreateDirectory(sessionFolderPath);
        // check if the session folder was created successfully
        if (directoryInfo.Exists) {
            Debug.Log("Session folder created: " + sessionFolderPath);
        }
        else {
            Debug.LogError("Failed to create session folder: " + sessionFolderPath);
        }

        return Tuple.Create(true, "");
    }

    public void OnBackButtonClicked() {
        subjectIDInput.text = "";
        // panelTitle.text = "Prepare Subject";
        subjectIdFeedbackMessage.text = "";
        sessionIdFeedbackMessage.text = "";
        subjectIDInput.interactable = true;
        checkButton.interactable = true;
        ShowSessionDetails(false);

        if (currentState is not SubjectSelectionState.Initial) {
            currentState = SubjectSelectionState.Initial;
        }
        else {
            MainMenu.Instance.ChangeState(MainMenuState.MainMenu);
        }
    }
    
    public void OnHandSelectionDropdownChanged() {
        currentHandSelection = handSelectionDropdown.value == 0 ? HandSelection.Left : HandSelection.Right;
        Debug.Log("Hand selection changed to: " + currentHandSelection);
        // Debug.Log("Hand selection dropdown value after cast: " + value);
    }
    

    private void ShowSessionDetails(bool show) {
        subjectSessionDetails.SetActive(show);
        // Add more UI logic if needed
    }
}