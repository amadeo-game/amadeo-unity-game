using System;
using BridgePackage;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour {
    public static GameManager instance;
    [SerializeReference] private bool autoStart = false;
    [Header("Events")] [SerializeField] private UnityEvent _buildGameBridge;
    [SerializeField] private UnityEvent<int[], BridgeCollectionSO, int> _buildBridgeWithHeights;
    [SerializeField] private UnityEvent _startGame;
    [SerializeField] private UnityEvent _endGame;
    [SerializeField] private UnityEvent _winGame;

    [SerializeField] BridgeCollectionSO bridgeCollectionSO;

    [SerializeField] private StartEndButtons startEndButtons; // temp for demoUI handling.
    
    [SerializeField] private UDPClient _udpClient;
    
    // For Demo Purpose
    DemoBridgeHeights demoBridgeHeights;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        demoBridgeHeights = GetComponent<DemoBridgeHeights>();
    }




    private void OnEnable() {
        BridgeAPI.BridgeReady += OnBridgeReady;
    }

    private void OnBridgeReady() {
        Debug.Log("NotifyBridgeReady");
        if (autoStart == false) {
            return;
        }

        // Enable the play button
        _startGame.Invoke();
    }

    public void StartGame() {
        // Enable the play button
        _startGame.Invoke();

        // Open data connection 
        bool isServerListening = ServerAPI.Instance.StartListeningForGame();
        if (!isServerListening) {
            Debug.LogError("Failed to connect to the server.");
            return;
        }

        Debug.Log("Start listening for game data from client");
        _udpClient.StartReceiveData(); // Start receiving data from the client
    }


    public void BuildBridgeWithHeights(int bridgeTypeIndex) {
        startEndButtons?.DisableButtons();
        _buildBridgeWithHeights.Invoke(demoBridgeHeights.GetPlayerUnitsHeights(), bridgeCollectionSO, bridgeTypeIndex);
    }

    public void EndGameInvoke() {
        _endGame.Invoke();

        StopListeningToData();
    }

    private void StopListeningToData() {
        try {
            _udpClient.StopReceiveData(); // Stop receiving data from the client
        }
        catch (Exception ex) {
            Debug.LogError($"Error stopping UDP client: {ex.Message}");
        }

        try {
            ServerAPI.Instance.StopListeningForGame();
        }
        catch (Exception ex) {
            Debug.LogError($"Error stopping ServerAPI listening: {ex.Message}");
        }
    }

    public void WinGame() {
        _winGame.Invoke();

        StopListeningToData();
    }
    

}