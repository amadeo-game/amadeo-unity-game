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
using UnityEngine.Serialization;

public enum InputType {
    EmulationMode,
    Amadeo,
}

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine), typeof(UnitsControl))]
    public class BridgeClient : MonoBehaviour {
        [SerializeField] InputType inputType = InputType.EmulationMode;

        [SerializeField, Tooltip("Port should be 4444 for Amadeo connection")]
        private int _port = 4444;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isReceiving = false;
        private UdpClient _udpClient;
        private const string EmulationDataFile = "Assets/AmadeoRecords/force_data.txt";

        private const int DefaultPortNumber = 4444;


        private IPEndPoint _remoteEndPoint;

        private float[] forces = new float[5];
        private float[] _zeroForces = new float[10]; // Store zeroing forces
        private double[] mvcForceExtension = new double[5]; // Store MVC forces
        private double[] mvcForceFlexion = new double[5]; // Store MVC forces
        bool isLeftHand = false;
        bool isFlexion = false;

        private void OnEnable() {
            BridgeEvents.BridgeStateChanged += OnBridgeStateChanged;
            // BridgeEvents.BridgeReady += StartReceiveData;
            BridgeEvents.BridgeCollapsed += StopReceiveData;
            BridgeEvents.BridgeIsComplete += StopReceiveData;
        }

        private void OnBridgeStateChanged(BridgeStates state) {
            if (state == BridgeStates.InZeroF) {
                SetZeroForces();
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
                StopReceiveData();
            }
        }

        private void OnDisable() {
            BridgeEvents.BridgeStateChanged -= OnBridgeStateChanged;
            // BridgeEvents.BridgeReady -= StartReceiveData;
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

        public void StartReceiveData() {
            if (_cancellationTokenSource.IsCancellationRequested) {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            _isReceiving = true;
            isLeftHand = BridgeDataManager.IsLeftHand;
            if (inputType == InputType.EmulationMode) {
                Task.Run(() => HandleIncomingDataEmu(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
            else {
                Task.Run(() => ReceiveData(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
            }
        }

        public void StopReceiveData() {
            _isReceiving = false;
            _cancellationTokenSource.Cancel();
        }

        private async Task ReceiveData(CancellationToken cancellationToken) {
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


        private async Task HandleIncomingDataEmu(CancellationToken cancellationToken) {
            string[] lines = await File.ReadAllLinesAsync(EmulationDataFile);
            
            int index = 0;

            while (_isReceiving) {
                // Debug.Log("HandleIncomingDataEmu: Receiving data... from Emulation file.");
                string line = lines[index];
                index = (index + 1) % lines.Length;
                HandleReceivedData(ParseDataFromAmadeo(line));
                // _isReceiving = false;
                await Task.Delay(100, cancellationToken); // Delay to allow UI updates and prevent high CPU usage
            }

            Debug.Log("HandleIncomingDataEmu: Stopped receiving data.");
        }

        private void HandleReceivedData(string data) {
            if (BridgeStateMachine.currentState is not BridgeStates.InGame) {
                return;
            }

            string[] strForces = data.Split('\t');
            // Debug.Log("strForces: " + string.Join(", ", strForces));
            if (strForces.Length != 11) {
                // Debug.Log("Received data does not contain exactly 11 values. Ignoring...");
                return; // Ensuring we have exactly 11 values (1 time + 10 forces)
            }

            // Debug.Log("Forces: " + string.Join(", ", forces));
            // Parse the forces from the received data, str length is 11
            strForces.Select(str =>
                    float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture))
                .Skip(strForces.Length - 5) // Skip to the last 5 elements
                .ToArray()
                .CopyTo(forces, 0); // Copy to test array starting at index 0

            // Apply zeroing offset
            for (var i = 0; i < forces.Length; i++) {
                //The goal of zeroing is to remove the baseline effect from the measurements
                forces[i] -= _zeroForces[i];
            }

            forces = forces.Select((force, i) => force - _zeroForces[i]).ToArray();

            if (!isLeftHand) {
                forces = forces.Reverse().ToArray();
            }

            // Send the parsed forces to the bridgeApi script
            BridgeEvents.ForcesUpdated?.Invoke(forces);
            // Debug.Log("Forces applied to UnitsControl");
        }

        private double[] ApplyMvcForces(double[] forcesNum) {
            // Normalize forces using MVC values
            var normalizedForces = new double[10];

            for (var i = 0; i < 5; i++) {
                if (isFlexion) {
                    // Left hand mvc forces
                    normalizedForces[i] = mvcForceFlexion[i] != 0 ? forcesNum[i] / mvcForceFlexion[i] : 0;
                    // Right hand mvc forces
                    normalizedForces[i + 5] = mvcForceFlexion[i] != 0 ? forcesNum[i + 5] / mvcForceFlexion[i] : 0;
                }
                else {
                    // Left hand mvc forces
                    normalizedForces[i] = mvcForceExtension[i] != 0 ? forcesNum[i] / mvcForceExtension[i] : 0;
                    // Right hand mvc forces
                    normalizedForces[i + 5] = mvcForceExtension[i] != 0 ? forcesNum[i + 5] / mvcForceExtension[i] : 0;
                }
            }

            Debug.Log("Normalized forces: " + string.Join(", ", normalizedForces));
            return normalizedForces;
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

        private void SetMvcForces() {
            for (var i = 1; i <= mvcForceExtension.Length; i++) {
                var key = "E" + i;
                double forceFinger = PlayerPrefs.GetFloat(key);
                mvcForceExtension[i - 1] = forceFinger;
            }

            for (var i = 1; i <= mvcForceFlexion.Length; i++) {
                var key = "F" + i;
                double forceFinger = PlayerPrefs.GetFloat(key);
                mvcForceFlexion[i - 1] = forceFinger;
            }

            Debug.Log(string.Join(", ", mvcForceExtension));
            Debug.Log(string.Join(", ", mvcForceFlexion));
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
    }
}