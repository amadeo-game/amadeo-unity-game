using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;
[CreateAssetMenu(fileName = "New Input Reader", menuName = "Input/InputReader")]
public class InputReader : ScriptableObject, ILeftHandActions, IRightHandActions {
    // Left Hand Fingers
    public event Action<Vector2> OnLF1Event;
    public event Action<Vector2> OnLF2Event;
    public event Action<Vector2> OnLF3Event;
    public event Action<Vector2> OnLF4Event;
    public event Action<Vector2> OnLF5Event;
    
    // Right Hand Fingers
    public event Action<Vector2> OnRF1Event;
    public event Action<Vector2> OnRF2Event;
    public event Action<Vector2> OnRF3Event;
    public event Action<Vector2> OnRF4Event;
    public event Action<Vector2> OnRF5Event;
    
    
    private Controls controls;
    private void OnEnable() {
        if (controls == null) {
            controls = new Controls();
            controls.LeftHand.SetCallbacks(this);
            controls.RightHand.SetCallbacks(this);

        }
        controls.LeftHand.Enable();
        controls.RightHand.Enable();

        
    }

    // Left Hand Input Actions
    public void OnLF1(InputAction.CallbackContext context) {
        OnLF1Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLF2(InputAction.CallbackContext context) {
        OnLF2Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLF3(InputAction.CallbackContext context) {
        OnLF3Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLF4(InputAction.CallbackContext context) {
        OnLF4Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLF5(InputAction.CallbackContext context) {
        OnLF5Event?.Invoke(context.ReadValue<Vector2>());
    }
    
    // Right Hand Input Actions

    public void OnRF1(InputAction.CallbackContext context) {
        OnRF1Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRF2(InputAction.CallbackContext context) {
        OnRF2Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRF3(InputAction.CallbackContext context) {
        OnRF3Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRF4(InputAction.CallbackContext context) {
        OnRF4Event?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnRF5(InputAction.CallbackContext context) {
        OnRF5Event?.Invoke(context.ReadValue<Vector2>());
    }
}