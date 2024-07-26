using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum InputType {
    EmulationMode,
    Amadeo,
}

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine), typeof(UnitsControl))]
    public class BridgeClient : MonoBehaviour {
        [SerializeField] InputType inputType = InputType.EmulationMode;

        [SerializeField, Tooltip("Port should be 4444 for Amadeo connection"), Range(1024, 49151)]
        private int _port = 4444;

        // On emulation file, recommended to be small value because of high changes in data lines
        // On Amadeo, recommended to be around 100
        [SerializeField] private int _zeroFBuffer = 100;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isReceiving = false;
        private UdpClient _udpClient;
        private const string EmulationDataFile = "Assets/AmadeoRecords/force_data.txt";

        private const int DefaultPortNumber = 4444;


        private IPEndPoint _remoteEndPoint;

        private float[] _forces = new float[5];
        private readonly float[] _zeroForces = new float[5]; // Store zeroing forces
        private bool _isLeftHand = false;

        private void OnEnable() {
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            BridgeEvents.BridgeCollapsed += StopReceiveData;
            BridgeEvents.BridgeIsComplete += StopReceiveData;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (state == BridgeStates.InZeroF) {
                Debug.Log("BridgeClient is in ZeroF state. Starting zeroing forces.");
                StartReceiveData(zeroF: true);
            }
            else if (state == BridgeStates.InGame) {
                if (_isReceiving) {
                    Debug.LogWarning(
                        "BridgeClient is already receiving data. Ignoring request to start receiving data.");
                    return;
                }

                StartReceiveData();
            }
            else if (_isReceiving) {
                _isReceiving = false;
            }
        }

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            BridgeEvents.BridgeCollapsed -= StopReceiveData;
            BridgeEvents.BridgeIsComplete -= StopReceiveData;
        }

        private void Start() {
            // Initialize the UdpClient
            try {
                _udpClient = new UdpClient(PortValidation(_port)); // Listen for data on port (should be 4444)
                _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Placeholder for any remote endpoint
            }
            catch (Exception ex) {
                Debug.LogError($"Failed to initialize UdpClient: {ex.Message}");
                return; // Exit if UdpClient initialization fails
            }

            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void StartReceiveData(bool zeroF = false) {
            if (_cancellationTokenSource.IsCancellationRequested) {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            _isLeftHand = BridgeDataManager.IsLeftHand;
            if (zeroF) {
                SetZeroF(_cancellationTokenSource.Token);
                Debug.Log("BridgeClient :: StartReceiveData :: ZeroF is true. Starting zeroing forces.");
                return;
            }
            _isReceiving = true;
            if (inputType == InputType.EmulationMode) {
                Debug.Log("BridgeClient :: StartReceiveData :: Emulation mode is true. Starting emulation data.");
                HandleIncomingDataEmu(_cancellationTokenSource.Token);
            }
            else {
                ReceiveData(_cancellationTokenSource.Token);
            }
        }

        public void StopReceiveData() {
            _isReceiving = false;
        }

        private async void ReceiveData(CancellationToken cancellationToken) {
            while (_isReceiving && !cancellationToken.IsCancellationRequested) {
                try {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    string receivedData = Encoding.ASCII.GetString(result.Buffer);
                    HandleReceivedData(ParseDataFromAmadeo(receivedData));
                }
                catch (OperationCanceledException) {
                    Debug.Log("Data reception was canceled.");
                    break;
                }
                catch (Exception ex) {
                    Debug.LogError($"Exception in ReceiveData: {ex.Message}");
                    break;
                }
            }
        }


        private async void HandleIncomingDataEmu(CancellationToken cancellationToken) {
            try {
                string[] lines = await File.ReadAllLinesAsync(EmulationDataFile, cancellationToken);

                int index = 0;

                while (_isReceiving) {
                    string line = lines[index];
                    if (!string.IsNullOrWhiteSpace(line)) {
                        HandleReceivedData(ParseDataFromAmadeo(line));
                    }

                    index = (index + 1) % lines.Length;
                    await Task.Delay(10, cancellationToken); // Delay to allow UI updates and prevent high CPU usage
                }

                Debug.Log("HandleIncomingDataEmu: Stopped receiving data.");
            }
            catch (TaskCanceledException e) {
                Debug.Log("TaskCanceledException on state + " + BridgeStateMachine.currentState);
                Debug.Log($"Task was canceled: {e.Message}");
                throw;
            }
        }

        private void HandleReceivedData(string data) {
            if (BridgeStateMachine.currentState is not BridgeStates.InGame) {
                return;
            }

            string[] strForces = data.Split('\t');
            // Debug.Log("strForces: " + string.Join(", ", strForces));
            if (strForces.Length != 11) {
                Debug.Log("Received data does not contain exactly 11 values. Ignoring...");
                return; // Ensuring we have exactly 11 values (1 time + 10 forces)
            }

            // Parse the forces from the received data, str length is 11
            strForces.Select(str =>
                    float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture))
                .Skip(strForces.Length - 5) // Skip to the last 5 elements
                .ToArray()
                .CopyTo(_forces, 0); // Copy to test array starting at index 0

            // Apply zeroing offset
            for (var i = 0; i < _forces.Length; i++) {
                //The goal of zeroing is to remove the baseline effect from the measurements
                _forces[i] -= _zeroForces[i];
            }

            _forces = _forces.Select((force, i) => force - _zeroForces[i]).ToArray();

            if (!_isLeftHand) {
                _forces = _forces.Reverse().ToArray();
            }

            // Debug.Log("Invoking Forces: " + string.Join(", ", _forces));
            // Send the parsed forces to the bridgeApi script
            BridgeEvents.ForcesUpdated?.Invoke(_forces);
        }

        private void SetZeroForces() {
            var data = PlayerPrefs.GetString("zeroForces", ""); // Default to empty string if not set
            if (string.IsNullOrEmpty(data)) {
                Debug.LogError("ZeroForces data is empty or not set in PlayerPrefs");
                return;
            }

            string[] forces = data.Split('\t');
            if (forces.Length != 10) {
                Debug.LogError("ZeroForces data does not contain exactly 10 values");
                return;
            }

            for (int i = 0; i < _zeroForces.Length; i++) {
                if (float.TryParse(forces[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var force)) {
                    _zeroForces[i] = force;
                }
                else {
                    Debug.LogError($"Error parsing zero force at position {i + 1}: {forces[i + 1]}");
                    _zeroForces[i] = 0; // or any other default/fallback value
                }
            }
        }

        private static string ParseDataFromAmadeo(string data) {
            return data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        }

        private int PortValidation(int portNumber) {
            if (portNumber < 1024 || portNumber > 49151) {
                Debug.Log(
                    "Invalid port number. Port number must be between 1024 and 49151, using default port number " +
                    DefaultPortNumber);
                return DefaultPortNumber;
            }

            return portNumber;
        }

        private void OnApplicationQuit() {
            StopClientConnection();
        }

        private void StopClientConnection() {
            _isReceiving = false;
            if (_udpClient != null) {
                _udpClient.Close();
                _udpClient.Dispose();
                _udpClient = null;
            }

            if (_cancellationTokenSource != null) {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        private async void SetZeroF(CancellationToken cancellationToken) {
            Debug.Log("SetZeroF() :: Starting zeroing forces.");

            var index = 0;
            int numOfLinesToRead = _zeroFBuffer;
            string[] lines = new string[numOfLinesToRead];
            try {
                if (inputType is InputType.EmulationMode) {
                    // lines = File.ReadAllLines(EmulationDataFile);
                    // lines = await File.ReadAllLinesAsync(EmulationDataFile, cancellationToken);

                    // read the only 100 first lines from file, and add them only if it is not an empty line
                    using (StreamReader reader = new StreamReader(EmulationDataFile)) {
                        string line;
                        while ((line = reader.ReadLine()) != null && index < numOfLinesToRead) {
                            if (!string.IsNullOrWhiteSpace(line)) {
                                lines[index] = line;
                                index++;
                            }
                        }
                    }

                    if (lines.Length == 0) {
                        Debug.LogError("No data found in the emulation file.");
                        return;
                    }
                }
                else {
                    int i = 0;
                    while (i < numOfLinesToRead && !cancellationToken.IsCancellationRequested) {
                        // Receive data asynchronously
                        UdpReceiveResult result = await _udpClient.ReceiveAsync();
                        string receivedData = Encoding.ASCII.GetString(result.Buffer);

                        // Handle the received data
                        HandleReceivedData(ParseDataFromAmadeo(receivedData));

                        // Store the received data into the lines array
                        lines[i] = receivedData;
                        i++;
                    }

                    if (i < numOfLinesToRead) {
                        Debug.LogError("Not enough data received from Amadeo device.");
                    }
                }

                // apply ParseDataFromAmadeo to lines
                string[] parsedData = lines.Select(ParseDataFromAmadeo).ToArray();
                CalculateZeroingForces(parsedData);

                Debug.Log("Zeroing completed and data sent to client.");
                BridgeEvents.ZeroingCompleted?.Invoke();
            }
            catch (OperationCanceledException) {
                Debug.Log("Data reception was canceled.");
            }
            catch (Exception ex) {
                Debug.LogError($"Exception in ReceiveData: {ex.Message}");
            }
        }

        private void CalculateZeroingForces(string[] lines) {
            float[] sums = new float[5];
            int count = 0;
            Debug.Log("lines: " + lines.Length);

            // get the last 5 elements of lines

            // Transform each line correctly into a string array (split by space for example)
            // Transform each line correctly into a string array (split by space for example)
            String[][] allLines = lines
                .Select(line => line.Replace(",", ".").Split('\t')) // Split by space or any delimiter you have
                .ToArray();
            string[][] relevantForces = allLines.Select(line => line.Skip(line.Length - 5).ToArray()).ToArray();

            foreach (var line in relevantForces) {
                if (line.Length == 5) {
                    // Correct the condition to match the intended check
                    for (int i = 0; i < 5; i++) {
                        // Adjust the loop to match zero-based index
                        if (float.TryParse(line[i], NumberStyles.Float, CultureInfo.InvariantCulture,
                                out float value)) {
                            Debug.Log("line[i]: " + line[i] + " parsed value: " + value);
                            sums[i] += value;
                        }
                    }

                    count++;
                }
                else {
                    Debug.Log("line: " + string.Join(", ", line));
                }
            }

            Debug.Log(
                "CalculatingZeroingForces: Calculated sums. Count: " + count + " Sums: " + string.Join(", ", sums));
            float[] means = new float[5];
            for (int i = 0; i < sums.Length; i++) {
                _zeroForces[i] = sums[i] / count;
            }
        }
    }
}