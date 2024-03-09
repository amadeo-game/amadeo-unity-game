using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private static Dictionary<FingerUnit, bool> dicUnitPlaced;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
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