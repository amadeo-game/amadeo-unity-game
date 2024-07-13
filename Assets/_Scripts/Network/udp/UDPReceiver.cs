using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool running;
    private IPAddress ipAddress = IPAddress.Parse("10.100.4.30");
    private int port = 4444;

    void Start()
    {
        running = true;
        receiveThread = new Thread(ReceiveData);
        receiveThread.Start();
        Debug.Log("Receiver thread started.");
    }

    void OnApplicationQuit()
    {
        running = false;
        udpClient?.Close();
        receiveThread?.Abort();
        Debug.Log("Application quitting. UDP client closed and receive thread aborted.");
    }

    private void ReceiveData()
    {
        try
        {
            Debug.Log("Initializing UdpClient...");
            udpClient = new UdpClient();
            //udpClient.MulticastLoopback = true;

            // Bind to the specified port
            udpClient.Client.Bind(new IPEndPoint(ipAddress, port));
            Debug.Log($"Bound to {ipAddress}:{port}");

            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
            Debug.Log("Starting receive loop...");

            while (running)
            {
                try
                {
                    byte[] receivedData = udpClient.Receive(ref remoteEndPoint);
                    string receivedString = Encoding.ASCII.GetString(receivedData);
                    Debug.Log($"Received raw data (hex): {BitConverter.ToString(receivedData)}");
                    Debug.Log($"Received from {remoteEndPoint}: {receivedString}");
                }
                catch (SocketException ex)
                {
                    if (ex.SocketErrorCode == SocketError.Interrupted)
                    {
                        Debug.Log("Receive operation was interrupted.");
                        break;
                    }
                    Debug.LogError($"SocketException: {ex.Message}");
                }
                catch (ObjectDisposedException)
                {
                    Debug.Log("UdpClient has been closed, stopping receive loop.");
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogError($"ReceiveData Exception: {e.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Initialization Exception: {e.Message}");
        }
    }
}
