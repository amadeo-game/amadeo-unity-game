using System;
using UnityEngine;

namespace BridgePackage {
    internal class UnitsControl : MonoBehaviour {
        [SerializeField] private InputReader inputReader;
        private float MaxHeight { get; set; } = 5f;

        private bool isUnitsSet = false;

        private Transform unit1Transform;
        private Transform unit2Transform;
        private Transform unit3Transform;
        private Transform unit4Transform;
        private Transform unit5Transform;

        private Rigidbody2D rb1;
        private Rigidbody2D rb2;
        private Rigidbody2D rb3;
        private Rigidbody2D rb4;
        private Rigidbody2D rb5;

        private Vector2 previous1MovementInput;
        private Vector2 previous2MovementInput;
        private Vector2 previous3MovementInput;
        private Vector2 previous4MovementInput;
        private Vector2 previous5MovementInput;

        // Store the forces for each unit
        private float[] unitForces = new float[5];

        internal void SetPlayerUnits(GameObject[] gameUnits) {
            if (gameUnits.Length != 5) {
                throw new ArgumentException("gameUnits must be length 5.");
            }

            unit1Transform = gameUnits[0].transform;
            unit2Transform = gameUnits[1].transform;
            unit3Transform = gameUnits[2].transform;
            unit4Transform = gameUnits[3].transform;
            unit5Transform = gameUnits[4].transform;

            rb1 = gameUnits[0].GetComponent<Rigidbody2D>();
            rb2 = gameUnits[1].GetComponent<Rigidbody2D>();
            rb3 = gameUnits[2].GetComponent<Rigidbody2D>();
            rb4 = gameUnits[3].GetComponent<Rigidbody2D>();
            rb5 = gameUnits[4].GetComponent<Rigidbody2D>();

            isUnitsSet = true;
        }

        internal void DisableControl() {
            Debug.Log("Stop moving units.");
            isUnitsSet = false;
        }

        private void OnEnable() {
            try {
                isLeftHand = PlayerPrefs.GetInt("IsLeftHand") == 1;
            }
            catch (Exception e) {
                Debug.Log(e);
            }

            if (isLeftHand) {
                inputReader.OnLF1Event += HandleMoveUnit1;
                inputReader.OnLF2Event += HandleMoveUnit2;
                inputReader.OnLF3Event += HandleMoveUnit3;
                inputReader.OnLF4Event += HandleMoveUnit4;
                inputReader.OnLF5Event += HandleMoveUnit5;
            }
            else {
                inputReader.OnRF1Event += HandleMoveUnit1;
                inputReader.OnRF2Event += HandleMoveUnit2;
                inputReader.OnRF3Event += HandleMoveUnit3;
                inputReader.OnRF4Event += HandleMoveUnit4;
                inputReader.OnRF5Event += HandleMoveUnit5;
            }
        }

        private void OnDisable() {
            if (isLeftHand) {
                inputReader.OnLF1Event -= HandleMoveUnit1;
                inputReader.OnLF2Event -= HandleMoveUnit2;
                inputReader.OnLF3Event -= HandleMoveUnit3;
                inputReader.OnLF4Event -= HandleMoveUnit4;
                inputReader.OnLF5Event -= HandleMoveUnit5;
            }
            else {
                inputReader.OnRF1Event -= HandleMoveUnit1;
                inputReader.OnRF2Event -= HandleMoveUnit2;
                inputReader.OnRF3Event -= HandleMoveUnit3;
                inputReader.OnRF4Event -= HandleMoveUnit4;
                inputReader.OnRF5Event -= HandleMoveUnit5;
            }
        }

        private bool isLeftHand = false;

        //[FormerlySerializedAs("moveSpeed")] [Header("Settings")] [SerializeField]
        private float maxHeight = 4f;

        // Method to handle the move event for each unit
        private void HandleMoveUnit1(Vector2 value) {
            previous1MovementInput = value;
        }

        private void HandleMoveUnit2(Vector2 value) {
            previous2MovementInput = value;
        }

        private void HandleMoveUnit3(Vector2 value) {
            previous3MovementInput = value;
        }

        private void HandleMoveUnit4(Vector2 value) {
            previous4MovementInput = value;
        }

        private void HandleMoveUnit5(Vector2 value) {
            previous5MovementInput = value;
        }

        // FixedUpdate is called once per physics frame
        void FixedUpdate() {
            if (!isUnitsSet) return;

            // Use the forces to move the units
            MoveUnit(rb1, unit1Transform, unitForces[0]);
            MoveUnit(rb2, unit2Transform, unitForces[1]);
            MoveUnit(rb3, unit3Transform, unitForces[2]);
            MoveUnit(rb4, unit4Transform, unitForces[3]);
            MoveUnit(rb5, unit5Transform, unitForces[4]);
        }

        // Modified MoveUnit to use both input and forces
        private void MoveUnit(Rigidbody2D rb, Transform unitTransform, float forceY) {
            if (rb == null) return;
            float MVC_Value = 1f;
            // Validate forceY to ensure it is a valid number
            if (float.IsNaN(forceY) || float.IsInfinity(forceY)) {
                Debug.LogError($"Invalid force value: {forceY}");
                return;
            }

            // Calculate the new target position
            // Vector2 targetPosition = new Vector2(unitTransform.position.x,
            //     unitTransform.position.y + inputY * MoveSpeed + forceY * MoveSpeed);// Apply both input and force
            // Vector2 targetPosition = new Vector2(unitTransform.position.x,
            //     forceY * MVC_Value);// Apply both input and force    
            float targetY = Mathf.Clamp(forceY * maxHeight, -maxHeight, maxHeight);


            Vector2 targetPosition = new Vector2(unitTransform.position.x,
                targetY); // Apply both input and force    


            // Calculate the new target position
            // Vector2 targetPosition = new Vector2(unitTransform.position.x, targetY);

            Vector2 currentPosition = rb.position;

            // Lerp the position for smooth movement
            // Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime);

            // Apply the new position
            // rb.MovePosition(newPosition);
            rb.MovePosition(targetPosition);
        }


        // Method to apply forces received from UDPClient
        public void ApplyForces(double[] forces) {
            if (forces.Length != 10) {
                Debug.LogError("Invalid forces array length. Expected 10 forces.");
                return;
            }

            // Map forces to the units. The first 5 are for left units.
            if (isLeftHand) {
                for (int i = 0; i < 5; i++) {
                    unitForces[i] = (float)forces[i];
                }
            }
            else {
                for (int i = 0, j = 5; i < 5 && j < forces.Length; i++, j++) {
                    unitForces[i] = (float)forces[j];
                }
            }

            // Validate the forces to ensure none are NaN
            for (int i = 0; i < unitForces.Length; i++) {
                if (float.IsNaN(unitForces[i]) || float.IsInfinity(unitForces[i])) {
                    Debug.LogError($"Invalid force value at index {i}: {unitForces[i]}");
                    unitForces[i] = 0; // Reset to zero or another default value
                }
            }
        }
    }
}