using System;
using UnityEngine;

public class ServerAPI : MonoBehaviour {
    // this script is responsible for all the entry point to the script UDP_Server

    [SerializeField] InputType inputType = InputType.EmulationMode;
    private UDPServer udpServer; // Reference to the UDP server
    private int portNumber;
    bool isServerConnected = false;


    public static ServerAPI Instance;


    private void Awake() {

        
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(Instance.gameObject);
            // create new game object UDPReceiver
        
            //GameObject udpReceiver = new GameObject("UDPReceiver");
            //udpReceiver.AddComponent<UDPReceiver>();
            InitServer();
        }
        else {
            Debug.Log("ServerAPI already exists. Deleting duplicate.");
            Destroy(gameObject);
        }
    }

    private void InitServer() {

        
         Debug.Log("Initializing Server");
         portNumber = PlayerPrefs.GetInt("portNumber", 4444);
         Debug.Log("ServerAPI: Port Number: " + portNumber);
         udpServer = new UDPServer(portNumber, inputType);
         isServerConnected = udpServer.OpenConnection(); // Start the UDP server
    }

    private void OnEnable() {
        if (!isServerConnected) {
            InitServer();
        }
    }

    public void SetPortNumber(int portNumber) {
        this.portNumber = portNumber;
        // udpServer.CheckUpdatedPort(this.portNumber);
    }

    public bool StartZeroF() {
        Debug.Log("zero");
        if (!isServerConnected) {
            Debug.LogError("ZeroF server Connection failed");
            return false;
        }

        Debug.Log("ZeroF server Connection established");
        udpServer.ZeroForces(); // Trigger zeroing
        return true;
    }

    // Handle all operations of UDP_server

    public bool StartListeningForGame() {
        if (!isServerConnected) {
            Debug.LogError("Server Connection failed");
            return false;
        }

        udpServer.StartListeningForGame();
        return true;
    }

    public void StopListeningForGame() {
        // udpServer.StopListeningForGame();
    }

    private void StopServer() {
        if (!isServerConnected) return;
        udpServer.StopServer(); // Stop the UDP server
        isServerConnected = false;
    }

    private void OnDisable() {
        StopServer();
    }

    private void OnDestroy() {
        StopServer();
    }

    private void OnApplicationQuit() {
        StopServer();
    }
}