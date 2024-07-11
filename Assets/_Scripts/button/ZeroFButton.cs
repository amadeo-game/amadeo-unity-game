using UnityEngine;

public class ZeroFButton : MonoBehaviour {
    /*
     * This script is attached to the ZeroF button in the UI.
     * It you to start the server for making the zeroF operation.
     */

    [SerializeField] private int defaultPortNumber = 4444;

    private int portNumber;
    [SerializeField] private ZeroFClient _zeroFClient;
    // [SerializeField] private UDPReceiver _udpReceiver;

    private void Awake() {
        portNumber = PlayerPrefs.GetInt("portNumber", defaultPortNumber);
    }

    public void StartZeroF() {
        
        
        
        // bool isServerListening = ServerAPI.Instance.StartZeroF();
        //
        // if (!isServerListening) {
        //     Debug.LogError("ZeroF server Connection failed");
        //     return;
        // }
        //
        // Debug.Log("ZeroF server Connection established");
        // _zeroFClient.StartReceiveData();
        // ServerAPI.Instance.StartZeroF();
    }


    private void OnDisable() {
        Debug.Log("'disable'");

        StopServer();
        _zeroFClient.StopConnection();
    }


    private void StopServer() {
        // Debug.Log("disable");
        // _udpServer.StopZeroForces();
        ServerAPI.Instance.StopListeningForGame();
    }
}