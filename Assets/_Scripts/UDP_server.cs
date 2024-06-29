using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPServer
{
    private UdpClient _udpServer;
    private IPEndPoint _remoteEndPoint;
    private Thread _serverThread; // Thread for running the UDP server
    private bool canRunServer = false;
    private int portNumber;
    private bool isZeroing;
    private bool isEmulationMode = true;
    private string emulationDataFile = "Assets/AmadeoRecords/force_data.txt";
    private bool isPlay = false;

    public UDPServer(int portNumber)
    {
        this.portNumber = portNumber;
    }


    public void OpenConnection()
    {
        // Start the server on a new thread
        //assigning it a method (ServerThreadMethod) that will be executed when the thread starts running.
        if (_serverThread == null || !_serverThread.IsAlive)
        {
            _serverThread = new Thread(ServerThreadMethod);
            _serverThread.Start();
        }
        // _serverThread = new Thread(ServerThreadMethod);
        // _serverThread.Start();
    }

    private void ServerThreadMethod()
    {
        try
        {
            StartServer();
            canRunServer = true;


            HandleIncomingData();
        }
        catch (Exception ex)
        {
            Debug.LogError($"UDP server error: {ex.Message}");
        }
    }

    private void StartServer()
    {
        Debug.Log("port number is: " + this.portNumber);
        // Receive data from Amadeo device on given port 
        _udpServer = new UdpClient(portNumber);
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
        Debug.Log("UDP server started. Listening for data from Amadeo device...");
    }

  

    private void HandleIncomingData()
    {
        var index = 0;
        string[] lines = null;

        if (isEmulationMode)
        {
            lines = File.ReadAllLines(emulationDataFile);

            if (lines == null || lines.Length == 0)
            {
                Debug.LogError("No data found in the emulation file.");
                return;
            }
        }

        while (canRunServer)
        {
            string line;

            if (isEmulationMode)
            {
                line = lines[index];
                index = (index + 1) % lines.Length;
            }
            else
            {
                var data = _udpServer.Receive(ref _remoteEndPoint);
                line = Encoding.ASCII.GetString(data);
                Debug.Log($"Received data: {line} from {_remoteEndPoint}");
            }

            var parsedData = ParseDataFromAmadeo(line);

            if (isZeroing)
            {
                var zeroingData = CalculateZeroingForces(lines);
                Debug.Log("zeroingData:  " + zeroingData);
                SendDataToClient(zeroingData);
                isZeroing = false;
                Debug.Log("Zeroing completed and data sent to client.");
            }
            else if (isPlay)
            {
               
                SendDataToClient(parsedData);
            }
        }
    }

    private static string ParseDataFromAmadeo(string data)
    {
        var cleanedData = data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        return cleanedData;
    }

    private string CalculateZeroingForces(string[] lines)
    {
        double[] sums = new double[10];
        int count = 0;
        Debug.Log("lines" + lines.Length);

        if (isEmulationMode)
        {
            for (int i = 0; i < lines.Length && i < 100; i++)
            {
                if (lines[i].Length != 0)
                {
                    var data = ParseDataFromAmadeo(lines[i]).Split('\t');
                    for (int j = 1; j <= 10; j++)
                    {
                        if (double.TryParse(data[j].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture,
                                out double value))
                        {
                            // Debug.Log(value);
                            sums[j - 1] += value;
                        }
                    }

                    count++;
                }
            }
        }
        else
        {
            foreach (var line in lines)
            {
                if (line.Length != 0)
                {
                    var data = ParseDataFromAmadeo(line).Split('\t');
                    for (int i = 1; i <= 10; i++)
                    {
                        if (double.TryParse(data[i].Replace(",", "."), NumberStyles.Float, CultureInfo.InvariantCulture,
                                out double value))
                        {
                            sums[i - 1] += value;
                        }
                    }

                    count++;
                }
            }
        }

        double[] means = new double[10];
        for (int i = 0; i < sums.Length; i++)
        {
            means[i] = sums[i] / count;
        }

        Debug.Log("finished zeroing forces...");

        return string.Join("\t", means);
    }

    public void ZeroForces()
    {
        isZeroing = true;
        Debug.Log("Zeroing forces initiated...");
    }
    
    public void StopZeroForces()
    {
        isZeroing = false;
        Debug.Log("Zeroing forces stopped...");
    }
    
    public void setIsPlay(bool value)
    {
        isPlay = value;
    }

    private void SendDataToClient(string data)
    {
        //send data to udp client at port 8888
        var sendData = Encoding.ASCII.GetBytes(data);
        _udpServer.Send(sendData, sendData.Length, new IPEndPoint(IPAddress.Loopback, 8888));
    }

    public void StopServer()
    {
        canRunServer = false;
        // Stop the server and clean up resources
        _serverThread?.Abort(); // Abort the server thread
        _udpServer?.Close(); // Close the UDP client
    }
}