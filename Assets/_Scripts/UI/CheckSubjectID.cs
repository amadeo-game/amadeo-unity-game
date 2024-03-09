using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class CheckSubjectID : MonoBehaviour
{
    [SerializeField] TMP_InputField subjectID;
    [SerializeField] TextMeshProUGUI errorText;
    [SerializeField] private string errorMessage = "Subject ID doesn't exist";
    [SerializeField] private GameObject form;

    private bool idIsValid;

    public void ValidateID() {
        if (subjectID.text.Length == 4) {
            idIsValid = true;
            // check for subject folder existence
        }
        else {
            idIsValid = false;
        }
    }
    // check if the subject ID is valid, should be 4 digits
    public void CheckID() {
        if (idIsValid && SubjectExists()) {
            errorText.gameObject.SetActive(false);
            form.SetActive(true);
        }
        else {
            errorText.gameObject.SetActive(true);
        }
    }
    
    private bool SubjectExists() {
        string patientID = subjectID.text;
        string subjectFolderPath = Path.Combine(Application.dataPath, "Model/Subjects", patientID);
        return Directory.Exists(subjectFolderPath);
    }

    private void OnEnable() {
        errorText.text = errorMessage;
        errorText.gameObject.SetActive(false);
    }

    private void OnDisable() {
        subjectID.text = String.Empty;
        errorText.gameObject.SetActive(false);
        form.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}


