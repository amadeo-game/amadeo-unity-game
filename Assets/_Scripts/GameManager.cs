using BridgePackage;
using UnityEngine;
using UnityEngine.Events;


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeReference] private bool autoStart = false;
    [Header("Events")] [SerializeField] private UnityEvent _buildGameBridge;
    [SerializeField] private UnityEvent<int[], BridgeCollectionSO, int> _buildBridgeWithHeights;
    [SerializeField] private UnityEvent _startGame;
    [SerializeField] private UnityEvent _endGame;
    [SerializeField] private UnityEvent _winGame;

    [SerializeField] BridgeCollectionSO bridgeCollectionSO;

    [SerializeField] private StartEndButtons startEndButtons; // temp for demoUI handling.

    [SerializeField, Range(0, 5)] // TODO: support flexion mode (negative values)
    private int[] playerUnitsHeights = { 0, 0, 0, 0, 0 }; // Set this in the Inspector

    private UDPServer _udpServer;
    [SerializeField] private UDPClient _udpClient;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        PlayerPrefs.GetInt("portNumber");
        Debug.Log("Port Number: " + PlayerPrefs.GetInt("portNumber"));
        _udpServer = new UDPServer(PlayerPrefs.GetInt("portNumber"));
    }
    
    
    
    
    public void OnValueChanged0(float value)
    {
        playerUnitsHeights[0] = (int)value;
    }

    public void OnValueChanged1(float value)
    {
        playerUnitsHeights[1] = (int)value;
    }

    public void OnValueChanged2(float value)
    {
        playerUnitsHeights[2] = (int)value;
    }

    public void OnValueChanged3(float value)
    {
        playerUnitsHeights[3] = (int)value;
    }

    public void OnValueChanged4(float value)
    {
        playerUnitsHeights[4] = (int)value;
    }

    private void OnEnable()
    {
        BridgeAPI.BridgeReady += OnBridgeReady;
    }

    private void OnBridgeReady()
    {
        Debug.Log("NotifyBridgeReady");
        if (autoStart == false)
        {
            return;
        }

        // Enable the play button
        _startGame.Invoke();
    }

    public void StartGame()
    {
        // Enable the play button
        _startGame.Invoke();
        _udpServer.setIsPlay(true);
        _udpServer.OpenConnection(); // Start the UDP server on a separate thread
    }


    public void BuildBridgeWithHeights(int bridgeTypeIndex)
    {
        startEndButtons?.DisableButtons();
        _buildBridgeWithHeights.Invoke(playerUnitsHeights, bridgeCollectionSO, bridgeTypeIndex);
    }

    public void EndGameInvoke()
    {
        _endGame.Invoke();
        _udpServer.setIsPlay(false);
        _udpServer.StopServer(); // Stop the UDP server and clean up resources
    }

    public void WinGame()
    {
        _winGame.Invoke();
    }

    private void OnApplicationQuit()
    {
        _udpServer.StopServer(); // Ensure server is stopped on application quit
    }
}