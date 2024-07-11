using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PortNumber : MonoBehaviour {
    private int portNumber;
    private readonly int defaultPortNumber = 4444;
    [SerializeField] TMP_Text textMeshPro;


    private void OnEnable() {
        portNumber = PlayerPrefs.GetInt("portNumber", defaultPortNumber);
        if (portNumber < 1024 || portNumber > 49151) {
            portNumber = defaultPortNumber;
            PlayerPrefs.SetInt("portNumber", portNumber);
            PlayerPrefs.Save();
        }
        gameObject.GetComponent<TMP_InputField>().text = portNumber.ToString();
        UpdateSelectedPortUI();
    }


    public void OnInputFieldChanged(string number) {
        if (string.IsNullOrEmpty(number) || number.Length != 4 || !int.TryParse(number, out _)) {
            Debug.Log("Invalid Port Number: Must be 4 digits");
            return;
        }

        portNumber = int.Parse(number);
        Debug.Log(portNumber);
        PlayerPrefs.SetInt("portNumber", portNumber);
        PlayerPrefs.Save();
    }

    public void OnButtonClick() {
        ServerAPI.Instance.SetPortNumber(portNumber);
        UpdateSelectedPortUI();
    }

    private void UpdateSelectedPortUI() {
        textMeshPro.text = "Selected Port: " + portNumber;
    }
}