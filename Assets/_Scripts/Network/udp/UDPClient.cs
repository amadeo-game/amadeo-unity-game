using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using BridgePackage;
//todo: 1.Flexion or extension for mvc forces  
//todo: 2. left or right hand

// Attached to the GameObject BridgeGenerator

public class UDPClient : MonoBehaviour
{
    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;
    private CancellationTokenSource _cancellationTokenSource;
    private bool isReceiving = true;
    [SerializeField] private BridgeAPI bridgeApi;
    private double[] _zeroForces = new double[10]; // Store zeroing forces
    private double[] mvcForceExtension = new double[5]; // Store MVC forces
    private double[] mvcForceFlexion = new double[5]; // Store MVC forces
    bool isLeftHand = false;
    bool isFlexion = false;
    private void Start()
    {
        // Initialize the UdpClient
        try
        {
            _udpClient = new UdpClient(8888); // Listen for data on port 8888
            _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Placeholder for any remote endpoint
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize UdpClient: {ex.Message}");
            return; // Exit if UdpClient initialization fails
        }

        _cancellationTokenSource = new CancellationTokenSource();

        SetZeroForces(); // Load zeroing forces from PlayerPrefs
        SetMvcForces(); // Load MVC forces from PlayerPrefs

    }

    private void SetZeroForces()
    {
        var data = PlayerPrefs.GetString("zeroForces", ""); // Default to empty string if not set
        // Debug.Log("Retrieved zeroForces from PlayerPrefs: " + data);
        if (string.IsNullOrEmpty(data))
        {
            Debug.LogError("ZeroForces data is empty or not set in PlayerPrefs");
            return;
        }

        string[] forces = data.Split('\t');
        if (forces.Length != 10)
        {
            Debug.LogError("ZeroForces data does not contain exactly 10 values");
            // Debug.Log(string.Join(", ", forces));
            return;
        }


        for (int i = 0; i < _zeroForces.Length; i++)
        {
            if (double.TryParse(forces[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var force))
            {
                _zeroForces[i] = force;
            }
            else
            {
                Debug.LogError($"Error parsing zero force at position {i + 1}: {forces[i + 1]}");
                _zeroForces[i] = 0; // or any other default/fallback value
            }
        }

        // Debug.Log("Parsed zeroing forces: " + string.Join(", ", zeroForces));
    }

    private void SetMvcForces()
    {
        for (var i = 1; i <= mvcForceExtension.Length; i++)
        {   
            var key = "E"+i;
            double forceFinger = PlayerPrefs.GetFloat(key);
            mvcForceExtension[i-1] = forceFinger;
        }

        for (var i = 1; i <= mvcForceFlexion.Length; i++)
        {   
            var key = "F"+i;
            double forceFinger = PlayerPrefs.GetFloat(key);
            mvcForceFlexion[i-1] = forceFinger;
        }
        
        Debug.Log(string.Join(", ", mvcForceExtension));
        Debug.Log(string.Join(", ", mvcForceFlexion));
    }
    
    public void StartReceiveData() {
        ReceiveData(_cancellationTokenSource.Token);
        Debug.Log("UDPClient: Start listening for ZeroF data from server");
    }
    
    public void StopReceiveData() {
        isReceiving = false;
        Debug.Log("UDPClient: Stop listening for ZeroF data from server");
    }
    private async void ReceiveData(CancellationToken cancellationToken)
    {
        while (isReceiving && !cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Receive data from the server
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                byte[] data = result.Buffer;

                // Convert the received data to a string
                string receivedData = Encoding.ASCII.GetString(data);

                // Pass the received data to the Move script
                HandleReceivedData(receivedData);
            }
            catch (SocketException ex)
            {
                Debug.LogError($"SocketException: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                // This exception is expected when _udpClient is closed during ReceiveAsync
                Debug.Log("UDPClient has been disposed.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception: {ex.Message}");
            }
        }
    }

    private void HandleReceivedData(string data)
    {
        string[] forces = data.Split('\t');
        double[] forcesNum = new double[10]; // Array to store parsed forces. We expect 10 force values.
        
        if (forces.Length != 11) return; // Ensuring we have exactly 11 values (1 time + 10 forces)
        
        for (var i = 0; i < forcesNum.Length; i++)
        {
            if (double.TryParse(forces[i + 1].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture,
                    out var force))
            {
                forcesNum[i] = force;
            }
            else
            {
                Debug.LogError($"Error parsing force at position {i + 1}: {forces[i + 1]}");
                forcesNum[i] = 0; // or any other default/fallback value
            }
        }

        // Apply zeroing offset
        for (var i = 0; i < forcesNum.Length; i++)
        {
            //The goal of zeroing is to remove the baseline effect from the measurements
            forcesNum[i] -= _zeroForces[i];
        }
        // Send the parsed forces to the bridgeApi script
        if (bridgeApi != null)
        {
            bridgeApi.ApplyForces(forcesNum);
        }
        else
        {
            Debug.LogError("UnitsControl reference is missing in UDPClient.");
        }
    }


    
    
    //todo maybe need to change 
    private double[] ApplyMvcForces(double[] forcesNum) 
    {
        // Normalize forces using MVC values
        var normalizedForces = new double[10];
    
        for (var i = 0; i < 5; i++)
        {
            if (isFlexion)
            {
                // Left hand mvc forces
                normalizedForces[i] = mvcForceFlexion[i] != 0 ? forcesNum[i] / mvcForceFlexion[i] : 0;
                // Right hand mvc forces
                normalizedForces[i + 5] = mvcForceFlexion[i] != 0 ? forcesNum[i + 5] / mvcForceFlexion[i] : 0;
            }
            else
            {
                // Left hand mvc forces
                normalizedForces[i] = mvcForceExtension[i] != 0 ? forcesNum[i] / mvcForceExtension[i] : 0;
                // Right hand mvc forces
                normalizedForces[i + 5] = mvcForceExtension[i] != 0 ? forcesNum[i + 5] / mvcForceExtension[i] : 0;
            }
        }
    
        Debug.Log("Normalized forces: " + string.Join(", ", normalizedForces));
        return normalizedForces;
    }

    private void OnApplicationQuit() {
        StopClientConnection();
    }

    private void StopClientConnection() {
        isReceiving = false; // Signal the receiving loop to stop

        _cancellationTokenSource.Cancel(); // Cancel the receive task

        // Properly dispose of the UdpClient 
        if (_udpClient != null) {
            _udpClient.Close();
            _udpClient = null;
        }

        //cancellation token source
        _cancellationTokenSource.Dispose();
    }
}