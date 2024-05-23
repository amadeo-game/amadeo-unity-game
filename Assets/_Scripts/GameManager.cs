using BridgePackage;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour {
    public static GameManager instance;
    
    [Header("Events")]
    [SerializeField] private UnityEvent _startGame;
    [SerializeField] private UnityEvent<int[], bool> _startGameWithHeights;
    [SerializeField] private UnityEvent _endGame;
    
    


    private void Awake() {
        if (instance == null) {
            instance = this;
        }
    }

    private void OnEnable() {
        BridgeAPI.BridgeReady += StartGameInvoke;
        
    }
    
    private void EnablePlayButton() {
        // Enable the play button
        
    }

    public void StartGameInvoke() {
        _startGame.Invoke();
    }
    
    public void StartGameInvokeWithHeights() {
        _startGameWithHeights.Invoke(new int[] { 1, 2, 3, 4, 5 }, true);
    }

    public void EndGameInvoke() {
        _endGame.Invoke();
    }
}