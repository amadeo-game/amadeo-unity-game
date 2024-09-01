using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public enum InputType {
    EmulationMode,
    FileMode,
    Amadeo,
}

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine), typeof(UnitsControl))]
    public class BridgeClient : MonoBehaviour {
        [SerializeField] InputType _inputType = InputType.Amadeo;
        private const float MIN_FORCE = -5.0f;
        private const float MAX_FORCE = 5.0f;
        UnitsControl _unitsControl;

        // input system on button
        private bool _useInputSystem = false;


        [SerializeField, Tooltip("Port should be 4444 for Amadeo connection"), Range(1024, 49151)]
        private int _port = 4444;

        [SerializeField] bool _debug = false;

        // On emulation file, recommended to be small value because of high changes in data lines
        // On Amadeo, recommended to be around 100
        [SerializeField] private int _zeroFBuffer = 100;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isReceiving = false;
        private UdpClient _udpClient;
        private const string FileModeDataFile = "Assets/AmadeoRecords/force_data.txt";

        // Emulation Mode
        [SerializeField] float _emulationSpeed = 0.01f;

        // you can use this to store and use specific endpoint
        private IPEndPoint _remoteEndPoint;

        // NativeArrays to hold forces and zeroForces
        private NativeArray<float> _forces;
        private NativeArray<float> _zeroForces;


        private bool _dataReceived = false;

        private bool _isLeftHand = false;

        // Set Input System to listen on buttons {'y', 'u', 'i', 'o', 'p'}

        [SerializeField] InputAction _finger1 = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction _finger2 = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction _finger3 = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction _finger4 = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction _finger5 = new InputAction(type: InputActionType.Button);


        private void GetForcesFromInput() {
            if (_useInputSystem) {
                _forces[0] += _finger1.ReadValue<float>() * _emulationSpeed * Time.fixedDeltaTime;
                _forces[1] += _finger2.ReadValue<float>() * _emulationSpeed * Time.fixedDeltaTime;
                _forces[2] += _finger3.ReadValue<float>() * _emulationSpeed * Time.fixedDeltaTime;
                _forces[3] += _finger4.ReadValue<float>() * _emulationSpeed * Time.fixedDeltaTime;
                _forces[4] += _finger5.ReadValue<float>() * _emulationSpeed * Time.fixedDeltaTime;
                for (int i = 0; i < _forces.Length; i++) {
                    _forces[i] = Mathf.Clamp(_forces[i], MIN_FORCE, MAX_FORCE);
                }

                _unitsControl.OnForcesUpdated(_forces);
            }
        }


        private void FixedUpdate() {
            if (_dataReceived) {
                _unitsControl.OnForcesUpdated(_forces);
                _dataReceived = false;
            }

            GetForcesFromInput();
        }


        private void OnEnable() {
            // Enable the input system
            _finger1.Enable();
            _finger2.Enable();
            _finger3.Enable();
            _finger4.Enable();
            _finger5.Enable();


            BridgeEvents.BuildingState += () => _isLeftHand = BridgeDataManager.IsLeftHand;
            BridgeEvents.InZeroFState += OnZeroFState;
            BridgeEvents.InGameState += OnInGameState;

            BridgeEvents.BridgeCollapsingState += OnBridgeCollapsingState;
            BridgeEvents.BridgeCompletingState += OnBridgeCompletingState;
        }

        private void OnDisable() {
            // Disable the input system
            _finger1.Disable();
            _finger2.Disable();
            _finger3.Disable();
            _finger4.Disable();
            _finger5.Disable();


            BridgeEvents.BuildingState -= () => _isLeftHand = BridgeDataManager.IsLeftHand;
            BridgeEvents.InZeroFState -= OnZeroFState;
            BridgeEvents.InGameState -= OnInGameState;

            BridgeEvents.BridgeCollapsingState -= OnBridgeCollapsingState;
            BridgeEvents.BridgeCompletingState -= OnBridgeCompletingState;
        }

        private void OnBridgeCompletingState() {
            if (_debug) {
                Debug.Log("BridgeClient is in BridgeCompleting state. Stopping data reception.");
            }

            StopReceiveData();
        }

        private void OnBridgeCollapsingState() {
            if (_debug) {
                Debug.Log("BridgeClient is in BridgeCollapsing state. Stopping data reception.");
            }

            StopReceiveData();
        }


        private void OnZeroFState() {
            if (_debug) {
                Debug.Log("BridgeClient is in ZeroF state. Starting zeroing forces.");
            }

            if (_inputType is InputType.EmulationMode) {
                BridgeEvents.FinishedZeroF?.Invoke();
                return;
            }

            StartZeroF();
        }

        private void OnInGameState() {
            if (_isReceiving) {
                Debug.LogWarning(
                    "BridgeClient is already receiving data. Ignoring request to start receiving data.");
                return;
            }

            StartReceiveData();
        }


        private void Start() {
            _unitsControl = GetComponent<UnitsControl>();
            // Initialize the UdpClient
            try {
                _udpClient = new UdpClient(_port); // Listen for data on port (should be 4444)
                // you can use this to store and use specific endpoint

                _remoteEndPoint =
                    new IPEndPoint(IPAddress.Parse("10.100.4.30"), 0); // Placeholder for any remote endpoint
                // Start receiving data asynchronously
                _udpClient.BeginReceive(ReceiveDataCallback, null);
            }
            catch (Exception ex) {
                Debug.LogError($"Failed to initialize UdpClient: {ex.Message}");
                return; // Exit if UdpClient initialization fails
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _forces = new NativeArray<float>(5, Allocator.Persistent);
            _zeroForces = new NativeArray<float>(5, Allocator.Persistent);
        }

        private void StartZeroF() {
            _isLeftHand = BridgeDataManager.IsLeftHand;
            if (_cancellationTokenSource.IsCancellationRequested) {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            if (_debug) {
                Debug.Log("StartReceiveData :: Starting zeroing forces.");
            }

            SetZeroF(_cancellationTokenSource.Token);
        }

        private void StartReceiveData() {
            _isReceiving = true;

            if (_inputType == InputType.FileMode) {
                if (_debug) {
                    Debug.Log("StartReceiveData :: FileMode mode is true. Listening to data from demo file...");
                }

                HandleIncomingDataFileMode(_cancellationTokenSource.Token);
            }
            else if (_inputType is InputType.EmulationMode) {
                if (_debug) {
                    Debug.Log("StartReceiveData :: Emulation mode is true. Starting emulation data.");
                }

                _useInputSystem = true;
            }
            else {
                if (_debug) {
                    Debug.Log("StartReceiveData :: Amadeo mode is true. Listening to data from Amadeo device...");
                }
            }
        }

        private void StopReceiveData() {
            if (_debug) {
                Debug.Log("StopReceiveData :: Stopping data reception.");
            }

            _isReceiving = false;
            if (_inputType is InputType.EmulationMode) {
                _useInputSystem = false;
            }

            // Reset the forces to zero using Unity.Mathematics
            for (int i = 0; i < _forces.Length; i++) {
                _forces[i] = 0; // math.zero() returns 0
            }
        }

        private void ReceiveDataCallback(IAsyncResult ar) {
            if (_isReceiving) {
                try {
                    byte[] receivedBytes = _udpClient.EndReceive(ar, ref _remoteEndPoint);
                    string receivedData = Encoding.ASCII.GetString(receivedBytes);


                    if (_debug) {
                        Debug.Log("Receive Data from Amadeo");

                        Debug.Log($"Received data: {receivedData}");
                    }

                    HandleReceivedData(ParseDataFromAmadeo(receivedData));
                }
                catch (OperationCanceledException) {
                    Debug.Log("Data reception was canceled.");
                }
                catch (Exception ex) {
                    Debug.LogError($"Exception in ReceiveData: {ex.Message}");
                }
            }

            _udpClient.BeginReceive(ReceiveDataCallback, null);
        }


        private async void HandleIncomingDataFileMode(CancellationToken cancellationToken) {
            try {
                string[] lines = await File.ReadAllLinesAsync(FileModeDataFile, cancellationToken);

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
            if (strForces.Length != 11) {
                Debug.Log("Received data does not contain exactly 11 values. Ignoring...");
                return; // Ensuring we have exactly 11 values (1 time + 10 forces)
            }

            // Assuming strForces is a string array with at least 11 elements
            for (int i = 0; i < 5; i++) {
                // Parse the last 5 elements from strForces and assign them to _forces
                float force = float.Parse(strForces[strForces.Length - 5 + i].Replace(",", "."),
                    CultureInfo.InvariantCulture);
                _forces[i] = Mathf.Clamp(force, -5.0f, 5.0f);
            }


            OffsetForcesAndSend();
        }

        private void OffsetForcesAndSend() {
            // Ensure the NativeArrays are the same length
            int length = _forces.Length;
            if (length != _zeroForces.Length) {
                Debug.LogError("Forces and ZeroForces arrays must be the same length.");
                return;
            }

            if (!_isLeftHand) {
                // do it with reverse
                // Subtract zeroForces from forces and reverse the array in one pass
                for (int i = 0; i < length / 2; i++) {
                    // Perform the subtraction for both the forward and reverse pairs
                    float offsetValue1 = _forces[i] - _zeroForces[i];
                    float offsetValue2 = _forces[length - 1 - i] - _zeroForces[length - 1 - i];

                    // Reverse the values in place
                    _forces[i] = offsetValue2;
                    _forces[length - 1 - i] = offsetValue1;
                }
            }
            else {
                // Subtract zeroForces from forces
                for (int i = 0; i < length; i++) {
                    _forces[i] -= _zeroForces[i];
                }
            }


            // Handle the middle element for odd-length arrays
            if (length % 2 != 0) {
                int midIndex = length / 2;
                _forces[midIndex] -= _zeroForces[midIndex];
            }

            // Mark the data as received
            _dataReceived = true;

            // Example debug output
            Debug.Log("Forces processed and event triggered:");
            for (int i = 0; i < length; i++) {
                Debug.Log($"Force {i}: {_forces[i]}");
            }
        }

        private static string ParseDataFromAmadeo(string data) {
            return data.Replace("<Amadeo>", "").Replace("</Amadeo>", "");
        }

        private void OnDestroy() {
            StopClientConnection();
            // Dispose of NativeArrays to avoid memory leaks
            if (_forces.IsCreated) _forces.Dispose();
            if (_zeroForces.IsCreated) _zeroForces.Dispose();
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
                if (_inputType is InputType.FileMode) {
                    // read the only 100 first lines from file, and add them only if it is not an empty line
                    using (StreamReader reader = new StreamReader(FileModeDataFile)) {
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

            Debug.Log("Zeroing completed and data sent to client.");

            BridgeEvents.FinishedZeroF?.Invoke();
        }
    }
}