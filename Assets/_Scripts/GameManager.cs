using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;



public class GameManager : MonoBehaviour {
    public static GameManager instance;

    [SerializeField] private UnityEvent _startGame;
    [SerializeField] private UnityEvent _endGame;

    

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    public void StartGameInvoke() {
        _startGame.Invoke();
    }
    
    public void EndGameInvoke() {
        _endGame.Invoke();
    }

    private void Start() {

    }


}