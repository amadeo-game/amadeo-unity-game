using System;
using BridgePackage;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static event Action GameWon;
    public static event Action GameLost;
    
    private LevelManager levelManager;
    private void Awake() {
        levelManager = GetComponent<LevelManager>();
    }

    private void Start()
    {
        // Listen for game state events
        BridgeEvents.OnGameStart += HandleGameStart;
        BridgeEvents.BridgeCollapsed += HandleGameFailed;
        BridgeEvents.BridgeIsComplete += HandleGameSuccess;
    }

    private void OnDestroy()
    {
        // Unsubscribe from game state events
        BridgeEvents.OnGameStart -= HandleGameStart;
        BridgeEvents.BridgeCollapsed -= HandleGameFailed;
        BridgeEvents.BridgeIsComplete -= HandleGameSuccess;
    }

    public void InitializeNewGame()
    {
        Debug.Log("GameManager :: InitializeNewGame() called.");
        levelManager.InitializeSession();
        // StartGame();
    }

    // public void StartGame() {
    //     if (gameInitialized) {
    //         levelManager.StartSession();
    //     }
    //     Debug.Log("GameManager :: StartGame() called.");
    //     
    // }
    

    private void HandleGameStart()
    {
        Debug.Log("Game started.");
        // Additional logic when the game starts
    }

    private void HandleGameFailed()
    {
        Debug.Log("Game failed.");
        // Additional logic when the game fails
        AudioManager.PlayDefeatSound();
    }

    private void HandleGameSuccess()
    {
        Debug.Log("Game succeeded.");
        // Additional logic when the game succeeds
        AudioManager.PlayVictorySound();
    }
}