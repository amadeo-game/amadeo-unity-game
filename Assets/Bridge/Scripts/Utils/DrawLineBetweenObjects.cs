using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BridgePackage {
    public class DrawLineBetweenObjects : MonoBehaviour {
        public Transform Target; // The second GameObject
        private LineRenderer _lineRenderer;

        void Start() {
            _lineRenderer = GetComponent<LineRenderer>();

            if (_lineRenderer == null) {
                Debug.LogError("LineRenderer component missing from this game object. Please add one.");
                return;
            }

            // Optional: Set up line appearance
            _lineRenderer.startWidth = 0.1f;
            _lineRenderer.endWidth = 0.1f;
            _lineRenderer.positionCount = 2; // Line needs 2 points to draw a line
        }

        void Update() {
            if (transform != null && Target != null && _lineRenderer != null) {
                // Update the line positions to match the GameObjects' positions
                _lineRenderer.SetPosition(0, transform.position);
                _lineRenderer.SetPosition(1, Target.position);
            }
        }
    }
}