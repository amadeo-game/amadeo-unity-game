using System;
using UnityEngine;

public class DebugColliderVisualizer : MonoBehaviour {
    private BoxCollider2D _boxCollider2D;

    private void Start() {
        _boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.F1)) // Change the key as needed
        {
            Debug.Log(" Toggling Debug Mode");
            DebugSettings.ToggleDebugMode(!DebugSettings.DebugModeEnabled);
        }
    }
    
    private void OnDrawGizmos() {
        // This will visualize in the Editor when Gizmos are active
        if (DebugSettings.DebugModeEnabled) {
            DrawColliderGizmos();
        }
    }

    private void OnDrawGizmosSelected() {
        // Optional: Visualize only when selected in the Editor
        if (DebugSettings.DebugModeEnabled) {
            DrawColliderGizmos();
        }
    }

    private void OnRenderObject() {
        // This will visualize in both the Editor and the Build
        if (DebugSettings.DebugModeEnabled) {
            DrawColliderGizmos();
        }
    }

    private void DrawColliderGizmos() {
        // For 2D colliders
        if (_boxCollider2D != null) {
            Gizmos.color = Color.green;
            var bounds = _boxCollider2D.bounds;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }
}