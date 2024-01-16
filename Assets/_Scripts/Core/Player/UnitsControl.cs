using System;
using UnityEngine;

public class UnitsControl : MonoBehaviour {
    [Header("References")] 
    [SerializeField] private InputReader inputReader;
    
    [SerializeField] private Transform unit1Transform;
    private Vector2 previous1MovementInput;
    [SerializeField] private Transform unit2Transform;
    private Vector2 previous2MovementInput;
    [SerializeField] private Transform unit3Transform;
    private Vector2 previous3MovementInput;
    [SerializeField] private Transform unit4Transform;
    private Vector2 previous4MovementInput;
    [SerializeField] private Transform unit5Transform;
    private Vector2 previous5MovementInput;
    
    [SerializeField] private Rigidbody2D rb1;
    [SerializeField] private Rigidbody2D rb2;
    [SerializeField] private Rigidbody2D rb3;
    [SerializeField] private Rigidbody2D rb4;
    [SerializeField] private Rigidbody2D rb5;
    
    private bool isLeftHand = false;
    
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 4f;

    private void Start() {
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

    void FixedUpdate()
    {
        MoveUnit(rb1, unit1Transform, previous1MovementInput.y);
        MoveUnit(rb2, unit2Transform, previous2MovementInput.y);
        MoveUnit(rb3, unit3Transform, previous3MovementInput.y);
        MoveUnit(rb4, unit4Transform, previous4MovementInput.y);
        MoveUnit(rb5, unit5Transform, previous5MovementInput.y);
    }

    private void MoveUnit(Rigidbody2D rb, Transform unitTransform, float inputY)
    {
        // Adjust the position based on the input and moveSpeed
        Vector2 targetPosition = new Vector2(unitTransform.position.x,unitTransform.up.y * (inputY * moveSpeed));
        // Debug.Log("UnitTransform.up: " + targetPosition);
        Vector2 currentPosition = rb.position;

        // Lerp the position to create smooth movement
        Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime * moveSpeed);

        // Set the new position
        rb.MovePosition(newPosition);
    }
}
