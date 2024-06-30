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
    bool isServerConnected = false;
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
        isServerConnected = _udpServer.OpenConnection(); // Start the UDP server on a separate thread

    }
    
    // MVC = 3
    // Amadeo value = 2
    // real location on unity units = (2/3) * MaxHeight == (MVC/Value) * MaxHeight
    
    
    
    
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
        // _udpServer.setIsPlay(true);
        
        if (isServerConnected)
        {
            _udpServer.StartListeningForGame();
            Debug.Log("Start listening for game data from client");
            _udpClient.StartReceiveData(); // Start receiving data from the client
        }
        else {
            Debug.LogError("Failed to connect to the server.");
        }
    }


    public void BuildBridgeWithHeights(int bridgeTypeIndex)
    {
        startEndButtons?.DisableButtons();
        _buildBridgeWithHeights.Invoke(playerUnitsHeights, bridgeCollectionSO, bridgeTypeIndex);
    }

    public void EndGameInvoke()
    {
        _endGame.Invoke();
        // _udpServer.setIsPlay(false);
        _udpClient.StopReceiveData(); // Stop receiving data from the client
        _udpServer.StopListeningForGame(); // Stop the UDP server and clean up resources
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