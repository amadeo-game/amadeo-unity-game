using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BridgePackage {
    [RequireComponent(typeof(BridgeStateMachine))]
    public class BridgeAPI : MonoBehaviour, IBridgeAPI {
        public static Action BridgeReady;
        public event Action OnGameStart;
        public event Action BridgeCollapsed;
        public event Action BridgeIsComplete;

        private BridgeStateMachine bridgeStateMachine;
        
        internal static void NotifyBridgeReady() {
            BridgeReady?.Invoke();
        }

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
        public void BuildBridge(int[] unitHeights, bool activateUnits = false) {
            if (unitHeights.Length != 5) {
                throw new ArgumentException("unitHeights must have 5 elements.");
            }

            if (Array.Exists(unitHeights, height => height < 0 || height > 5)) {
                throw new ArgumentException("The height of each unit must be between 0 and 5.");
            }

            bridgeStateMachine.StartBuilding(unitHeights);
        }

        public void EnableGameUnits() {
            bridgeStateMachine.FinishBuilding();
        }

        public void CollapseBridge() {
            bridgeStateMachine.StartCollapsing();
        }

        public void PauseBridge() {
            throw new NotImplementedException();
        }

        public void ResetBridge() { }
    }
}