using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroFButton : MonoBehaviour
{
    /*
     * This script is attached to the ZeroF button in the UI.
     * It you to start the server for making the zeroF operation.
     */
    
    private UDPServer _udpServer; // Reference to the UDP server
   
    private int portNumber;
    bool isServerConnected = false;
    [SerializeField]
    private ZeroFClient _zeroFClient;
    private void Awake()
    {
        portNumber = PlayerPrefs.GetInt("portNumber", 4444);
    }

    private void OnEnable() {
        _udpServer = new UDPServer(portNumber);
        isServerConnected = _udpServer.OpenConnection(); // Start the UDP server

    }

    public void StartZeroF() {
        if (isServerConnected) {
            Debug.Log("ZeroF server Connection established");
            _zeroFClient.StartReceiveData();
            ZeroForces();
        }
        else {
            Debug.LogError("ZeroF server Connection failed");
        
        }
    }
    private void ZeroForces() {
        _udpServer.ZeroForces(); // Trigger zeroing
    }
    
    
    
    private void OnDisable()
    {   
        Debug.Log("'disable'");
        
        StopServer();
        _zeroFClient.stopConnection();
    }
    
    

    private void StopServer() {
        // Debug.Log("disable");
        // _udpServer.StopZeroForces();
        _udpServer.StopServer(); // Stop the UDP server
    }
}
