using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System;
using System.Linq;
using BridgePackage;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine;

public class UDP_server {
    private static UdpClient _udpServer; // UDP server
    private static IPEndPoint _remoteEndPoint;
    
    public static void OpenConnection() {
        // Start the server
        StartServer();
        //
        // Handle the incoming data
        HandleIncomingData();
        //
        // Console.ReadLine();
    }

    private static void StartServer() {
        // Create the server
        _udpServer = new UdpClient(4444); // Receive data from Amadeo device on port 4444
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Sender's remote endpoint
        Console.WriteLine("Server started. Listening for data from Amadeo device...");
        Debug.Log("UDP server started. Listening for data from Amadeo device...");

    }

    private static async void HandleIncomingData() {
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
            // Get the next line from the sample data
            var line = lines[index];
            index = (index + 1) % lines.Length; // Loop back to the beginning if reached the end

            // Parse the fake data
            var parsedData = ParseDataFromAmadeo(line);

            // Send the parsed data to the Unity game client
            var sendData = Encoding.ASCII.GetBytes(parsedData);
            _udpServer.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Loopback, 8888));

            // Simulate delay
            //await Task.Delay(100); // Adjust delay as necessary
        }
    }

    private static string ParseDataFromAmadeo(string data) {
        // Remove the <Amadeo> and </Amadeo> tags
        var cleanedData = data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");

        // // Split the data by tab characters
        // var values = cleanedData.Split('\t');
        //
        // // Extract the last five values
        // var extractedValues = values.Skip(values.Length - 5).ToArray();
        //
        // // Join the extracted values back into a single string separated by tabs
        // var result = string.Join("\t", extractedValues);

        return cleanedData;
    }
}