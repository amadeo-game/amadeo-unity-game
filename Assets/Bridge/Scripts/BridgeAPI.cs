using System;
using UnityEngine;
namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine))]
    public class BridgeAPI : MonoBehaviour, IBridgeAPI {



        private BridgeStateMachine bridgeStateMachine;
        private UnitsControl unitsControl;

        private static BridgeAPI instance;

        private void Awake() {
            if (instance == null) {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else {
                Destroy(gameObject);
                return;
            }

            bridgeStateMachine = GetComponent<BridgeStateMachine>();
            unitsControl = GetComponent<UnitsControl>();
        }

        public void BuildBridge() {
            if (BridgeDataManager.Heights.Length != 5) {
                throw new ArgumentException("unitHeights must have 5 elements.");
            }

            if (Array.Exists(BridgeDataManager.Heights, height => height < 0 || height > 5)) {
                throw new ArgumentException("The height of each unit must be between 0 and 5.");
            }
            bridgeStateMachine.StartBuilding();
            Debug.Log("BridgeAPI: BuildBridge called");
        }

        public void EnableGameUnits(bool doZeroF) {
            Debug.Log("BridgeAPI: EnableGameUnits called, ZeroF: " + doZeroF);
            bridgeStateMachine.ChangeState(doZeroF ? BridgeStates.InZeroF : BridgeStates.InGame);
        }

        public void CollapseBridge() {
            bridgeStateMachine.ChangeState(BridgeStates.BridgeCollapsing);
        }

        public void CompleteBridge() {
            Debug.Log("Called CompleteBridge");
            bridgeStateMachine.ChangeState(BridgeStates.BridgeCompleting);
        }

        public void PauseBridge() {
            throw new NotImplementedException();
        }

        public SessionData GetSessionData() {
            throw new NotImplementedException();
        }
    }
    
    public struct SessionData
    {
        public int[] heights;
        public float[] highestYPositions;
        public bool success;
    }
}