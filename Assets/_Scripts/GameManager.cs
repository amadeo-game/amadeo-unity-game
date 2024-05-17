using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum FingerUnit {
    first,
    second,
    third,
    fourth,
    fifth,
}

public class GameManager : MonoBehaviour {
    public static GameManager instance;
    public static event Action OnUnitsPlaced;

    [SerializeField] private UnityEvent _startGame;
    [SerializeField] private UnityEvent _endGame;

    
    private static Dictionary<FingerUnit, bool> dicUnitPlaced;

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
        dicUnitPlaced = new Dictionary<FingerUnit, bool> {
            { FingerUnit.first, false },
            { FingerUnit.second, false },
            { FingerUnit.third, false },
            { FingerUnit.fourth, false },
            { FingerUnit.fifth, false },
        };
    }

    public static void OnOnUnitsPlaced(FingerUnit fingerUnit, bool isPlaced) {
        if (dicUnitPlaced.ContainsKey(fingerUnit)) {
            dicUnitPlaced[fingerUnit] = isPlaced;
        }
        
        // dicUnitPlaced.Keys.ToList().ForEach(x => Debug.Log(x + " : " + dicUnitPlaced[x]));

        if (!dicUnitPlaced.Values.Contains(false)) {
            OnUnitsPlaced?.Invoke();
        }
    }
}