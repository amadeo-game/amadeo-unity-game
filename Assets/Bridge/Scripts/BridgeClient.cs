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

        [SerializeField] bool _debug = false;

        // On emulation file, recommended to be small value because of high changes in data lines
        // On Amadeo, recommended to be around 100
        [SerializeField] private int _zeroFBuffer = 100;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isReceiving = false;
        private UdpClient _udpClient;
        private const string EmulationDataFile = "Assets/AmadeoRecords/force_data.txt";

        private const int DefaultPortNumber = 4444;

        // you can use this to store and use specific endpoint
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
                _udpClient = new UdpClient(_port); // Listen for data on port (should be 4444)
                // you can use this to store and use specific endpoint
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
                Debug.Log("StartReceiveData :: Starting zeroing forces.");
                return;
            }

            _isReceiving = true;
            if (inputType == InputType.EmulationMode) {
                Debug.Log("StartReceiveData :: Emulation mode is true. Starting emulation data.");
                HandleIncomingDataEmu(_cancellationTokenSource.Token);
            }
            else {
                ReceiveDataAmadeo(_cancellationTokenSource.Token);
            }
        }

        public void StopReceiveData() {
            _isReceiving = false;
        }

        private async void ReceiveDataAmadeo(CancellationToken cancellationToken) {
            while (_isReceiving && !cancellationToken.IsCancellationRequested) {
                try {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    string receivedData = Encoding.ASCII.GetString(result.Buffer);
                    if (_debug)
                    {
                        Debug.Log($"Received data: {receivedData}");
                    }
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


            // Fixing the offset of the forces
            _forces = _forces.Select((force, i) => force - _zeroForces[i]).ToArray();

            if (!_isLeftHand) {
                _forces = _forces.Reverse().ToArray();
            }

            BridgeEvents.ForcesUpdated?.Invoke(_forces);

           
        }

        private static string ParseDataFromAmadeo(string data) {
            return data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        }

        private void OnDestroy() {
            StopClientConnection();
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
            var index = 0;
            int numOfLinesToRead = _zeroFBuffer;
            string[] lines = new string[numOfLinesToRead];
            try {
                if (inputType is InputType.EmulationMode) {
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

            // get the last 5 elements of lines
            String[][] allLines = lines
                .Select(line => line.Replace(",", ".").Split('\t')) // Split by space or any delimiter you have
                .ToArray();
            string[][] relevantForces = allLines.Select(line => line.Skip(line.Length - 5).ToArray()).ToArray();

            foreach (var line in relevantForces) {
                if (line.Length == 5) {
                    // Correct the condition to match the intended check
                    for (int i = 0; i < 5; i++) {
                        // Adjust the loop to match zero-based index
                        if (!float.TryParse(line[i], NumberStyles.Float, CultureInfo.InvariantCulture,
                                out float value)) continue;
                        // Successfully parsed the value, add it to the sum
                        sums[i] += value;
                    }

                    count++;
                }
            }

            for (int i = 0; i < sums.Length; i++) {
                _zeroForces[i] = sums[i] / count;
            }

            BridgeEvents.ZeroingCompleted?.Invoke();
        }
    }
}