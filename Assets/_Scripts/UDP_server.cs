using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPServer {
    private UdpClient _udpServer;
    private IPEndPoint _remoteEndPoint;
    private Thread _serverThread; // Thread for running the UDP server

    public  void OpenConnection() {
        // Start the server on a new thread
        _serverThread = new Thread(ServerThreadMethod);
        _serverThread.Start();
    }

    private void ServerThreadMethod() {
        try {
            StartServer();
            HandleIncomingData();
        } catch (Exception ex) {
            Debug.LogError($"UDP server error: {ex.Message}");
        }
    }

    private void StartServer() {
        // Receive data from Amadeo device on port 4444 
        //Change the port number if necessary
        _udpServer = new UdpClient(4444); 
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("UDP server started. Listening for data from Amadeo device...");
    }
    
    
    private  void HandleIncomingData() {
        /* comment this block when receiving data from Amadeo device !!!*/
        //     //fake data
        String record1 = "Assets/AmadeoRecords/force_data.txt";
        // Read the sample data from the file
        var lines = File.ReadAllLines(record1);
        if (lines == null || lines.Length == 0) {
            // Check if the file is empty
            Console.WriteLine("No data found in the file.");
            return;
        }
        Debug.Log("Data found in the file, sending to Unity client...");
        // return;
        var index = 0;
        
        while (true) {

            //uncomment this block to receive data from Amadeo device
            //var data = _udpServer.Receive(ref _remoteEndPoint); // Receive data from Amadeo device
            //string line = Encoding.ASCII.GetString(data);   // Convert the data to a string
            
            /* comment this block when receiving data from Amadeo device !!!*/
            //fake data
            var line = lines[index];
            index = (index + 1) % lines.Length; // Loop back to the beginning if reached the end

            
            var parsedData = ParseDataFromAmadeo(line);

            // Send the parsed data to the Unity game client
            var sendData = Encoding.ASCII.GetBytes(parsedData);
            _udpServer.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Loopback, 8888));

           
        }
    }
    private string ParseDataFromAmadeo(string data) {
        var cleanedData = data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        return cleanedData;
    }

    public void StopServer() {
        // Stop the server and clean up resources
        _serverThread?.Abort(); // Abort the server thread
        _udpServer?.Close();   // Close the UDP client
    }
}