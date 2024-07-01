using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PortNumber : MonoBehaviour {
    private int portNumber;

    private void Awake() {
        int portSavedValue = PlayerPrefs.GetInt("portNumber", 4444);
        gameObject.GetComponent<TMP_InputField>().text = portSavedValue.ToString();
        PlayerPrefs.SetInt("portNumber", portSavedValue);
        PlayerPrefs.Save();
    }


    public void OnInputFieldChanged(string number) {
        if (string.IsNullOrEmpty(number) || number.Length != 4 || !int.TryParse(number, out _)) {
            Debug.Log("Invalid Port Number: Must be 4 digits");
        }

        portNumber = int.Parse(number);
        Debug.Log(portNumber);
        PlayerPrefs.SetInt("portNumber", portNumber);
    }
}