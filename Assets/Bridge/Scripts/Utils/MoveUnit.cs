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

        private bool _heightChanged = true;

        private readonly Queue<float> _heightQueue = new Queue<float>();
        private float _sum = 0f;
        [SerializeField] private int _queueSize = 5;

        // setter for _height
        private int _fingerIndex;
        private FingerUnit _fingerUnit;

        internal float MvcE = 1;
        internal float MvcF = 1;

        private float _height;



        private bool _isFlexion;
        private bool _isRbNotNull;


        private void Awake() {
     
            // Cache the Rigidbody2D component at the start
            _rb = GetComponent<Rigidbody2D>();

            Collider = GetComponent<BoxCollider2D>();
            if (_rb == null) {
                
                Debug.LogError("Rigidbody2D component not found on " + gameObject.name);
            }
            _isRbNotNull = true;
            if (Collider == null) {
                Debug.LogError("BoxCollider2D component not found on " + gameObject.name);
            }

            _isFlexion = BridgeDataManager.IsFlexion;

            MvcE = BridgeDataManager.MvcValuesExtension[_fingerIndex];
            MvcF = BridgeDataManager.MvcValuesFlexion[_fingerIndex];


            // Initialize the queue with zeros
            //for (int i = 0; i < _queueSize; i++) {
            //    _heightQueue.Enqueue(0f);
            //}
        }

        internal void SetFingerUnit(FingerUnit unit, int index) {
            _fingerUnit = unit;
            _fingerIndex = index;
        }
        
        internal void OnForcesUpdated(float force) {
            // Add new value to the queue and update the running sum

            _heightQueue.Enqueue(force);
            _sum += force;


            if (_heightQueue.Count > _queueSize) {
                _sum -= _heightQueue.Dequeue();
            }

            // Calculate the average height from the queue
            float averageHeight = _sum / _queueSize;

            _height = averageHeight;

            MoveUnitPosition();
            
        }

        /// <summary>
        /// Applies the specified force to the unit.
        /// </summary>
        /// <param name="force">Force to apply.</param>
        private void MoveUnitPosition() {
            if (_isRbNotNull) {
                Vector2 targetPosition = new Vector2(transform.position.x, _height);
                // Debug.Log("applying force :: " + _height);
                _rb.MovePosition(targetPosition);
                // Debug.Log($"Unit moved at: {Time.time} to position: {targetPosition}");
            }
        }

        internal void SetControl(bool controlEnabled) {
            _controlEnabled = controlEnabled;
        }

        internal void ResetPosition() {
            _height = 0;
            var transform1 = transform;
            Vector2 targetPosition = new Vector2(transform1.position.x, _height);
            transform1.position = targetPosition;
            // ApplyForce();
        }

        internal void ConnectToBridge() {
            _height = BridgeDataManager.Heights[_fingerIndex];
            MoveUnitPosition();
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (_controlEnabled) { }

            BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, true);
        }

        private void OnTriggerExit2D(Collider2D other) {
            BridgeEvents.UnitPlacementStatusChanged?.Invoke(_fingerUnit, false);
        }
    }
}