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
using UnityEngine.InputSystem;
using UnityEngine.Serialization;


public enum InputType {
    EmulationMode,
    FileMode,
    Amadeo,
}

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine), typeof(UnitsControl))]
    public class BridgeClient : MonoBehaviour {
        [SerializeField] InputType inputType = InputType.Amadeo;

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

        private float[] _forces = new float[5];
        private readonly float[] _zeroForces = new float[5]; // Store zeroing forces

        private float[] _emulationForces = new float[5]; // Store forces from emulation file

        // TODO: Check if still need to use this
        private bool _isLeftHand = false;

        // Set Input System to listen on buttons {'y', 'u', 'i', 'o', 'p'}

        [SerializeField] InputAction Finger1 = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction Finger2  = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction Finger3  = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction Finger4  = new InputAction(type: InputActionType.Button);
        [SerializeField] InputAction Finger5  = new InputAction(type: InputActionType.Button);

        void Update() {
            if (_useInputSystem) {
                
                _forces[0] += Finger1.ReadValue<float>()*_emulationSpeed*Time.deltaTime;
                _forces[1] += Finger2.ReadValue<float>()*_emulationSpeed*Time.deltaTime;
                _forces[2] += Finger3.ReadValue<float>()*_emulationSpeed*Time.deltaTime;
                _forces[3] += Finger4.ReadValue<float>()*_emulationSpeed*Time.deltaTime;
                _forces[4] += Finger5.ReadValue<float>()*_emulationSpeed*Time.deltaTime;
                
                // if (Input.GetKeyDown(Finger1)) {
                //     Debug.Log("Key Y Pressed");
                //     _forces[0] += -0.1f;
                // }
                // else {
                //     _forces[0] = 0;
                // }
                //
                // if (Input.GetKeyDown(Finger2)) {
                //     Debug.Log("Key U Pressed");
                //     _forces[1] += -0.1f;
                // }
                // else {
                //     _forces[1] = 0;
                // }
                //
                // if (Input.GetKeyDown(Finger3)) {
                //     Debug.Log("Key I Pressed");
                //     _forces[2] += -0.1f;
                // }
                // else {
                //     _forces[2] = 0;
                // }
                //
                // if (Input.GetKeyDown(Finger4)) {
                //     Debug.Log("Key O Pressed");
                //     _forces[3] += -0.1f;
                // }
                // else {
                //     _forces[3] = 0;
                // }
                //
                // if (Input.GetKeyDown(Finger5)) {
                //     Debug.Log("Key P Pressed");
                //     _forces[4] += -0.1f;
                // }
                // else {
                //     _forces[4] = 0;
                // }
                Debug.Log( "Input System Forces: " + string.Join(", ", _forces));
                BridgeEvents.ForcesUpdated?.Invoke(_forces);
            }
        }


        private void OnEnable() {
            // Enable the input system
            Finger1.Enable();
            Finger2.Enable();
            Finger3.Enable();
            Finger4.Enable();
            Finger5.Enable();

            
            
            BridgeEvents.InZeroFState += OnZeroFState;
            BridgeEvents.InGameState += OnInGameState;

            BridgeEvents.BridgeCollapsingState += OnBridgeCollapsingState;
            BridgeEvents.BridgeCompletingState += OnBridgeCompletingState;
        }

        private void OnDisable() {
            // Disable the input system
            Finger1.Disable();
            Finger2.Disable();
            Finger3.Disable();
            Finger4.Disable();
            Finger5.Disable();
            
            
            
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

            if (inputType is InputType.EmulationMode) {
                if (_debug) {
                    Debug.Log("BridgeClient is in ZeroF state. Starting zeroing forces.");
                    
                }
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
            if (inputType == InputType.FileMode) {
                if (_debug) {
                    Debug.Log("StartReceiveData :: FileMode mode is true. Listening to data from demo file...");
                }

                HandleIncomingDataFileMode(_cancellationTokenSource.Token);
            }
            else if (inputType is InputType.EmulationMode) {
                if (_debug) {
                    Debug.Log("StartReceiveData :: Emulation mode is true. Starting emulation data.");
                }

                _useInputSystem = true;
            }
            else {
                if (_debug) {
                    Debug.Log("StartReceiveData :: Amadeo mode is true. Listening to data from Amadeo device...");
                }

                ReceiveDataAmadeo(_cancellationTokenSource.Token);
            }
        }

        private void StopReceiveData() {
            if (_debug) {
                Debug.Log("StopReceiveData :: Stopping data reception.");
            }
            _isReceiving = false;
            if (inputType is InputType.EmulationMode) {
                _useInputSystem = false;
            }
            // reset forces to zero
            _forces = new float[5];
        }

        private async void ReceiveDataAmadeo(CancellationToken cancellationToken) {
            Debug.Log("Receive Data from Amadeo");
            while (_isReceiving && !cancellationToken.IsCancellationRequested) {
                try {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    string receivedData = Encoding.ASCII.GetString(result.Buffer);
                    if (_debug) {
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


            OffsetForcesAndSend();
        }

        private void OffsetForcesAndSend() {
            // Fixing the offset of the forces
            _forces = _forces.Select((force, i) => force - _zeroForces[i]).ToArray();

            _forces = _forces.Reverse().ToArray();

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
                if (inputType is InputType.FileMode) {
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