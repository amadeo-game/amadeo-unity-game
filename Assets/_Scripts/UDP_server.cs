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
    private bool _isZeroing = false;
    private bool canRunServer = false;
    private int portNumber;
    

    public UDPServer(int portNumber)
    {
        this.portNumber = portNumber;
    }
    

    public  void OpenConnection() {
        // Start the server on a new thread
        //assigning it a method (ServerThreadMethod) that will be executed when the thread starts running.
        _serverThread = new Thread(ServerThreadMethod);
        _serverThread.Start();
    }

    private void ServerThreadMethod() {
        try {
            StartServer();
            canRunServer = true;
            HandleIncomingData();
        } catch (Exception ex) {
            Debug.LogError($"UDP server error: {ex.Message}");
        }
    }

    private void StartServer() {
        
        // Receive data from Amadeo device on port 4444 
        //Change the port number if necessary
        _udpServer = new UdpClient(this.portNumber); 
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("UDP server started. Listening for data from Amadeo device...");
    }
    
    
    private  void HandleIncomingData() {
        /* comment this block when receiving data from Amadeo device !!!*/
        //     //fake data
        String record1 = "Assets/AmadeoRecords/force_data.txt";
        bool isEmulationMode = true;
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
        
        while (canRunServer) {

            //uncomment this block to receive data from Amadeo device
            //var data = _udpServer.Receive(ref _remoteEndPoint); // Receive data from Amadeo device
            //string line = Encoding.ASCII.GetString(data);   // Convert the data to a string
            
            /* comment this block when receiving data from Amadeo device !!!*/
            //fake data
            var line = lines[index];
            index = (index + 1) % lines.Length; // Loop back to the beginning if reached the end

              var parsedData = ParseDataFromAmadeo(line);
              
              
            if (_isZeroing)
            {
                var zeroingData = CalculateZeroingForces(lines, isEmulationMode);
                SendDataToClient(zeroingData);
                _isZeroing = false;
            }
            
            
             // Send the parsed data to the Unity game client
            SendDataToClient(parsedData);
           
           

           
        }
    }
    private string ParseDataFromAmadeo(string data) {
        var cleanedData = data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        return cleanedData;
    }
    
    private string CalculateZeroingForces(string[] lines, bool isEmulationMode) {
        
        double[] sums = new double[10]; // Sum for each of the 10 force values
        int count = 0;

        if (isEmulationMode)
        {
            for(int i = 0; i <= 100 && lines.Length >= 100; i++)
            {
                var data = ParseDataFromAmadeo(lines[i]).Split('\t');
                for (int j = 1; j<= 10; j++) {
                    if (double.TryParse(data[j], out double value)) {
                        sums[j - 1] += value;
                    }
                }
                count++;
            }
        }
        else
        {


            foreach (var line in lines)
            {
                var data = ParseDataFromAmadeo(line).Split('\t');
                for (int i = 1; i <= 10; i++)
                {
                    if (double.TryParse(data[i], out double value))
                    {
                        sums[i - 1] += value;
                    }
                }

                count++;
            }
        }

        double[] means = new double[10];
        for (int i = 0; i < sums.Length; i++) {
            means[i] = sums[i] / count;
        }

        return string.Join("\t", means);
    }
    
    public void ZeroForces() {
        _isZeroing = true;
    }
    private void SendDataToClient(string data) {
        var sendData = Encoding.ASCII.GetBytes(data);
        _udpServer.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Loopback, 8888));
    }

    public void StopServer() {
        canRunServer = false;
        // Stop the server and clean up resources
        _serverThread?.Abort(); // Abort the server thread
        _udpServer?.Close();   // Close the UDP client
    }
    
    
}