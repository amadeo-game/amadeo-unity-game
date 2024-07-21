using System;
using UnityEngine;

public class SessionManager : MonoBehaviour {
    [SerializeField] private float initialTimeDuration = 60f;
    private int[] heights = new int[5] { 0, 0, 0, 0, 0 };
    private BridgeTypeSO bridgeTypeSO;
    private bool isLeftHand = true;
    private bool isFlexion = true;
    private float[] mvcValues = new float[5] { 1, 1, 1, 1, 1 };
    private bool[] playableUnits = new bool[5] { false, false, false, true, true };
    private float[] unitsGrace = new float[5] { 0, 0, 0, 0, 0 };
    
    private bool zeroF = false;
    private bool autoPlay = false;

    public int[] Heights => heights;
    public BridgeTypeSO BridgeType => bridgeTypeSO;
    public bool IsLeftHand => isLeftHand;
    public bool IsFlexion => isFlexion;
    public float[] MvcValues => mvcValues;
    public bool[] PlayableUnits => playableUnits;
    public float[] UnitsGrace => unitsGrace;
    public float TimeDuration => initialTimeDuration;
    
    public bool ZeroF => zeroF;
    
    public bool AutoPlay => autoPlay;

    public void SetHeights(int[] newHeights) {
        if (newHeights.Length == 5) {
            heights = newHeights;
        }
    }

    public void SetBridgeType(BridgeTypeSO newBridgeType) {
        bridgeTypeSO = newBridgeType;
    }

    public void SetIsLeftHand(bool leftHand) {
        isLeftHand = leftHand;
    }

    public void SetIsFlexion(bool flexion) {
        isFlexion = flexion;
    }

    public void SetMvcValues(float[] newMvcValues) {
        if (newMvcValues.Length == 5) {
            mvcValues = newMvcValues;
        }
    }

    public void SetPlayableUnits(bool[] newPlayableUnits) {
        if (newPlayableUnits.Length == 5) {
            playableUnits = newPlayableUnits;
        }
    }

    public void SetTimeDuration(float newTimeDuration) {
        initialTimeDuration = newTimeDuration;
    }

    public void SetUnitsGrace(float[] floats) {
        if (floats.Length == 5) {
            unitsGrace = floats;
        }
    }
    
    public void SetZeroF(bool newZeroF) {
        zeroF = newZeroF;
    }
    
    public void SetAutoPlay(bool newAutoPlay) {
        autoPlay = newAutoPlay;
    }
}