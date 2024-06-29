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
   
    private int portNUmber;
    [SerializeField]
    private ZeroFClient _zeroFClient;
    private void Awake()
    {
        portNUmber = PlayerPrefs.GetInt("portNumber");
        _udpServer = new UDPServer(portNUmber);
        StartServer();
    }

    private void StartServer() {
        Debug.Log("start server");
        _udpServer.OpenConnection(); // Start the UDP server
    }
    public void ZeroForces() {
        _udpServer.ZeroForces(); // Trigger zeroing
    }
    
    
    
    private void OnDisable()
    {   
        Debug.Log("'disable'");
        
        StopServer();
        _zeroFClient.stopConnection();
    }
    
    

    private void StopServer() {
        Debug.Log("'disable'");
        _udpServer.StopZeroForces();
        _udpServer.StopServer(); // Stop the UDP server
    }
}
