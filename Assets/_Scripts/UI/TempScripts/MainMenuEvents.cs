using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour {
    private UIDocument _document;
    private Button _button;


    private void Awake() {
        _document = GetComponent<UIDocument>();
        _button = _document.rootVisualElement.Q("StartGameButton") as Button;
        _button.RegisterCallback<ClickEvent>(OnPlayGameClicked);
    }

    private void OnDisable() {
        _button.UnregisterCallback<ClickEvent>(OnPlayGameClicked);
    }

    private void OnPlayGameClicked(ClickEvent evt) {
        Debug.Log("Play game clicked");
    }
    
}
