using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BridgePackage {
    /// <summary>
    /// Controls the individual player unit.
    /// </summary>
    public class MoveUnit : MonoBehaviour {
        private bool _controlEnabled = false;

        private Rigidbody2D _rb; // Cached reference to the Rigidbody2D component
        public BoxCollider2D Collider; // Cached reference to the BoxCollider2D component

        private bool _heightChanged = false;

        private readonly Queue<float> _heightQueue = new Queue<float>();
        private float _sum = 0f;
        [SerializeField] private int _queueSize = 20;

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
            Collider = GetComponent<BoxCollider2D>();
            if (_rb == null) {
                Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
            }

            if (Collider == null) {
                Debug.LogError("BoxCollider2D component not found on " + gameObject.name);
            }

            _isFlexion = BridgeDataManager.IsFlexion;

            MvcE = BridgeDataManager.MvcValuesExtension[_fingerIndex];
            MvcF = BridgeDataManager.MvcValuesFlexion[_fingerIndex];
            _setBestHeight = _isFlexion
                ? height => _bestHeight = Mathf.Min(_height, _bestHeight)
                : height => _bestHeight = Mathf.Max(_height, _bestHeight);

            // Initialize the queue with zeros
            for (int i = 0; i < _queueSize; i++) {
                _heightQueue.Enqueue(0f);
            }
        }

        internal void SetFingerUnit(FingerUnit unit, int index) {
            _fingerUnit = unit;
            _fingerIndex = index;
        }

        private void OnEnable() {
            // BridgeEvents.ForcesUpdated += OnForcesUpdated;
        }

        private void OnDisable() {
            // BridgeEvents.ForcesUpdated -= OnForcesUpdated;
        }

        // private void OnForcesUpdated(float[] forces) {
        //     var height = forces[_fingerIndex];
        //     // Debug.Log(" Forces updated for " + _fingerUnit + " with height " + height);
        //
        //     // Add new value to the queue and update the running sum
        //     _sum -= _heightQueue.Dequeue();
        //     _heightQueue.Enqueue(height);
        //     _sum += height;
        //
        //     // Calculate the average height from the queue
        //     float averageHeight = _sum / _queueSize;
        //
        //     // Adjust _height based on MvcF and MvcE
        //     if (_height < averageHeight) {
        //         _height = averageHeight * MvcF;
        //     }
        //     else if (_height > averageHeight) {
        //         _height = averageHeight * MvcE;
        //     }
        //     else {
        //         _heightChanged = false;
        //         return;
        //     }
        //
        //     _heightChanged = true;
        //     _setBestHeight(_height);
        //     Vector2 targetPosition = new Vector2(transform.position.x, _height);
        //     _rb.MovePosition(targetPosition);
        // }
        
        internal void OnForcesUpdated(float force) {
            var height = force;
            // Debug.Log(" Forces updated for " + _fingerUnit + " with height " + height);

            // Add new value to the queue and update the running sum
            _sum -= _heightQueue.Dequeue();
            _heightQueue.Enqueue(height);
            _sum += height;

            // Calculate the average height from the queue
            float averageHeight = _sum / _queueSize;

            // Adjust _height based on MvcF and MvcE
            if (_height < averageHeight) {
                _height = averageHeight * MvcF;
            }
            else if (_height > averageHeight) {
                _height = averageHeight * MvcE;
            }
            else {
                _heightChanged = false;
                return;
            }

            _heightChanged = true;
            _setBestHeight(_height);
            Vector2 targetPosition = new Vector2(transform.position.x, _height);
            _rb.MovePosition(targetPosition);
        }

        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        internal void ApplyForce() {
            Vector2 targetPosition = new Vector2(transform.position.x, _height);
            _rb.MovePosition(targetPosition);
        }

        // private void FixedUpdate() {
        //     if (_controlEnabled) {
        //         if (_heightChanged) {
        //             ApplyForce();
        //         }
        //     }
        // }

        internal void SetActiveUnit(bool isActive, float goToPos) {
            _controlEnabled = isActive;
            if (!isActive) {
                
                
                    OnForcesUpdated(goToPos);
                
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
                BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, true);
            }
        }

        private void OnTriggerExit2D(Collider2D other) {
            BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, false);
        }
    }
}