using System;
using BridgePackage;
using UnityEngine;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeDataManager))]
    public class UnitsControl : MonoBehaviour {
       private GameObject[] playerUnits;
        private MoveUnit[] moveUnits; // Array to store MoveUnit components
        private BridgeDataManager bridgeDataManager;
        private float[] leftHandHighestY = new float[5];
        private float[] rightHandHighestY = new float[5];

        private bool controlEnabled;

        float[] forces = new float[10];

        private void Start() {
            controlEnabled = false;
            bridgeDataManager = GetComponent<BridgeDataManager>();
        }

        private void OnEnable() {
            BridgeEvents.ForcesUpdated += UpdateForces;
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            controlEnabled = state == BridgeStates.InGame;
            Debug.Log("UnitsControl :: OnBridgeStateChanged: ControlEnabled is " + controlEnabled);
        }


        private void OnDisable() {
            BridgeEvents.ForcesUpdated -= UpdateForces;
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
        }

        private void UpdateForces(float[] forces) {
            this.forces = forces;
        }

        public void SetPlayerUnits(GameObject[] units) {
            Debug.Log("UnitsControl :: SetPlayerUnits called");
            playerUnits = units;
            // Set FingerUnit enum using setter for each player Unit
            for (int i = 0; i < playerUnits.Length; i++) {
                playerUnits[i].GetComponent<UnitReachedDestination>().SetFingerUnit((FingerUnit)i);
            }
            
            AssignUnitControlScripts(); // Ensure this is called when setting player units
        }

        private void AssignUnitControlScripts() {
            moveUnits = new MoveUnit[playerUnits.Length]; // Initialize the array
            for (int i = 0; i < playerUnits.Length; i++) {
                var unit = playerUnits[i];
                if (unit != null) {
                    var moveUnit = unit.GetComponent<MoveUnit>();
                    if (moveUnit == null) {
                        playerUnits[i].AddComponent<MoveUnit>();
                    }

                    moveUnits[i] = moveUnit; // Store the reference
                }
            }
        }

        private void FixedUpdate() {
            if (!controlEnabled) {
                return;
            }

            // ApplyForces();
        }

        public void ApplyForces() {
            var isLeft = BridgeDataManager.IsLeftHand;
            double[] forcesByHand = new double[5];
            int len = forces.Length;
            for (int i = 5; i < len; i++) {
                int index = isLeft ? len - 1 - i : i - 5;
                forcesByHand[index] = forces[i];
                float currentForce = (float)forces[i];
                if (isLeft) {
                    leftHandHighestY[index] = Mathf.Max(currentForce, leftHandHighestY[index]);
                }
                else {
                    rightHandHighestY[index] = Mathf.Max(currentForce, rightHandHighestY[index]);
                }
            }
            Debug.Log("UnitsControl :: forces: " + string.Join(", ", forces));
                        Debug.Log("UnitsControl :: forces: " + string.Join(", ", forces));

            if (moveUnits == null || moveUnits.Length == 0) return;
            Debug.Log("UnitsControl :: forcesByHand: " + string.Join(", ", forcesByHand));

            for (int i = 0; i < moveUnits.Length; i++) {
                var moveUnit = moveUnits[i];
                if (moveUnit != null) {
                    Debug.Log($"UnitsControl :: Applying force to unit: {i}");
                    moveUnit.ApplyForce((float)forcesByHand[i]);
                    Debug.Log($"UnitsControl :: Applied force to unit: {i} force: {(float)forcesByHand[i]}");
                }
            }
        }

        public void DisableControl() { }
    }
}