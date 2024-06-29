using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MVCForceCollector : MonoBehaviour
{
    [SerializeField] private string preferenceKey; // The key used for PlayerPrefs

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
