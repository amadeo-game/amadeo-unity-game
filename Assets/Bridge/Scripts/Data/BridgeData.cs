﻿using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BridgePackage {
    [Serializable]
    public class BridgeData {
        public float TimeDuration = 60f;
        public int[] heights;
        public BridgeCollectionSO bridgeCollection;
        public int level;
        public bool isLeftHand;
        public bool isFlexion;
        public float[] mvcValuesExtension;
        public float[] mvcValuesFlexion;
        public bool[] playableUnits;
        public float[] unitsGrace;
        public bool zeroF;
        [FormerlySerializedAs("autoPlay")] public bool autoStart;
        public SessionData SessionData;


        // constructor, starting values
        public BridgeData() {
            // constructor, starting values
            this.SessionData = new SessionData();
            this.TimeDuration = 60f;
            this.heights = new int[5] { 0, 1, 0, 0, 0 };
            this.bridgeCollection = null;
            this.level = 1;
            this.isLeftHand = true;
            this.isFlexion = true;
            this.mvcValuesExtension = new float[5] { 20, 20, 20, 20, 20 };
            this.mvcValuesFlexion = new float[5] { 20, 20, 20, 20, 20 };
            this.playableUnits = new bool[5] { false, true, false, false, false};
            this.unitsGrace = new float[5] { 1, 1, 1, 1, 1 };
            this.zeroF = true;
            this.autoStart = true;
        }

        public string ToJson() {
            return JsonUtility.ToJson(this);
        }

        public void LoadJson(string jsonFilepath) {
            JsonUtility.FromJsonOverwrite(jsonFilepath, this);
        }
    }

    public struct SessionData {
        public int[] heights;
        public float[] BestYPositions;
        public bool success;
    }
}