using System;
using UnityEngine;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine))]
    public class BridgeAPI : MonoBehaviour, IBridgeAPI {
        public static event Action BridgeReady;
        public static event Action OnGameStart;
        public static event Action BridgeCollapsed;
        public static event Action BridgeIsComplete;

        private BridgeStateMachine bridgeStateMachine;


        internal static void NotifyBridgeReady() => BridgeReady?.Invoke();


        internal static void NotifyGameStart() => OnGameStart?.Invoke();


        internal static void NotifyBridgeCollapsed() => BridgeCollapsed?.Invoke();


        internal static void NotifyBridgeIsComplete() => BridgeIsComplete?.Invoke();


        private void Awake() {
            bridgeStateMachine = GetComponent<BridgeStateMachine>();
        }

        public void BuildBridge() {
            bridgeStateMachine.StartBuilding();
        }

        /* This method is for generating a bridge with different unit heights from out source.
          The unitHeights array must have 5 elements.
            The first element is the height of the first unit, the second element is the height of the second unit, and so on.
            The height of each unit must be between 0 and 5.
        */
        public void BuildBridge(int[] unitHeights, BridgeCollectionSO collectionSO = null, int bridgeTypeIndex = 0) {
            if (unitHeights.Length != 5) {
                throw new ArgumentException("unitHeights must have 5 elements.");
            }

            if (Array.Exists(unitHeights, height => height < 0 || height > 5)) {
                throw new ArgumentException("The height of each unit must be between 0 and 5.");
            }

            // if (!bridgeStateMachine.CanBuild) {
            //     return;
            // }
            // if (bridgeStateMachine.CanForceReset) {
            //     ResetBridge();
            // }
            Debug.Log("Building");
            bridgeStateMachine.StartBuilding(unitHeights, collectionSO, bridgeTypeIndex);
        }

        public void EnableGameUnits() {
            bridgeStateMachine.StartGame();
        }

        public void CollapseBridge() {
            bridgeStateMachine.StartCollapsing();
        }

        public void CompleteBridge() {
            Debug.Log("Called CompleteBridge");
            bridgeStateMachine.StartSuccess();
        }

        public void PauseBridge() {
            throw new NotImplementedException();
        }
    }
}