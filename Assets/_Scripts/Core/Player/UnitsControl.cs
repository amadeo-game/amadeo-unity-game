using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitsControl : MonoBehaviour {
    [SerializeField] private InputReader inputReader;
    private bool isUnitsSet = false;
    private Transform unit1Transform;
    private Transform unit2Transform;
    private Transform unit3Transform;
    private Transform unit4Transform;
    private Transform unit5Transform;

    private Rigidbody2D rb1;
    private Rigidbody2D rb2;
    private Rigidbody2D rb3;
    private Rigidbody2D rb4;
    private Rigidbody2D rb5;
    
    private Vector2 previous1MovementInput;
    private Vector2 previous2MovementInput;
    private Vector2 previous3MovementInput;
    private Vector2 previous4MovementInput;
    private Vector2 previous5MovementInput;
    


    public void SetPlayerUnits(GameObject[] gameUnits) {
        if (gameUnits.Length != 5) {
            throw new ArgumentException("gameUnits must be length 5.");            
        }
        // gameUnits is length 5
        unit1Transform = gameUnits[0].transform;
        unit2Transform = gameUnits[1].transform;
        unit3Transform = gameUnits[2].transform;
        unit4Transform = gameUnits[3].transform;
        unit5Transform = gameUnits[4].transform;
        
        rb1 = gameUnits[0].GetComponent<Rigidbody2D>();
        rb2 = gameUnits[1].GetComponent<Rigidbody2D>();
        rb3 = gameUnits[2].GetComponent<Rigidbody2D>();
        rb4 = gameUnits[3].GetComponent<Rigidbody2D>();
        rb5 = gameUnits[4].GetComponent<Rigidbody2D>();
        
        isUnitsSet = true;
        
    }
    

    private void OnDisable() {
        if (isLeftHand) {
            inputReader.OnLF1Event -= HandleMoveUnit1;
            inputReader.OnLF2Event -= HandleMoveUnit2;
            inputReader.OnLF3Event -= HandleMoveUnit3;
            inputReader.OnLF4Event -= HandleMoveUnit4;
            inputReader.OnLF5Event -= HandleMoveUnit5;
        }
        else {
            inputReader.OnRF1Event -= HandleMoveUnit1;
            inputReader.OnRF2Event -= HandleMoveUnit2;
            inputReader.OnRF3Event -= HandleMoveUnit3;
            inputReader.OnRF4Event -= HandleMoveUnit4;
            inputReader.OnRF5Event -= HandleMoveUnit5;
        }
    }

    private void OnEnable() {
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

    public void DisableControl() {
        Debug.Log("Stop moving units.");
        isUnitsSet = false;
    }

    private bool isLeftHand = false;
    
    [FormerlySerializedAs("moveSpeed")]
    [Header("Settings")]
    [SerializeField] private float maxHeight = 4f;
    

    // private void Awake() {
    //     enabled = false;
    // }
    
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

    void FixedUpdate() {
        if (!isUnitsSet) return;
        MoveUnit(rb1, unit1Transform, previous1MovementInput.y);
        MoveUnit(rb2, unit2Transform, previous2MovementInput.y);
        MoveUnit(rb3, unit3Transform, previous3MovementInput.y);
        MoveUnit(rb4, unit4Transform, previous4MovementInput.y);
        MoveUnit(rb5, unit5Transform, previous5MovementInput.y);
    }

    private void MoveUnit(Rigidbody2D rb, Transform unitTransform, float inputY)
    {
        // Adjust the position based on the input and moveSpeed
        Vector2 targetPosition = new Vector2(unitTransform.position.x, (inputY*MoveSpeed) + inputY*unitTransform.position.y);
        // Debug.Log("UnitTransform.up: " + targetPosition);
        Vector2 currentPosition = rb.position;

        // Lerp the position to create smooth movement
        Vector2 newPosition = Vector2.Lerp(currentPosition, targetPosition, Time.fixedDeltaTime);

        // Set the new position
        rb.MovePosition(newPosition);
    }

    float MoveSpeed { get; set; } = 0.5f;
}
