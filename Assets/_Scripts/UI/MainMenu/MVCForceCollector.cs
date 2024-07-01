using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MVCForceCollector : MonoBehaviour
{
    [SerializeField] private string preferenceKey; // The key used for PlayerPrefs
    private bool isFlextion = true;

    private void Awake() {
        if (preferenceKey != null) {
            Debug.Log("preferenceKey: " + preferenceKey + "Starts with + E" + preferenceKey.StartsWith("E"));
            if (preferenceKey.StartsWith("E")) {
                isFlextion = false;
                Debug.Log("isFlextion: " + isFlextion);
            }
        }
    }

    private void OnEnable() {
        
        int defaultValue = isFlextion ? -5 : 5;
        // Get the saved value from PlayerPrefs
        float savedValue = PlayerPrefs.GetFloat(preferenceKey, defaultValue);
        // Set the input field text to the saved value
        gameObject.GetComponent<TMP_InputField>().text = savedValue.ToString();
    }
    // private TMP_InputField inputField;


   

    public void OnInputFieldChanged(string number)
    {   
        if( string.IsNullOrEmpty(number) ||!float.TryParse(number, out _) )
        {
            Debug.Log("Invalid input: Must be an integer");
        }
        // Check if the input is a valid integer
        else
        {
            // Save the valid integer input using PlayerPrefs
            PlayerPrefs.SetFloat(preferenceKey, float.Parse(number));
            PlayerPrefs.Save(); // Ensure the data is saved immediately
            Debug.Log($"Saved value for {preferenceKey}: {number}");
        }
       
    }
}
