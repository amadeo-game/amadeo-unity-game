using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    private UdpClient udpClient;
    private Task receiveTask;
    private bool canReceive = true;
    private int localPort = 4444; // The port your robot sends data to

    void Start()
    {
        udpClient = new UdpClient(localPort); // Bind to the local port to listen for incoming data
        receiveTask = Task.Run(() => ReceiveData());
    }

    void OnDestroy()
    {
        canReceive = false; // Stop the receive loop
        receiveTask.Wait(); // Wait for the receive task to finish
        udpClient.Close();
        
    }

    private async void ReceiveData()
    {
        // IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, localPort); // Listen on all network interfaces
        Debug.Log("Listening for data on port " + localPort + "...");

        while (canReceive)
        {
            try
            {
                UdpReceiveResult result = await udpClient.ReceiveAsync();
                string receivedText = Encoding.ASCII.GetString(result.Buffer);
                Debug.Log("Received data from " + result.RemoteEndPoint.ToString() + ": " + receivedText);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }
    }
}