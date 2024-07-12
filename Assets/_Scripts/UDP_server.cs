using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum InputType {
    EmulationMode,
    Amadeo,
}

public class UDPServer {
    readonly int defaultPortNumber = 4444;
    private UdpClient _udpServer;
    private IPEndPoint _remoteEndPoint;
    private Thread _serverThread; // Thread for running the UDP server
    private bool canRunServer = false;
    private int portNumber;
    IPAddress ipAddress = IPAddress.Parse("10.100.4.30");

    private InputType inputType;
    private string emulationDataFile = "Assets/AmadeoRecords/force_data.txt";

    public UDPServer(int portNumber, InputType inputType = InputType.EmulationMode) {
        // check the port number is valid
        
        if (!IsPortValid(portNumber)) {
            portNumber = defaultPortNumber;
        }
        else {
            this.portNumber = portNumber;
        }
        this.inputType = inputType;
    }

    private bool IsPortValid(int portNumber) {
        if (portNumber < 1024 || portNumber > 49151) {
            Debug.Log("Invalid port number. Port number must be between 1024 and 49151, using default port number " + defaultPortNumber);
            return false;
        }

        return true;
    }

    public void CheckUpdatedPort(int portNumber) {
        Debug.Log("Checking updated port number: " + portNumber);
        if (IsPortValid(this.portNumber)) {
            StopServer();
            this.portNumber = portNumber;
            OpenConnection();
        }

    }


    public bool OpenConnection() {
        try {
            StartServer();
            canRunServer = true;
            Debug.Log("canRunServer: " + canRunServer);
        }
        catch (Exception ex) {
            Debug.LogError($"UDP server error: {ex.Message}");
        }


        return true;
    }


    public void StartListeningForGame() {
        if (_serverThread == null || !_serverThread.IsAlive) {
            _serverThread = new Thread(StartListeningBasedOnInput);
            _serverThread.Start();
        }
    }


    public void StopListeningForGame() {
        canRunServer = false;
        // _serverThread?.Abort();
    }

    private void StartListeningBasedOnInput() {
        if (inputType is InputType.EmulationMode) {
            HandleIncomingDataEmu();
        }
        else {
            HandleIncomingDataAmadeo();
        }

        Debug.Log("Stopped listening for game data.");
    }

    private void StartServer() {
        Debug.Log("port number is: " + this.portNumber);
        // Receive data from Amadeo device on given port 
        _udpServer = new UdpClient();
        //_udpServer.MulticastLoopback = true;
        _udpServer.Client.Bind(new IPEndPoint(ipAddress, portNumber));
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, portNumber);

        //_remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("UDP server started. Listening for data from Amadeo device...");
    }

    private void HandleIncomingDataEmu() {
        var index = 0;
        string[] lines = null;


        lines = File.ReadAllLines(emulationDataFile);

        if (lines == null || lines.Length == 0) {
            Debug.LogError("No data found in the emulation file.");
            return;
        }


        while (canRunServer) {
            string line;
            line = lines[index];
            index = (index + 1) % lines.Length;
            var parsedData = ParseDataFromAmadeo(line);
            // Send Data to client
            SendDataToClient(parsedData);
        }
    }

    private void HandleIncomingDataAmadeo() {
        var index = 0;
        string[] lines = null;

        while (canRunServer) {
            string line;
            //IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, portNumber);
            Debug.Log($"HandleIncomingDataAmadeo:: Receiving data... from {_remoteEndPoint}");

            var data = _udpServer.Receive(ref _remoteEndPoint);
            line = Encoding.ASCII.GetString(data);
            Debug.Log($"Received data: {line} from {_remoteEndPoint}");


            var parsedData = ParseDataFromAmadeo(line);


            SendDataToClient(parsedData);
        }
    }


    private static string ParseDataFromAmadeo(string data) {
        var cleanedData = data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        return cleanedData;
    }

    public void StartZeroF() {
        // Create a new thread and call HandleZeroF
        if (_serverThread == null || !_serverThread.IsAlive) {
            _serverThread = new Thread(HandleZeroF);
            _serverThread.Start();
        }
    }

    private void HandleZeroF() {
        var index = 0;
        int numOfLinesToRead = 100;
        string[] lines = new string[numOfLinesToRead];

        if (inputType is InputType.EmulationMode) {
            lines = File.ReadAllLines(emulationDataFile);

            if (lines == null || lines.Length == 0) {
                Debug.LogError("No data found in the emulation file.");
                return;
            }
        }
        else {
            int i = 0;
            while (canRunServer && i < numOfLinesToRead) {
                string line = "";
                Debug.Log($"HandleZeroF() :: Getting data for ZeroF: {line} from {_remoteEndPoint}");

                var data = _udpServer.Receive(ref _remoteEndPoint);
                line = Encoding.ASCII.GetString(data);
                Debug.Log($"Received data: {line} from {_remoteEndPoint}");

                lines[i] = line;
                i++;
            }

            if (i < numOfLinesToRead) {
                Debug.LogError("Not enough data received from Amadeo device.");
            }
        }

        var zeroingData = CalculateZeroingForces(lines);
        Debug.Log("zeroingData sending to client:  " + zeroingData);
        SendDataToClient(zeroingData);
        Debug.Log("Zeroing completed and data sent to client.");
    }

    private string CalculateZeroingForces(string[] lines) {
        double[] sums = new double[10];
        int count = 0;
        Debug.Log("lines" + lines.Length);

        if (inputType is InputType.EmulationMode) {
            for (int i = 0; i < lines.Length && i < 100; i++) {
                if (lines[i].Length != 0) {
                    var data = ParseDataFromAmadeo(lines[i]).Split('\t');
                    for (int j = 1; j <= 10; j++) {
                        if (double.TryParse(data[j].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture,
                                out double value)) {
                            // Debug.Log(value);
                            sums[j - 1] += value;
                        }
                    }

                    count++;
                }
            }
        }
        else {
            foreach (var line in lines) {
                if (line.Length != 0) {
                    var data = ParseDataFromAmadeo(line).Split('\t');
                    for (int i = 1; i <= 10; i++) {
                        if (double.TryParse(data[i].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture,
                                out double value)) {
                            sums[i - 1] += value;
                        }
                    }

                    count++;
                }
            }
        }

        double[] means = new double[10];
        for (int i = 0; i < sums.Length; i++) {
            means[i] = sums[i] / count;
        }

        Debug.Log("finished zeroing forces");

        return string.Join("\t", means);
    }

    public void ZeroForces() {
        Debug.Log("UDP_Server: Zeroing forces started...");
        StartZeroF();
        Debug.Log("UDP_Server: Zeroing forces Completed");
    }

    private void SendDataToClient(string data) {
        //send data to udp client at port 8888
        var sendData = Encoding.ASCII.GetBytes(data);
        
        _udpServer.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Parse("10.100.4.30"), 8888));
    }

    public void StopServer() {
        canRunServer = false;
        // Stop the server and clean up resources
        _serverThread?.Join( 1000); // Wait for the server thread to finish (max 1 second)
        _serverThread?.Abort(); // Abort the server thread
        _udpServer?.Close(); // Close the UDP client
        Debug.Log("UDP server stopped.");
    }
}