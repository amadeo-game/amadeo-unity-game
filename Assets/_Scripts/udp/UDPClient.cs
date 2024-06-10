using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using BridgePackage;
using PlasticGui.WorkspaceWindow.Merge; // Ensure this matches the namespace in the Move script

public class UDPClient : MonoBehaviour
{
    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;
    private CancellationTokenSource _cancellationTokenSource;
    private bool isReceiving = true;
    [SerializeField] private UnitsControl unitsControl;

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

        // Start receiving data asynchronously
        ReceiveData(_cancellationTokenSource.Token);
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

    private  void HandleReceivedData(string data)
    {
        string[] forces = data.Split('\t');
        double[] forcesNum = new double[10]; // Array to store parsed forces. We expect 10 force values.
    
        if (forces.Length == 11) // Ensuring we have exactly 11 values (1 time + 10 forces)
        {
            Debug.Log("HandleReceivedData ");
            Debug.Log(data);
            Debug.Log("time: " + forces[0]);
            Debug.Log("force left 1 : " + forces[1]);
            Debug.Log("force left 2 : " + forces[2]);
            Debug.Log("force left 3 : " + forces[3]);
            Debug.Log("force left 4 : " + forces[4]);
            Debug.Log("force left 5 : " + forces[5]);
            Debug.Log("force right 6 : " + forces[6]);
            Debug.Log("force right 7 : " + forces[7]);
            Debug.Log("force right 8 : " + forces[8]);
            Debug.Log("force right 9 : " + forces[9]);
            Debug.Log("force right 10 : " + forces[10]);
        
            for (int i = 0; i < forcesNum.Length; i++)
            {
                if (double.TryParse(forces[i + 1], out double force)) // Parse force values (skipping the time)
                {
                    forcesNum[i] = force;
                }
                else
                {
                    Debug.LogError($"Error parsing force at position {i + 1}: {forces[i + 1]}");
                    forcesNum[i] = 0; // or any other default/fallback value
                }
            }
            // Send the parsed forces to the UnitsControl script
            if (unitsControl != null)
            {
                unitsControl.ApplyForces(forcesNum);
            }
            else
            {
                Debug.LogError("UnitsControl reference is missing in UDPClient.");
            }
            
        }
       
    }


    private void OnApplicationQuit()
    {
        isReceiving = false; // Signal the receiving loop to stop
       
        _cancellationTokenSource.Cancel(); // Cancel the receive task

        // Properly dispose of the UdpClient 
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
        }
        //cancellation token source
        _cancellationTokenSource.Dispose();
    }
}