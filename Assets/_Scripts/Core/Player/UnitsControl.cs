using System;
using System.Collections;
using System.Collections.Generic;
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

    // Update is called once per frame
    void FixedUpdate()
    {
        rb1.velocity = unit1Transform.up * (previous1MovementInput.y * moveSpeed);
        rb2.velocity = unit2Transform.up * (previous2MovementInput.y * moveSpeed);
        rb3.velocity = unit3Transform.up * (previous3MovementInput.y * moveSpeed);
        rb4.velocity = unit4Transform.up * (previous4MovementInput.y * moveSpeed);
        rb5.velocity = unit5Transform.up * (previous5MovementInput.y * moveSpeed);
    }
}
