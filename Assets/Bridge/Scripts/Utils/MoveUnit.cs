using System;
using UnityEngine;

namespace BridgePackage
{
    /// <summary>
    /// Controls the individual player unit.
    /// </summary>
    public class MoveUnit : MonoBehaviour
    {
        private bool controlEnabled = true;
        private Rigidbody2D rb; // Cached reference to the Rigidbody2D component
        // setter for _height
        internal int fingerIndex;

        public float Height;
        private void Awake()
        {
            // Cache the Rigidbody2D component at the start
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
            }
        }

        private void OnEnable() {
            BridgeEvents.ForcesUpdated += OnForcesUpdated;
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
        }
        
        private void OnDisable() {
            BridgeEvents.ForcesUpdated -= OnForcesUpdated;
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (state == BridgeStates.InGame) {
                controlEnabled = true;
                
            } else {
                controlEnabled = false;
            }
        }

        private void OnForcesUpdated(float[] forces) {
            
            Height = (float)forces[fingerIndex];
        }

        private void Start() {
            fingerIndex = 0;
            
        }

        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        public void ApplyForce(float force)
        {
            Debug.Log("MoveUnit :: ApplyForce called");
            if (controlEnabled && rb != null)
            {
                Vector2 targetPosition = new Vector2(transform.position.x, force);
                Debug.Log($"Attempting to move {gameObject.name} to {targetPosition}");
                rb.MovePosition(targetPosition);
                Debug.Log($"Position after MovePosition: {transform.position}");
            }
            else
            {
                Debug.LogError($"Movement not allowed or Rigidbody2D is null for {gameObject.name}");
            }
        }

        private void FixedUpdate() {
            if (controlEnabled && rb != null)
            {
                Debug.Log("MoveUnit :: Height: " + Height);
                Vector2 targetPosition = new Vector2(transform.position.x, Height);
                Debug.Log($"Attempting to move {gameObject.name} to {targetPosition}");
                rb.MovePosition(targetPosition);
                // Debug.Log($"Position after MovePosition: {transform.position}");
            }
        }


        /// <summary>
        /// Disables the control for this unit.
        /// </summary>
        public void DisableControl()
        {
            controlEnabled = false;
        }
    }
}