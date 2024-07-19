using System;
using BridgePackage;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private LevelManager levelManager;
    private bool gameInitialized = false;
    private void Awake() {
        levelManager = GetComponent<LevelManager>();
    }

    private void Start()
    {
        // Listen for game state events
        BridgeAPI.OnGameStart += HandleGameStart;
        BridgeAPI.BridgeCollapsed += HandleGameFailed;
        BridgeAPI.BridgeIsComplete += HandleGameSuccess;
    }

    private void OnDestroy()
    {
        // Unsubscribe from game state events
        BridgeAPI.OnGameStart -= HandleGameStart;
        BridgeAPI.BridgeCollapsed -= HandleGameFailed;
        BridgeAPI.BridgeIsComplete -= HandleGameSuccess;
    }

    public void InitializeNewGame()
    {
        Debug.Log("GameManager :: InitializeNewGame() called.");
        gameInitialized = levelManager.SetupNewLevel();
    }

    public void StartGame() {
        if (gameInitialized) {
            levelManager.StartSession();
        }
        Debug.Log("GameManager :: StartGame() called.");
        
    }
    

    private void HandleGameStart()
    {
        Debug.Log("Game started.");
        // Additional logic when the game starts
    }

    private void HandleGameFailed()
    {
        Debug.Log("Game failed.");
        // Additional logic when the game fails
    }

    private void HandleGameSuccess()
    {
        Debug.Log("Game succeeded.");
        // Additional logic when the game succeeds
    }
}