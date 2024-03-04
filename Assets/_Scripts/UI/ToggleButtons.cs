using System;
using UnityEngine;
using UnityEngine.UI;

public class ToggleButtons : MonoBehaviour {
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    
    [SerializeField] private Color activeColor; // Set this color in the Inspector
    private Color defaultColor; // Store the default color
    
    
    private bool button1Pressed = true;
    public void Start() {
        defaultColor = button1.GetComponent<Image>().color;
        OnButtonClick();
    }
    
    public void OnButtonClick() {
        button1Pressed = !button1Pressed;
        String buttonPressed = button1Pressed ? "Left" : "Right";
        Debug.Log("Button pressed: " + buttonPressed);

        button1.GetComponent<Image>().color = button1Pressed ? activeColor : defaultColor;

        button2.GetComponent<Image>().color = button1Pressed ?  defaultColor : activeColor;
    }
}