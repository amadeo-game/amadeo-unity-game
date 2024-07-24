using System;
using UnityEngine;

namespace BridgePackage {
    /// <summary>
    /// Controls the individual player unit.
    /// </summary>
    public class MoveUnit : MonoBehaviour {
        private bool _controlEnabled = false;

        private Rigidbody2D rb; // Cached reference to the Rigidbody2D component

        // setter for _height
        private int _fingerIndex;
        private FingerUnit _fingerUnit;

        private float _height;

        private float _bestHeight = 0f;

        private bool _isFlexion;

        delegate void SetBestHeight(float height);

        SetBestHeight _setBestHeight;

        public float BestHeight => _bestHeight;
        
        private void Awake() {
            // Cache the Rigidbody2D component at the start
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) {
                Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
            }

            _isFlexion = BridgeDataManager.IsFlexion;
            _setBestHeight = _isFlexion
                ? height => _bestHeight = Mathf.Min(_height, _bestHeight)
                : height => _bestHeight = Mathf.Max(_height, _bestHeight);
        }

        internal void SetFingerUnit(FingerUnit unit, int index) {
            _fingerUnit = unit;
            _fingerIndex = index;
        }

        private void OnEnable() {
            BridgeEvents.ForcesUpdated += OnForcesUpdated;
        }

        private void OnDisable() {
            BridgeEvents.ForcesUpdated -= OnForcesUpdated;
        }
        

        private void OnForcesUpdated(float[] forces) {
            Debug.Log("MoveUnit :: OnForcesUpdated :: Forces: " + forces[_fingerIndex] + "on FingerIndex: " + _fingerIndex);
            _height = forces[_fingerIndex];
            _setBestHeight(_height);
        }
        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        public void ApplyForce() {
            if (rb != null) {
                // Debug.Log("MoveUnit :: Height: " + Height);
                Vector2 targetPosition = new Vector2(transform.position.x, _height);
                // Debug.Log($"Attempting to move {gameObject.name} to {targetPosition}");
                rb.MovePosition(targetPosition);
                // Debug.Log($"Position after MovePosition: {transform.position}");
            }
        }

        private void FixedUpdate() {
            if (_controlEnabled) {
                Debug.Log("MoveUnit :: FixedUpdate :: Control Enabled");
                ApplyForce();
            }
        }
        
        internal void SetControl(bool controlEnabled, bool resetPos = false) {
            _controlEnabled = controlEnabled;
            if (!controlEnabled) {
                _height = 0;
                if (!resetPos) {
                    ApplyForce();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_controlEnabled) {
                BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, true);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (_controlEnabled) {
                BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, false);
            }
        }
    }
}