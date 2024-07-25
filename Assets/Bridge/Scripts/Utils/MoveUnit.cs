using System;
using UnityEngine;

namespace BridgePackage {
    /// <summary>
    /// Controls the individual player unit.
    /// </summary>
    public class MoveUnit : MonoBehaviour {
        private bool _controlEnabled = false;

        private Rigidbody2D _rb; // Cached reference to the Rigidbody2D component

        private bool _heightChanged = false;

        // setter for _height
        private int _fingerIndex;
        private FingerUnit _fingerUnit;

        internal float MvcE = 1;
        internal float MvcF = 1;

        private float _height;

        private float _bestHeight = 0f;

        private bool _isFlexion;

        delegate void SetBestHeight(float height);

        SetBestHeight _setBestHeight;

        public float BestHeight => _bestHeight;


        private void Awake() {
            // Cache the Rigidbody2D component at the start
            _rb = GetComponent<Rigidbody2D>();
            if (_rb == null) {
                Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
            }

            _isFlexion = BridgeDataManager.IsFlexion;
            
            MvcE = BridgeDataManager.MvcValuesExtension[_fingerIndex];
            MvcF = BridgeDataManager.MvcValuesFlexion[_fingerIndex];
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
            var height = forces[_fingerIndex];
            Debug.Log("MvcF :: " + MvcF + " MvcE :: " + MvcE + " fingerIndex :: " + _fingerIndex + " Height :: " + height);
            if (_height < height) {
                _height = height * MvcF;
            }
            else if (_height > height) {
                _height = height * MvcE;
            }
            else {
                _heightChanged = false;
                return;
            }

            _heightChanged = true;
            _setBestHeight(_height);
        }

        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        public void ApplyForce() {
            if (_rb != null) {
                // Debug.Log("MoveUnit :: Height: " + _height);
                Vector2 targetPosition = new Vector2(transform.position.x, _height);
                _rb.MovePosition(targetPosition);

                // Vector2 finalPos = Vector2.Lerp(transform.position, targetPosition, Time.deltaTime);
                // Debug.Log($"Attempting to move {gameObject.name} to {targetPosition}");
                // rb.MovePosition(finalPos);
                // Debug.Log($"Position after MovePosition: {transform.position}");
            }
        }

        private void FixedUpdate() {
            if (_controlEnabled) {
                if (_heightChanged) {
                    ApplyForce();
                }
            }
        }

        internal void SetControl(bool controlEnabled, bool resetPos = true, float goToHeight = 0f) {
            _controlEnabled = controlEnabled;
            if (!controlEnabled) {
                _height = goToHeight;
                if (resetPos) {
                    ApplyForce();
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_controlEnabled) {
                // Debug.Log(" OnTriggerEnter2D :: MoveUnit :: " + _fingerUnit);
                BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, true);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            if (_controlEnabled) {
                // Debug.Log(" OnTriggerExit2D :: MoveUnit :: " + _fingerUnit);
                BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, false);
            }
        }
    }
}