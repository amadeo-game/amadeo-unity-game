﻿using System;
using UnityEngine;

namespace BridgePackage {
    [Serializable]
    public class BridgeData {
        public float TimeDuration = 60f;
        public int[] heights;
        public BridgeTypeSO bridgeTypeSO;
        public bool isLeftHand;
        public bool isFlexion;
        public float[] mvcValues;
        public bool[] playableUnits;
        public float[] unitsGrace;
        public bool zeroF;
        public bool autoPlay;


        // constructor, starting values
        public BridgeData() {
            // constructor, starting values
            this.TimeDuration = 60f;
            this.heights = new int[5] { 0, 0, 0, 0, 0 };
            this.bridgeTypeSO = null;
            this.isLeftHand = true;
            this.isFlexion = true;
            this.mvcValues = new float[5] { 1, 1, 1, 1, 1 };
            this.playableUnits = new bool[5] { false, false, false, true, true };
            this.unitsGrace = new float[5] { 0, 0, 0, 0, 0 };
            this.zeroF = false;
            this.autoPlay = false;
        }
        
        
        
        // setter for isLeftHand
        public void SetIsLeftHand(bool leftHand) {
            this.isLeftHand = leftHand;
        }

        public string ToJson() {
            return JsonUtility.ToJson(this);
        }

        public void LoadJson(string jsonFilepath) {
            JsonUtility.FromJsonOverwrite(jsonFilepath, this);
        }
    }
}