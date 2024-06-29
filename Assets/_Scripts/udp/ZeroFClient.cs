using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class ZeroFClient : MonoBehaviour
{
    private UdpClient _zeroFClient;
    private IPEndPoint _remoteEndPoint;
    private CancellationTokenSource _cancellationTokenSource;
    private bool isReceiving = true;

    private double[] zeroForces = new double[10]; // Store zeroing forces

 
    private void Start()
    {
        // Initialize the UdpClient
        try
        {
            _zeroFClient = new UdpClient(8888); // Listen for data on port 8888
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
                UdpReceiveResult result = await _zeroFClient.ReceiveAsync();
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

    private static void HandleReceivedData(string data)
    {
        PlayerPrefs.SetString("zeroForces", data);
       
    }

    //stop the connection 
    public void stopConnection()
    {
        isReceiving = false; // Signal the receiving loop to stop
        Debug.Log("stop connection from zeroFClient");
        // Cancel the receive task if the CancellationTokenSource is not null and not disposed
        if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Cancel(); // Cancel the receive task
        }
        

        // Properly dispose of the UdpClient 
        if (_zeroFClient != null)
        {
            _zeroFClient.Close();
            _zeroFClient = null;
        }

        // Dispose the CancellationTokenSource if it has not been disposed yet
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void OnApplicationQuit()
    {
        stopConnection();
    }
}